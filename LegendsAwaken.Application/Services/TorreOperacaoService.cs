using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class TorreOperacaoService
    {
        private readonly ITorreOperacaoRepository _repo;
        private readonly ITorreRepository _torreRepo;
        private readonly IUsuarioRepository _usuarioRepo;

        public TorreOperacaoService(
            ITorreOperacaoRepository repo,
            ITorreRepository torreRepo,
            IUsuarioRepository usuarioRepo)
        {
            _repo = repo;
            _torreRepo = torreRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<int> ObterAndarAtualNumeroAsync(Guid usuarioId)
        {
            var andar = await _torreRepo.ObterAndarPorUsuarioAsync(usuarioId);
            return andar?.Numero ?? 1;
        }

        // Returns a concluded operation ready to collect, auto-concluding any finished active op.
        public async Task<TorreOperacao?> VerificarPendenteAsync(Guid usuarioId)
        {
            var ativa = await _repo.ObterAtivaAsync(usuarioId);
            if (ativa != null)
            {
                var fim = ativa.IniciadoEm.AddHours(ativa.DuracaoHoras);
                if (DateTime.UtcNow >= fim)
                {
                    ConcluirOperacao(ativa);
                    await _repo.AtualizarAsync(ativa);
                    return ativa;
                }
                return null;
            }

            return await _repo.ObterConcluidaAsync(usuarioId);
        }

        public Task<TorreOperacao?> ObterAtivaAsync(Guid usuarioId)
            => _repo.ObterAtivaAsync(usuarioId);

        public async Task<TorreOperacao> IniciarAsync(
            Guid usuarioId, int andarNumero,
            ObjetivoOperacao objetivo, PerfilRisco perfil)
        {
            var existente = await _repo.ObterAtivaAsync(usuarioId);
            if (existente != null)
            {
                existente.Status = StatusOperacao.Expirada;
                await _repo.AtualizarAsync(existente);
            }

            int duracao = objetivo == ObjetivoOperacao.FarmRecurso ? 4 : 8;
            var op = new TorreOperacao
            {
                Id          = Guid.NewGuid(),
                UsuarioId   = usuarioId,
                AndarNumero = andarNumero,
                Objetivo    = objetivo,
                PerfilRisco = perfil,
                Status      = StatusOperacao.Ativa,
                IniciadoEm  = DateTime.UtcNow,
                DuracaoHoras = duracao
            };
            await _repo.AdicionarAsync(op);
            return op;
        }

        public async Task ColetarAsync(TorreOperacao op, ulong discordUserId)
        {
            if (op.Status != StatusOperacao.Concluida) return;

            if (op.ResultadoOuro is > 0)
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(discordUserId);
                if (usuario != null)
                {
                    usuario.Moedas += op.ResultadoOuro.Value;
                    await _usuarioRepo.AtualizarAsync(usuario);
                }
            }

            op.Status = StatusOperacao.Expirada;
            await _repo.AtualizarAsync(op);
        }

        public async Task CancelarAsync(TorreOperacao op)
        {
            op.Status = StatusOperacao.Expirada;
            await _repo.AtualizarAsync(op);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static void ConcluirOperacao(TorreOperacao op)
        {
            op.Status      = StatusOperacao.Concluida;
            op.ConcluidoEm = DateTime.UtcNow;
            op.ResultadoOuro = CalcularOuro(op.AndarNumero, op.PerfilRisco, op.DuracaoHoras);
            var (nome, qtd) = CalcularRecurso(op.AndarNumero, op.PerfilRisco, op.DuracaoHoras);
            op.ResultadoRecursoNome = nome;
            op.ResultadoRecursoQtd  = nome != null ? qtd : null;
        }

        private static int CalcularOuro(int andar, PerfilRisco perfil, int horas)
        {
            double mult = perfil switch
            {
                PerfilRisco.Seguro     => 0.8,
                PerfilRisco.Balanceado => 1.0,
                PerfilRisco.Agressivo  => 1.5,
                _                      => 1.0
            };
            return (int)(andar * 3 * horas * mult);
        }

        private static (string? nome, int qtd) CalcularRecurso(int andar, PerfilRisco perfil, int horas)
        {
            string? nome = andar switch
            {
                >= 25 => "Núcleo Sombrio",
                >= 18 => "Cristal Arcano",
                >= 12 => "Essência Corrompida",
                >= 5  => "Fragmento Rústico",
                _     => null
            };
            if (nome == null) return (null, 0);

            double mult = perfil switch
            {
                PerfilRisco.Seguro     => 0.8,
                PerfilRisco.Balanceado => 1.0,
                PerfilRisco.Agressivo  => 1.5,
                _                      => 1.0
            };
            int qtd = Math.Max(1, (int)(horas / 4.0 * (1 + andar / 15.0) * mult));
            return (nome, qtd);
        }
    }
}

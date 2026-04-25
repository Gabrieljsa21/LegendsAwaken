using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LegendsAwaken.Application.Services
{
    public class TorreOperacaoService
    {
        private readonly ITorreOperacaoRepository _repo;
        private readonly ITorreRepository         _torreRepo;
        private readonly IUsuarioRepository       _usuarioRepo;

        public TorreOperacaoService(
            ITorreOperacaoRepository repo,
            ITorreRepository torreRepo,
            IUsuarioRepository usuarioRepo)
        {
            _repo      = repo;
            _torreRepo = torreRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<int> ObterAndarAtualNumeroAsync(Guid usuarioId)
        {
            var andar = await _torreRepo.ObterAndarPorUsuarioAsync(usuarioId);
            return andar?.Numero ?? 1;
        }

        // ── Query helpers ────────────────────────────────────────────────────────

        public Task<TorreOperacao?> ObterAtivaAsync(Guid usuarioId)
            => _repo.ObterAtivaAsync(usuarioId);

        public Task<List<TorreOperacao>> ListarAtivasAsync(Guid usuarioId)
            => _repo.ListarAtivasAsync(usuarioId);

        // Checks all active ops and auto-concludes finished ones; returns newly concluded list
        public async Task<List<TorreOperacao>> ProcessarTodasAsync(Guid usuarioId)
        {
            var ativas = await _repo.ListarAtivasAsync(usuarioId);
            var concluidas = new List<TorreOperacao>();
            foreach (var op in ativas)
            {
                if (DateTime.UtcNow >= op.IniciadoEm.AddHours(op.DuracaoHoras))
                {
                    ConcluirOperacao(op);
                    await _repo.AtualizarAsync(op);
                    concluidas.Add(op);
                }
            }
            return concluidas;
        }

        public Task<List<TorreOperacao>> ListarConcluidasAsync(Guid usuarioId)
            => _repo.ListarConcluidasAsync(usuarioId);

        // Legacy single-op check (used by /torre notification)
        public async Task<TorreOperacao?> VerificarPendenteAsync(Guid usuarioId)
        {
            await ProcessarTodasAsync(usuarioId);
            return await _repo.ObterConcluidaAsync(usuarioId);
        }

        // ── Start ─────────────────────────────────────────────────────────────────

        public async Task<TorreOperacao> IniciarAsync(
            Guid usuarioId, int andarNumero, IEnumerable<Construcao> construcoes)
        {
            // Slot capacity check
            var ativas = await _repo.ListarAtivasAsync(usuarioId);
            int maxSlots = TorreOperacaoConfig.CalcularMaxSlots(construcoes);
            if (ativas.Count >= maxSlots)
                throw new InvalidOperationException($"Capacidade máxima atingida ({maxSlots} operações simultâneas).");

            // Floor already running?
            var existente = await _repo.ObterPorAndarAsync(usuarioId, andarNumero);
            if (existente != null)
                throw new InvalidOperationException($"O andar {andarNumero} já tem uma operação em andamento.");

            var (recurso, quantidade, _) = TorreOperacaoConfig.ObterProducao(andarNumero);

            var op = new TorreOperacao
            {
                Id           = Guid.NewGuid(),
                UsuarioId    = usuarioId,
                AndarNumero  = andarNumero,
                Objetivo     = ObjetivoOperacao.FarmRecurso,   // legacy field — unused
                PerfilRisco  = PerfilRisco.Balanceado,          // legacy field — unused
                Status       = StatusOperacao.Ativa,
                IniciadoEm   = DateTime.UtcNow,
                DuracaoHoras = TorreOperacaoConfig.DuracaoHoras,
                ResultadoRecursoNome = recurso,
                ResultadoRecursoQtd  = quantidade
            };
            await _repo.AdicionarAsync(op);
            return op;
        }

        // ── Collect ──────────────────────────────────────────────────────────────

        public async Task<int> ColetarTodasAsync(Guid usuarioId, ulong discordUserId)
        {
            var concluidas = await _repo.ListarConcluidasAsync(usuarioId);
            if (concluidas.Count == 0) return 0;

            int ouroTotal = 0;
            foreach (var op in concluidas)
            {
                if (op.ResultadoOuro is > 0)
                    ouroTotal += op.ResultadoOuro.Value;

                op.Status = StatusOperacao.Expirada;
                await _repo.AtualizarAsync(op);
            }

            if (ouroTotal > 0)
            {
                var usuario = await _usuarioRepo.ObterPorIdAsync(discordUserId);
                if (usuario != null)
                {
                    usuario.Moedas += ouroTotal;
                    await _usuarioRepo.AtualizarAsync(usuario);
                }
            }

            return concluidas.Count;
        }

        // Legacy single-op collect (kept for backwards compat)
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

        // ── Cancel ───────────────────────────────────────────────────────────────

        public async Task CancelarAsync(TorreOperacao op)
        {
            op.Status = StatusOperacao.Expirada;
            await _repo.AtualizarAsync(op);
        }

        public async Task CancelarPorAndarAsync(Guid usuarioId, int andar)
        {
            var op = await _repo.ObterPorAndarAsync(usuarioId, andar);
            if (op == null) return;
            await CancelarAsync(op);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static void ConcluirOperacao(TorreOperacao op)
        {
            op.Status      = StatusOperacao.Concluida;
            op.ConcluidoEm = DateTime.UtcNow;

            var (recurso, quantidade, _) = TorreOperacaoConfig.ObterProducao(op.AndarNumero);

            // Ouro is a special case: store in ResultadoOuro, no recurso name
            if (recurso == "Ouro")
            {
                op.ResultadoOuro        = quantidade;
                op.ResultadoRecursoNome = null;
                op.ResultadoRecursoQtd  = null;
            }
            else
            {
                op.ResultadoOuro        = 0;
                op.ResultadoRecursoNome = recurso;
                op.ResultadoRecursoQtd  = quantidade;
            }
        }
    }
}

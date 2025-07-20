using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Discord;

namespace LegendsAwaken.Application.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Usuario> ObterOuCriarAsync(IUser discordUser)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(discordUser.Id);

            if (usuario == null)
            {
                usuario = new Usuario
                {
                    Id = discordUser.Id,
                    Nome = discordUser.Username,
                    DataCriacao = DateTime.UtcNow,
                    UltimoLogin = DateTime.UtcNow
                };

                await _usuarioRepository.AdicionarAsync(usuario);
            }
            else
            {
                usuario.UltimoLogin = DateTime.UtcNow;
                await _usuarioRepository.AtualizarAsync(usuario);
            }

            return usuario;
        }
    }
}

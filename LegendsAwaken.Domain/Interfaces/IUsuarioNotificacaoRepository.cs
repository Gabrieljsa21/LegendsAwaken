using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface IUsuarioNotificacaoRepository
{
    Task<UsuarioNotificacao?> ObterAsync(ulong usuarioId);
    Task AdicionarOuAtualizarAsync(UsuarioNotificacao notif);
}

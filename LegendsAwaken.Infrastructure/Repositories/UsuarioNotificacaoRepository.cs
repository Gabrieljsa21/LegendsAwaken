using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class UsuarioNotificacaoRepository(LegendsAwakenDbContext db) : IUsuarioNotificacaoRepository
{
    public async Task<UsuarioNotificacao?> ObterAsync(ulong usuarioId) =>
        await db.UsuariosNotificacao.FindAsync(usuarioId);

    public async Task AdicionarOuAtualizarAsync(UsuarioNotificacao notif)
    {
        var existing = await db.UsuariosNotificacao.FindAsync(notif.UsuarioId);
        if (existing == null)
            db.UsuariosNotificacao.Add(notif);
        else
        {
            existing.NotificacoesAtivas = notif.NotificacoesAtivas;
            existing.CanalPreferido = notif.CanalPreferido;
            existing.Preferencia = notif.Preferencia;
        }
        await db.SaveChangesAsync();
    }
}

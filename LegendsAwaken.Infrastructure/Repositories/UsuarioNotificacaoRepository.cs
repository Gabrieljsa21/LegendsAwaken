using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Interfaces;
using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LegendsAwaken.Infrastructure.Repositories;

public class UsuarioNotificacaoRepository : IUsuarioNotificacaoRepository
{
    private readonly LegendsAwakenDbContext _ctx;

    public UsuarioNotificacaoRepository(LegendsAwakenDbContext ctx) => _ctx = ctx;

    public async Task<UsuarioNotificacao?> ObterAsync(ulong usuarioId) =>
        await _ctx.UsuariosNotificacao.FindAsync(usuarioId);

    public async Task AdicionarOuAtualizarAsync(UsuarioNotificacao notif)
    {
        var existing = await _ctx.UsuariosNotificacao.FindAsync(notif.UsuarioId);
        if (existing == null)
            _ctx.UsuariosNotificacao.Add(notif);
        else
        {
            existing.NotificacoesAtivas = notif.NotificacoesAtivas;
            existing.CanalPreferido = notif.CanalPreferido;
            existing.Preferencia = notif.Preferencia;
            _ctx.UsuariosNotificacao.Update(existing);
        }
        await _ctx.SaveChangesAsync();
    }
}

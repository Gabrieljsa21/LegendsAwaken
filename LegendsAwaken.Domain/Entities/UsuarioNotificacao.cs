using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Domain.Entities;

public class UsuarioNotificacao
{
    public ulong UsuarioId { get; set; }
    public bool NotificacoesAtivas { get; set; } = true;
    public ulong? CanalPreferido { get; set; }
    public NotificacaoPreferencia Preferencia { get; set; } = NotificacaoPreferencia.Tudo;
}

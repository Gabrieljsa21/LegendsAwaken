using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities;

public class CidadeBoosterAtivo
{
    public Guid Id { get; set; }
    public ulong UsuarioId { get; set; }
    public TipoBoosterCidade Tipo { get; set; }
    public DateTime AtivadoEm { get; set; }
    public DateTime ExpiraEm { get; set; }
}

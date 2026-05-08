using System;

namespace LegendsAwaken.Domain.Entities;

public class TorreEventoLog
{
    public Guid Id { get; set; }
    public Guid ExploracaoId { get; set; }
    public string Texto { get; set; } = "";
    public DateTime CriadoEm { get; set; }
}

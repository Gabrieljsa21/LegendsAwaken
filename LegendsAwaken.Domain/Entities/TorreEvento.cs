using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities;

public class TorreEvento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExploracaoId { get; set; }
    public EventoStatus Status { get; set; }
    public TipoEvento Tipo { get; set; }
    public TierEvento Tier { get; set; }
    public EventoRaridade Raridade { get; set; }
    public string EventoKey { get; set; } = "";
    public int ProgressoNoCheckpoint { get; set; }
    public int AndarOrigem { get; set; }
    public int EventoSeed { get; set; }
    public int ResultadoSchemaVersion { get; set; } = 1;
    public string? OpcaoKey { get; set; }
    public string? ResultadoJson { get; set; }
    public string? SnapshotCombatStateJson { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiraEm { get; set; }
    public DateTime? ResolvidoEm { get; set; }
    public DateTime? ProcessadoEm { get; set; }
    public TorreExploracao Exploracao { get; set; } = null!;
}

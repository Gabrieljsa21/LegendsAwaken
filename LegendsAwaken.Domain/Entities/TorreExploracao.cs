using LegendsAwaken.Domain.Enum;
using System;

namespace LegendsAwaken.Domain.Entities;

public class TorreExploracao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public int AndarNumero { get; set; }
    public double Progresso { get; set; } = 0;          // 0-100
    public int UltimoCheckpoint { get; set; } = 0;      // last checkpoint % paid out
    public int CheckpointInterval { get; set; } = 25;   // % between checkpoints (default 25)
    public StatusExploracao Status { get; set; } = StatusExploracao.Ativa;
    public DateTime IniciadoEm { get; set; } = DateTime.UtcNow;
    public DateTime UltimoTickEm { get; set; } = DateTime.UtcNow;
    public string HeroisIds { get; set; } = "";         // comma-separated Guid strings
    public TipoBooster? BoosterAtivo { get; set; }
    public int LootOuro { get; set; } = 0;
    public int LootFragmentosQtd { get; set; } = 0;
    public string LootFragmentosHeroiId { get; set; } = ""; // Guid string of hero whose fragment dropped
    public DateTime? ConcluidoEm { get; set; }
    public string HeroisFeridosIds { get; set; } = "";  // comma-separated; set on Falha

    // Checkpoint event system
    public int Seed { get; set; }                                       // set on IniciarAsync, drives EventoRng
    public ulong DiscordUserId { get; set; }                            // Discord user for notifications
    public ulong ChannelId { get; set; }                                // channel where exploration started
    public CheckpointFlags CheckpointsProcessados { get; set; }         // bitmask tracking processed checkpoints
    public string? ConsequenceTags { get; set; }                        // JSON string[] — tags from chained events

    [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
    public int Version { get; set; }                                    // optimistic concurrency
}

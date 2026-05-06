using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LegendsAwaken.Application.Services;

/// <summary>
/// Lê heroes.json e faz upsert em HeroiConfigs + HeroiUnlockConfigs a cada startup.
/// Editar o JSON e reiniciar o bot é suficiente para aplicar mudanças de personagens.
/// </summary>
public class HeroiDataLoader
{
    private readonly LegendsAwakenDbContext _db;
    private readonly ILogger<HeroiDataLoader> _log;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public HeroiDataLoader(
        LegendsAwakenDbContext db,
        ILogger<HeroiDataLoader> log)
    {
        _db = db;
        _log = log;
    }

    public async Task SincronizarAsync(string heroesJsonPath)
    {
        if (!File.Exists(heroesJsonPath))
        {
            _log.LogWarning("heroes.json não encontrado em {Path}. Pulando sincronização.", heroesJsonPath);
            return;
        }

        var json = await File.ReadAllTextAsync(heroesJsonPath);
        var dtos = JsonSerializer.Deserialize<List<HeroiDto>>(json, _jsonOpts);
        if (dtos is null || dtos.Count == 0) return;

        var existingConfigs = await _db.HeroiConfigs.ToDictionaryAsync(h => h.Id);
        var existingUnlocks = await _db.HeroiUnlockConfigs.ToDictionaryAsync(u => u.HeroiId);

        foreach (var dto in dtos)
        {
            // Chave R2 derivada do ID numérico — estável mesmo após renomear o herói.
            // GUID a2000000-...-000000000001 → segmento final → int → "001"
            var numericId     = int.Parse(dto.Id.ToString().Split('-')[^1]).ToString("D3");
            var imageUrl      = $"heroes/display/{numericId}.webp";
            var imageUrlThumb = $"heroes/thumb/{numericId}.webp";

            // ── HeroiConfig (upsert) ───────────────────────────────────────
            if (existingConfigs.TryGetValue(dto.Id, out var config))
            {
                config.Nome          = dto.Nome;
                config.Titulo        = dto.Titulo;
                config.RaridadeBase  = (Raridade)dto.Raridade;
                config.Arquetipo     = dto.Arquetipo;
                config.Tag           = dto.Grupo;
                config.ImageUrl      = imageUrl;
                config.ImageUrlThumb = imageUrlThumb;
            }
            else
            {
                _db.HeroiConfigs.Add(new HeroiConfig
                {
                    Id           = dto.Id,
                    Nome         = dto.Nome,
                    Titulo       = dto.Titulo,
                    RaridadeBase = (Raridade)dto.Raridade,
                    Arquetipo    = dto.Arquetipo,
                    Tag          = dto.Grupo,
                    ImageUrl     = imageUrl,
                    ImageUrlThumb = imageUrlThumb
                });
            }

            // ── HeroiUnlockConfig (upsert) ────────────────────────────────
            var tipoUnlock = Enum.Parse<TipoUnlock>(dto.TipoUnlock, ignoreCase: true);

            if (existingUnlocks.TryGetValue(dto.Id, out var unlock))
            {
                unlock.TipoUnlock             = tipoUnlock;
                unlock.AndarMarco             = dto.AndarReferencia;
                unlock.QuantidadeFragmentos   = dto.FragmentosNecessarios;
            }
            else
            {
                _db.HeroiUnlockConfigs.Add(new HeroiUnlockConfig
                {
                    HeroiId             = dto.Id,
                    TipoUnlock          = tipoUnlock,
                    AndarMarco          = dto.AndarReferencia,
                    QuantidadeFragmentos = dto.FragmentosNecessarios
                });
            }
        }

        var saved = await _db.SaveChangesAsync();
        _log.LogInformation("HeroiDataLoader: {Count} heróis sincronizados ({Changes} alterações).", dtos.Count, saved);
    }

    // ── DTO interno ───────────────────────────────────────────────────────────
    private sealed class HeroiDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = "";
        public string? Titulo { get; set; }
        public int Raridade { get; set; }
        public Profissao Arquetipo { get; set; }
        public string? Grupo { get; set; }
        public string? ImageSlug { get; set; }
        public string TipoUnlock { get; set; } = "Fragmentos";
        public int? AndarReferencia { get; set; }
        public int? FragmentosNecessarios { get; set; }
    }
}

using Discord;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Auxiliares;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using System.Buffers.Binary;

namespace LegendsAwaken.Application.Services;

public class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IHeroiDesbloqueadoRepository desbloqueadoRepository,
    IHeroiConfigRepository heroiConfigRepository,
    HeroiService heroiService)
{
    // Kaeryn (#16), Elize (#29), Aegis (#9)
    private static readonly Guid[] HeroisIniciais =
    [
        new("a2000000-0000-0000-0000-000000000016"),
        new("a2000000-0000-0000-0000-000000000029"),
        new("a2000000-0000-0000-0000-000000000009"),
    ];

    public async Task<Usuario> ObterOuCriarAsync(IUser discordUser)
    {
        var usuario = await usuarioRepository.ObterPorIdAsync(discordUser.Id);

        if (usuario == null)
        {
            usuario = new Usuario
            {
                Id          = discordUser.Id,
                Nome        = discordUser.Username,
                DataCriacao = DateTime.UtcNow,
                UltimoLogin = DateTime.UtcNow
            };

            await usuarioRepository.AdicionarAsync(usuario);
            await DesbloquearHeroisIniciaisAsync(discordUser.Id);
        }
        else
        {
            usuario.UltimoLogin = DateTime.UtcNow;
            await usuarioRepository.AtualizarAsync(usuario);

            // Repair: ensure initial heroes have Heroi instances (one-time migration for existing accounts)
            var heroisExistentes = await heroiService.ObterHeroisPorUsuarioAsync(discordUser.Id);
            if (!heroisExistentes.Any())
                await DesbloquearHeroisIniciaisAsync(discordUser.Id);
        }

        return usuario;
    }

    private async Task DesbloquearHeroisIniciaisAsync(ulong discordId)
    {
        var usuarioGuid = DiscordToGuid(discordId);
        var heroisExistentes = await heroiService.ObterHeroisPorUsuarioAsync(discordId);

        foreach (var heroiId in HeroisIniciais)
        {
            var config = await heroiConfigRepository.ObterPorIdAsync(heroiId);
            if (config is null) continue;

            if (!await desbloqueadoRepository.JaDesbloqueadoAsync(usuarioGuid, heroiId))
            {
                await desbloqueadoRepository.SalvarAsync(new HeroiDesbloqueado
                {
                    UsuarioId     = usuarioGuid,
                    HeroiId       = heroiId,
                    Heroi         = config,
                    DesbloqueadoEm = DateTime.UtcNow
                });
            }

            // Create Heroi instance if missing
            if (!heroisExistentes.Any(h => h.Nome == config.Nome))
            {
                var novo = await heroiService.CriarHeroiAsync(
                    discordId,
                    config.Nome,
                    config.RaridadeBase,
                    RacaDeTag(config.Tag),
                    "",
                    [],
                    FuncaoDeArquetipo(config.Arquetipo),
                    config.Titulo);
                heroisExistentes.Add(novo);
            }
        }
    }

    internal static Raca RacaDeTag(string? tag) => tag switch
    {
        "Anjos Caídos"     => Raca.AnjoCaido,
        "Serafins"         => Raca.Serafim,
        "Bestiais"         => Raca.Bestial,
        "Dracossanguíneo"  => Raca.Draconato,
        "Elfo/Fada"        => Raca.Elfo,
        _                  => Raca.Humano,
    };

    internal static FuncaoTatica? FuncaoDeArquetipo(Profissao arquetipo) => arquetipo switch
    {
        Profissao.Guerreiro  => FuncaoTatica.Frente,
        Profissao.Paladino   => FuncaoTatica.Frente,
        Profissao.Arqueiro   => FuncaoTatica.LongoAlcance,
        Profissao.Mago       => FuncaoTatica.LongoAlcance,
        Profissao.Ladino     => FuncaoTatica.LongoAlcance,
        Profissao.Bardo      => FuncaoTatica.Suporte,
        Profissao.Clerigo    => FuncaoTatica.Curandeiro,
        Profissao.Invocador  => FuncaoTatica.Controle,
        _                    => null,
    };

    private static Guid DiscordToGuid(ulong discordId)
    {
        var bytes = new byte[16];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, discordId);
        return new Guid(bytes);
    }
}

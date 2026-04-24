using Discord;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsAwaken.Bot.Panels;

public static class HeroisPanel
{
    public static Embed CriarEmbed(List<Heroi> herois)
    {
        var cor = herois.Any() switch
        {
            true when herois.Any(h => h.Raridade == Raridade.Estrela5) => Color.Gold,
            true when herois.Any(h => h.Raridade == Raridade.Estrela4) => Color.Purple,
            true when herois.Any(h => h.Raridade == Raridade.Estrela3) => Color.Blue,
            _ => Color.DarkBlue
        };

        var builder = new EmbedBuilder()
            .WithTitle("⚔️ Seus Heróis")
            .WithColor(cor);

        if (!herois.Any())
        {
            builder.WithDescription("Você ainda não possui heróis.");
        }
        else
        {
            foreach (var h in herois.Take(25))
            {
                var estrelas  = new string('⭐', (int)h.Raridade);
                var profissao = h.Profissao?.ToString() ?? "—";
                var sustento  = IconeSustento(h.EstadoSustento);
                builder.AddField(
                    $"{sustento} {estrelas} {h.Nome}",
                    $"Nv **{h.Nivel}** | {h.Raca} | {profissao}",
                    inline: true);
            }

            builder.WithFooter(herois.Count > 25
                ? $"Total: {herois.Count} heróis | Exibindo os primeiros 25"
                : $"Total: {herois.Count} herói(s)");
        }

        return builder.Build();
    }

    public static MessageComponent CriarComponentes(List<Heroi> herois)
    {
        var builder = new ComponentBuilder();

        if (herois.Any())
        {
            var select = new SelectMenuBuilder()
                .WithCustomId("herois_ver")
                .WithPlaceholder("Ver detalhes de um herói...")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var h in herois.Take(25))
            {
                var estrelas = new string('⭐', (int)h.Raridade);
                select.AddOption($"{estrelas} {h.Nome} (Nv {h.Nivel})", h.Id.ToString(), h.Raca.ToString());
            }

            builder.WithSelectMenu(select);
        }

        builder.WithButton("🔄", "herois_atualizar", ButtonStyle.Secondary);
        return builder.Build();
    }

    public static Embed CriarEmbedDetalhe(Heroi heroi)
    {
        var estrelas = new string('⭐', (int)heroi.Raridade);

        var cor = heroi.Raridade switch
        {
            Raridade.Estrela5 => Color.Gold,
            Raridade.Estrela4 => Color.Purple,
            Raridade.Estrela3 => Color.Blue,
            Raridade.Estrela2 => Color.Teal,
            _                 => Color.DarkGrey
        };

        var totalAtributos = heroi.ObterAtributosTotais(new AtributosBase());
        var attrSb = new StringBuilder();
        foreach (var (attr, valor) in totalAtributos.ToEnumerable())
            attrSb.AppendLine($"{NomeAtributo(attr)}: **{valor}**");
        if (heroi.PontosAtributosDisponiveis > 0)
            attrSb.AppendLine($"⚠️ Pontos disponíveis: **{heroi.PontosAtributosDisponiveis}**");

        var s = heroi.Status;

        var embedBuilder = new EmbedBuilder()
            .WithTitle($"{estrelas} {heroi.Nome}")
            .WithColor(cor)
            .AddField("Nível",     heroi.Nivel.ToString(),                 inline: true)
            .AddField("Raça",      heroi.Raca.ToString(),                  inline: true)
            .AddField("Profissão", heroi.Profissao?.ToString() ?? "—",     inline: true)
            .AddField("Combate",
                $"❤️ {s.VidaAtual}/{s.VidaMaxima} | 💧 {s.ManaAtual}/{s.ManaMaxima}",
                inline: false)
            .AddField("Atributos", attrSb.ToString(), inline: false);

        if (heroi.Habilidades?.Any() == true)
        {
            var habsSb = new StringBuilder();
            foreach (var hh in heroi.Habilidades)
                habsSb.AppendLine($"**{hh.Habilidade.Nome}** — Nv {hh.Nivel}");
            embedBuilder.AddField("Habilidades", habsSb.ToString(), inline: false);
        }

        embedBuilder.AddField("Histórico",
            $"✅ {heroi.Vitorias} vitórias | ❌ {heroi.Derrotas} derrotas | 🗼 {heroi.AndaresConquistados} andares",
            inline: false);

        embedBuilder.AddField("Sustento",
            $"{IconeSustento(heroi.EstadoSustento)} {NomeSustento(heroi.EstadoSustento)}",
            inline: true);

        return embedBuilder.Build();
    }

    public static MessageComponent CriarComponentesDetalhe(Heroi heroi)
    {
        var label = heroi.EstadoSustento == EstadoSustento.Inativo
            ? "▶️ Ativar Sustento"
            : "⏸️ Pausar Sustento";
        return new ComponentBuilder()
            .WithButton(label, $"herois_toggle_inativo|{heroi.Id}", ButtonStyle.Secondary)
            .Build();
    }

    private static string IconeSustento(EstadoSustento estado) => estado switch
    {
        EstadoSustento.Ativo     => "✅",
        EstadoSustento.Instavel  => "⚠️",
        EstadoSustento.Degradado => "🔴",
        EstadoSustento.Inativo   => "💤",
        _                        => "❓"
    };

    private static string NomeSustento(EstadoSustento estado) => estado switch
    {
        EstadoSustento.Ativo     => "Ativo",
        EstadoSustento.Instavel  => "Instável",
        EstadoSustento.Degradado => "Degradado",
        EstadoSustento.Inativo   => "Inativo (pausado)",
        _                        => estado.ToString()
    };

    private static string NomeAtributo(Atributo attr) => attr switch
    {
        Atributo.Forca        => "Força",
        Atributo.Agilidade    => "Agilidade",
        Atributo.Vitalidade   => "Vitalidade",
        Atributo.Inteligencia => "Inteligência",
        Atributo.Percepcao    => "Percepção",
        _                     => attr.ToString()
    };
}

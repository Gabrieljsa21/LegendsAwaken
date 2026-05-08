using LegendsAwaken.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegendsAwaken.Application.Config;

public record OpcaoConfig(string Key, string TextoExibido, RiscoTom RiscoTom);

public record CheckpointEventoConfig(
    string Key,
    TipoEvento Tipo,
    TierEvento Tier,
    EventoRaridade Raridade,
    bool TemImpactoMecanico,
    string Titulo,
    string Descricao,
    OpcaoConfig[]? Opcoes,
    Pericia? Pericia,
    int? DC,
    int Peso,
    int MinAndar,
    int MaxAndar,
    string[] Tags,
    string[] Biomas,
    string[]? Requisitos,
    string[]? ConsequenceTags
);

public static class CheckpointEventoCatalog
{
    public static readonly IReadOnlyList<CheckpointEventoConfig> Todos = new List<CheckpointEventoConfig>
    {
        new(
            Key:                "encruzilhada_mercador",
            Tipo:               TipoEvento.BlockingChoice,
            Tier:               TierEvento.Maior,
            Raridade:           EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:             "Encruzilhada do Mercador",
            Descricao:          "Um mercador misterioso bloqueia o caminho, oferecendo passagem... por um preço.",
            Opcoes: new[]
            {
                new OpcaoConfig("pagar",    "Pagar o preço",         RiscoTom.Seguro),
                new OpcaoConfig("forcar",   "Forçar passagem",       RiscoTom.Arriscado),
                new OpcaoConfig("recuar",   "Recuar",                RiscoTom.Neutro)
            },
            Pericia:         null,
            DC:              null,
            Peso:            10,
            MinAndar:        1,
            MaxAndar:        15,
            Tags:            Array.Empty<string>(),
            Biomas:          Array.Empty<string>(),
            Requisitos:      null,
            ConsequenceTags: null
        ),
        new(
            Key:                "trilha_oculta",
            Tipo:               TipoEvento.BlockingChoice,
            Tier:               TierEvento.Maior,
            Raridade:           EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:             "Trilha Oculta",
            Descricao:          "Um dos heróis detecta uma passagem secreta que poderia encurtar o caminho.",
            Opcoes: new[]
            {
                new OpcaoConfig("explorar", "Explorar a passagem",           RiscoTom.Arriscado),
                new OpcaoConfig("ignorar",  "Continuar pela rota principal",  RiscoTom.Seguro)
            },
            Pericia:         null,
            DC:              null,
            Peso:            10,
            MinAndar:        5,
            MaxAndar:        15,
            Tags:            Array.Empty<string>(),
            Biomas:          Array.Empty<string>(),
            Requisitos:      null,
            ConsequenceTags: new[] { "trilha_aberta" }
        ),
        new(
            Key:                "chuva_de_fragmentos",
            Tipo:               TipoEvento.Reward,
            Tier:               TierEvento.Menor,
            Raridade:           EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:             "Câmara Abandonada",
            Descricao:          "A party encontra restos de uma câmara saqueada com alguns fragmentos deixados para trás.",
            Opcoes:          null,
            Pericia:         null,
            DC:              null,
            Peso:            10,
            MinAndar:        1,
            MaxAndar:        15,
            Tags:            Array.Empty<string>(),
            Biomas:          Array.Empty<string>(),
            Requisitos:      null,
            ConsequenceTags: null
        ),
        new(
            Key:                "armadilha_detectada",
            Tipo:               TipoEvento.PassiveEvent,
            Tier:               TierEvento.Menor,
            Raridade:           EventoRaridade.Comum,
            TemImpactoMecanico: false,
            Titulo:             "Armadilha Detectada",
            Descricao:          "Olhos atentos detectam uma armadilha no corredor. A party a contorna com cuidado.",
            Opcoes:          null,
            Pericia:         null,
            DC:              null,
            Peso:            10,
            MinAndar:        1,
            MaxAndar:        15,
            Tags:            Array.Empty<string>(),
            Biomas:          Array.Empty<string>(),
            Requisitos:      null,
            ConsequenceTags: null
        ),
        new(
            Key:                "teste_forca_porta",
            Tipo:               TipoEvento.GroupCheck,
            Tier:               TierEvento.Maior,
            Raridade:           EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:             "Porta Selada",
            Descricao:          "Uma porta de pedra maciça bloqueia o avanço. Parece que força bruta é a única saída.",
            Opcoes: new[]
            {
                new OpcaoConfig("arrombar", "Arrombar a porta", RiscoTom.Arriscado)
            },
            Pericia:         Pericia.Atletismo,
            DC:              14,
            Peso:            10,
            MinAndar:        3,
            MaxAndar:        15,
            Tags:            Array.Empty<string>(),
            Biomas:          Array.Empty<string>(),
            Requisitos:      null,
            ConsequenceTags: null
        ),
        new(
            Key:                "sombra_perseguindo",
            Tipo:               TipoEvento.Encounter,
            Tier:               TierEvento.Maior,
            Raridade:           EventoRaridade.Comum,
            TemImpactoMecanico: true,
            Titulo:             "Sombra Perseguidora",
            Descricao:          "Uma presença hostil começa a seguir a party pelos corredores. Cada segundo conta.",
            Opcoes: new[]
            {
                new OpcaoConfig("fugir",     "Fugir rapidamente",  RiscoTom.Arriscado),
                new OpcaoConfig("enfrentar", "Virar e enfrentar",  RiscoTom.Arriscado)
            },
            Pericia:         null,
            DC:              null,
            Peso:            10,
            MinAndar:        8,
            MaxAndar:        15,
            Tags:            Array.Empty<string>(),
            Biomas:          Array.Empty<string>(),
            Requisitos:      null,
            ConsequenceTags: null
        )
    };

    public static IEnumerable<CheckpointEventoConfig> FiltrarParaAndar(
        int andar, IEnumerable<string> consequenceTags) =>
        Todos.Where(e =>
            e.MinAndar <= andar && e.MaxAndar >= andar &&
            (e.Requisitos == null || e.Requisitos.All(r => consequenceTags.Contains(r))));
}

using System;
using System.Collections.Generic;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Infrastructure.SeedData;

public static class FragmentoSeed
{
    // ── Bioma 1 — Floresta de Aelindra (Andares 1–25) ──────────────────────
    public static readonly Guid IdLira      = new("a2000000-0000-0000-0000-000000000001");
    public static readonly Guid IdKorin     = new("a2000000-0000-0000-0000-000000000002");
    public static readonly Guid IdRinzi     = new("a2000000-0000-0000-0000-000000000003");
    public static readonly Guid IdSelva     = new("a2000000-0000-0000-0000-000000000004");
    public static readonly Guid IdLune      = new("a2000000-0000-0000-0000-000000000005");
    public static readonly Guid IdSeraLince = new("a2000000-0000-0000-0000-000000000006");

    // ── Bioma 2 — Ruínas de Valdrek (Andares 26–50) ────────────────────────
    public static readonly Guid IdIgara     = new("a2000000-0000-0000-0000-000000000007");
    public static readonly Guid IdVarga     = new("a2000000-0000-0000-0000-000000000008");
    public static readonly Guid IdSkaara    = new("a2000000-0000-0000-0000-000000000009");
    public static readonly Guid IdVelara    = new("a2000000-0000-0000-0000-000000000010");
    public static readonly Guid IdNara      = new("a2000000-0000-0000-0000-000000000011");
    public static readonly Guid IdElisse    = new("a2000000-0000-0000-0000-000000000012");

    // ── Bioma 3 — Pico Vulcânico (Andares 51–75) ───────────────────────────
    public static readonly Guid IdDraxa     = new("a2000000-0000-0000-0000-000000000013");
    public static readonly Guid IdKira      = new("a2000000-0000-0000-0000-000000000014");
    public static readonly Guid IdMarev     = new("a2000000-0000-0000-0000-000000000015");
    public static readonly Guid IdZara      = new("a2000000-0000-0000-0000-000000000016");
    public static readonly Guid IdValdara   = new("a2000000-0000-0000-0000-000000000017");
    public static readonly Guid IdLilith    = new("a2000000-0000-0000-0000-000000000018");

    // ── Bioma 4 — Abismo Sombrio (Andares 76–100) ──────────────────────────
    public static readonly Guid IdZarael    = new("a2000000-0000-0000-0000-000000000019");
    public static readonly Guid IdMoira     = new("a2000000-0000-0000-0000-000000000020");
    public static readonly Guid IdZephirael = new("a2000000-0000-0000-0000-000000000021");
    public static readonly Guid IdMalachiel = new("a2000000-0000-0000-0000-000000000022");
    public static readonly Guid IdVesper    = new("a2000000-0000-0000-0000-000000000023");
    public static readonly Guid IdVrael     = new("a2000000-0000-0000-0000-000000000024");

    // ── Bioma 5 — Domínio Celestial (Andares 101–125) ──────────────────────
    public static readonly Guid IdAelia     = new("a2000000-0000-0000-0000-000000000025");
    public static readonly Guid IdElyriel   = new("a2000000-0000-0000-0000-000000000026");
    public static readonly Guid IdSeraphael = new("a2000000-0000-0000-0000-000000000027");
    public static readonly Guid IdLumira    = new("a2000000-0000-0000-0000-000000000028");
    public static readonly Guid IdAurael    = new("a2000000-0000-0000-0000-000000000029");
    public static readonly Guid IdNyx       = new("a2000000-0000-0000-0000-000000000030");

    // ── Biomas ─────────────────────────────────────────────────────────────
    public static readonly Guid IdBiomaFloresta  = new("b1000000-0000-0000-0000-000000000001");
    public static readonly Guid IdBiomaRuinas    = new("b1000000-0000-0000-0000-000000000002");
    public static readonly Guid IdBiomaVulcanico = new("b1000000-0000-0000-0000-000000000003");
    public static readonly Guid IdBiomaAbismo    = new("b1000000-0000-0000-0000-000000000004");
    public static readonly Guid IdBiomaCelestial = new("b1000000-0000-0000-0000-000000000005");

    // ───────────────────────────────────────────────────────────────────────
    public static IEnumerable<HeroiConfig> HeroiConfigs() =>
    [
        // ── Bioma 1 — Floresta de Aelindra ────────────────────────────────
        new() { Id = IdLira,      Nome = "Lira",      Titulo = "Flecha Dourada",        RaridadeBase = Raridade.Estrela3, Arquetipo = Profissao.Arqueiro,  Tag = "B1" },
        new() { Id = IdKorin,     Nome = "Korin",     Titulo = "Guardã do Bosque",      RaridadeBase = Raridade.Estrela3, Arquetipo = Profissao.Invocador, Tag = "B1" },
        new() { Id = IdRinzi,     Nome = "Rinzi",     Titulo = "Filha do Mercado",      RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Bardo,     Tag = "B1" },
        new() { Id = IdSelva,     Nome = "Selva",     Titulo = "Filha da Raiz",         RaridadeBase = Raridade.Estrela3, Arquetipo = Profissao.Mago,      Tag = "B1" },
        new() { Id = IdLune,      Nome = "Lune",      Titulo = "Voz da Alcateia",       RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Invocador, Tag = "B1" },
        new() { Id = IdSeraLince, Nome = "Sera",      Titulo = "Caçadora das Neves",    RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Guerreiro, Tag = "B1" },

        // ── Bioma 2 — Ruínas de Valdrek ───────────────────────────────────
        new() { Id = IdIgara,  Nome = "Igara",  Titulo = "Ruído do Vulcão",     RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Guerreiro, Tag = "B2" },
        new() { Id = IdVarga,  Nome = "Varga",  Titulo = "a Corrente Solta",    RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Guerreiro, Tag = "B2" },
        new() { Id = IdSkaara, Nome = "Skaara", Titulo = "Fogo Desperto",       RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Guerreiro, Tag = "B2" },
        new() { Id = IdVelara, Nome = "Velara", Titulo = "a Sombra sem Nome",   RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Ladino,    Tag = "B2" },
        new() { Id = IdNara,   Nome = "Nara",   Titulo = "Maestrina Noturna",   RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Invocador, Tag = "B2" },
        new() { Id = IdElisse, Nome = "Elisse", Titulo = "a Ordem Perfeita",    RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mago,      Tag = "B2" },

        // ── Bioma 3 — Pico Vulcânico ──────────────────────────────────────
        new() { Id = IdDraxa,   Nome = "Draxa",   Titulo = "a Fortaleza Viva",    RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Paladino,  Tag = "B3" },
        new() { Id = IdKira,    Nome = "Kira",    Titulo = "Lâmina do Crepúsculo",RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Ladino,    Tag = "B3" },
        new() { Id = IdMarev,   Nome = "Marev",   Titulo = "a Maré Eterna",       RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mago,      Tag = "B3" },
        new() { Id = IdZara,    Nome = "Zara",    Titulo = "a Bruxa do Vazio",    RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mago,      Tag = "B3" },
        new() { Id = IdValdara, Nome = "Valdara", Titulo = "a Herança Negra",     RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Guerreiro, Tag = "B3" },
        new() { Id = IdLilith,  Nome = "Lilith",  Titulo = "Camareira do Caos",   RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mago,      Tag = "B3" },

        // ── Bioma 4 — Abismo Sombrio ──────────────────────────────────────
        new() { Id = IdZarael,    Nome = "Zarael",    Titulo = "a Acorrentada",         RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Guerreiro, Tag = "B4" },
        new() { Id = IdMoira,     Nome = "Moira",     Titulo = "a Ceifeira",            RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Mago,      Tag = "B4" },
        new() { Id = IdZephirael, Nome = "Zephirael", Titulo = "a Tempestade Caída",    RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mago,      Tag = "B4" },
        new() { Id = IdMalachiel, Nome = "Malachiel", Titulo = "a Muralha Quebrada",    RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Paladino,  Tag = "B4" },
        new() { Id = IdVesper,    Nome = "Vesper",    Titulo = "o Abismo Vestido",      RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Mago,      Tag = "B4" },
        new() { Id = IdVrael,     Nome = "Vrael",     Titulo = "a Voz do Vácuo",        RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Invocador, Tag = "B4" },

        // ── Bioma 5 — Domínio Celestial ───────────────────────────────────
        new() { Id = IdAelia,     Nome = "Aelia",     Titulo = "Sentinela do Limiar",   RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Paladino,  Tag = "B5" },
        new() { Id = IdElyriel,   Nome = "Elyriel",   Titulo = "a Última Canção",       RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Bardo,     Tag = "B5" },
        new() { Id = IdSeraphael, Nome = "Seraphael", Titulo = "a Chama Corrompida",    RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Guerreiro, Tag = "B5" },
        new() { Id = IdLumira,    Nome = "Lumira",    Titulo = "Bênção da Alvorada",    RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Clerigo,   Tag = "B5" },
        new() { Id = IdAurael,    Nome = "Aurael",    Titulo = "o Punho do Éden",       RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Guerreiro, Tag = "B5" },
        new() { Id = IdNyx,       Nome = "Nyx",       Titulo = "Umbraveil",             RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Ladino,    Tag = "B5" },
    ];

    // ───────────────────────────────────────────────────────────────────────
    public static IEnumerable<HeroiUnlockConfig> UnlockConfigs() =>
    [
        // ── Bioma 1 ────────────────────────────────────────────────────────
        new() { HeroiId = IdLira,      TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 20 },
        new() { HeroiId = IdKorin,     TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 20 },
        new() { HeroiId = IdRinzi,     TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 8  },
        new() { HeroiId = IdSelva,     TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 25 },
        new() { HeroiId = IdLune,      TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 30 },
        new() { HeroiId = IdSeraLince, TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 20 },

        // ── Bioma 2 ────────────────────────────────────────────────────────
        new() { HeroiId = IdIgara,  TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 35 },
        new() { HeroiId = IdVarga,  TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 26 },
        new() { HeroiId = IdSkaara, TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 38 },
        new() { HeroiId = IdVelara, TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 36 },
        new() { HeroiId = IdNara,   TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 42 },
        new() { HeroiId = IdElisse, TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 45 },

        // ── Bioma 3 ────────────────────────────────────────────────────────
        new() { HeroiId = IdDraxa,   TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 51 },
        new() { HeroiId = IdKira,    TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 50 },
        new() { HeroiId = IdMarev,   TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 52 },
        new() { HeroiId = IdZara,    TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 60 },
        new() { HeroiId = IdValdara, TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 68 },
        new() { HeroiId = IdLilith,  TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 56 },

        // ── Bioma 4 ────────────────────────────────────────────────────────
        new() { HeroiId = IdZarael,    TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 76 },
        new() { HeroiId = IdMoira,     TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 58 },
        new() { HeroiId = IdZephirael, TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 60 },
        new() { HeroiId = IdMalachiel, TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 88 },
        new() { HeroiId = IdVesper,    TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 62 },
        new() { HeroiId = IdVrael,     TipoUnlock = TipoUnlock.CondicaoUnica, CondicaoDescricao = "Completar o Bioma 3 com 3 ou mais Bestiais na mesma party" },

        // ── Bioma 5 ────────────────────────────────────────────────────────
        new() { HeroiId = IdAelia,     TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 101 },
        new() { HeroiId = IdElyriel,   TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 65 },
        new() { HeroiId = IdSeraphael, TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 108 },
        new() { HeroiId = IdLumira,    TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 68 },
        new() { HeroiId = IdAurael,    TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 70 },
        new() { HeroiId = IdNyx,       TipoUnlock = TipoUnlock.CondicaoUnica, CondicaoDescricao = "Derrotar o chefe do Andar 120 sem perder nenhum herói na tentativa" },
    ];

    // ───────────────────────────────────────────────────────────────────────
    public static IEnumerable<Bioma> Biomas() =>
    [
        new() { Id = IdBiomaFloresta,  Nome = "Floresta de Aelindra", AndarInicio = 1,   AndarFim = 25,  Descricao = "Uma floresta antiga onde aventureiros escrevem suas primeiras histórias.",            Tag = "Floresta"  },
        new() { Id = IdBiomaRuinas,    Nome = "Ruínas de Valdrek",    AndarInicio = 26,  AndarFim = 50,  Descricao = "Ruínas de uma civilização esquecida, repletas de armadilhas e segredos.",            Tag = "Ruinas"    },
        new() { Id = IdBiomaVulcanico, Nome = "Pico Vulcânico",       AndarInicio = 51,  AndarFim = 75,  Descricao = "O cume incandescente onde os guerreiros mais duros são forjados.",                   Tag = "Vulcanico" },
        new() { Id = IdBiomaAbismo,    Nome = "Abismo Sombrio",       AndarInicio = 76,  AndarFim = 100, Descricao = "Um abismo de trevas onde anjos caídos e magos do vazio travam suas guerras eternas.", Tag = "Abismo"    },
        new() { Id = IdBiomaCelestial, Nome = "Domínio Celestial",    AndarInicio = 101, AndarFim = 125, Descricao = "O palco final do conflito entre Serafins e Anjos Caídos pelo destino do mundo mortal.",Tag = "Celestial" },
    ];

    // ───────────────────────────────────────────────────────────────────────
    public static IEnumerable<BiomHeroPool> BiomHeroPools() =>
    [
        // ── Bioma 1 — Floresta (pool de fragmentos) ───────────────────────
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000001"), BiomeId = IdBiomaFloresta, HeroiId = IdLira,  Raridade = Raridade.Estrela3, DropWeight = 35, EHeroPrincipal = true  },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000002"), BiomeId = IdBiomaFloresta, HeroiId = IdKorin, Raridade = Raridade.Estrela3, DropWeight = 35, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000003"), BiomeId = IdBiomaFloresta, HeroiId = IdSelva, Raridade = Raridade.Estrela3, DropWeight = 20, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000004"), BiomeId = IdBiomaFloresta, HeroiId = IdLune,  Raridade = Raridade.Estrela4, DropWeight = 10, EHeroPrincipal = false },

        // ── Bioma 2 — Ruínas (pool de fragmentos) ─────────────────────────
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000005"), BiomeId = IdBiomaRuinas, HeroiId = IdIgara,  Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000006"), BiomeId = IdBiomaRuinas, HeroiId = IdSkaara, Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000007"), BiomeId = IdBiomaRuinas, HeroiId = IdNara,   Raridade = Raridade.Estrela4, DropWeight = 25, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000008"), BiomeId = IdBiomaRuinas, HeroiId = IdElisse, Raridade = Raridade.Estrela4, DropWeight = 15, EHeroPrincipal = false },

        // ── Bioma 3 — Vulcânico (pool de fragmentos) ──────────────────────
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000009"), BiomeId = IdBiomaVulcanico, HeroiId = IdKira,   Raridade = Raridade.Estrela4, DropWeight = 35, EHeroPrincipal = true  },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000010"), BiomeId = IdBiomaVulcanico, HeroiId = IdMarev,  Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000011"), BiomeId = IdBiomaVulcanico, HeroiId = IdLilith, Raridade = Raridade.Estrela4, DropWeight = 20, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000012"), BiomeId = IdBiomaVulcanico, HeroiId = IdValdara,Raridade = Raridade.Estrela5, DropWeight = 15, EHeroPrincipal = false },

        // ── Bioma 4 — Abismo (pool de fragmentos) ─────────────────────────
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000013"), BiomeId = IdBiomaAbismo, HeroiId = IdMoira,     Raridade = Raridade.Estrela5, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000014"), BiomeId = IdBiomaAbismo, HeroiId = IdZephirael, Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000015"), BiomeId = IdBiomaAbismo, HeroiId = IdVesper,    Raridade = Raridade.Estrela5, DropWeight = 25, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000016"), BiomeId = IdBiomaAbismo, HeroiId = IdMalachiel, Raridade = Raridade.Estrela5, DropWeight = 15, EHeroPrincipal = false },

        // ── Bioma 5 — Celestial (pool de fragmentos) ──────────────────────
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000017"), BiomeId = IdBiomaCelestial, HeroiId = IdElyriel,   Raridade = Raridade.Estrela5, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000018"), BiomeId = IdBiomaCelestial, HeroiId = IdSeraphael, Raridade = Raridade.Estrela5, DropWeight = 25, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000019"), BiomeId = IdBiomaCelestial, HeroiId = IdLumira,    Raridade = Raridade.Estrela5, DropWeight = 25, EHeroPrincipal = false },
        new() { Id = new Guid("c2000000-0000-0000-0000-000000000020"), BiomeId = IdBiomaCelestial, HeroiId = IdAurael,    Raridade = Raridade.Estrela5, DropWeight = 20, EHeroPrincipal = false },
    ];
}

using System;
using System.Collections.Generic;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Infrastructure.SeedData;

public static class FragmentoSeed
{
    public static readonly Guid IdAldric    = new("a1000000-0000-0000-0000-000000000001");
    public static readonly Guid IdYuzara    = new("a1000000-0000-0000-0000-000000000002");
    public static readonly Guid IdThorvald  = new("a1000000-0000-0000-0000-000000000003");
    public static readonly Guid IdKaen      = new("a1000000-0000-0000-0000-000000000004");
    public static readonly Guid IdNyra      = new("a1000000-0000-0000-0000-000000000005");
    public static readonly Guid IdSeraph    = new("a1000000-0000-0000-0000-000000000006");
    public static readonly Guid IdMira      = new("a1000000-0000-0000-0000-000000000007");
    public static readonly Guid IdGrom      = new("a1000000-0000-0000-0000-000000000008");
    public static readonly Guid IdHana      = new("a1000000-0000-0000-0000-000000000009");

    public static readonly Guid IdBiomaFloresta  = new("b1000000-0000-0000-0000-000000000001");
    public static readonly Guid IdBiomaRuinas    = new("b1000000-0000-0000-0000-000000000002");
    public static readonly Guid IdBiomaVulcanico = new("b1000000-0000-0000-0000-000000000003");

    public static IEnumerable<HeroiConfig> HeroiConfigs() =>
    [
        new() { Id = IdAldric,   Nome = "Aldric, o Sem-Corrente",         RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Guerreiro  },
        new() { Id = IdYuzara,   Nome = "Yuzara, a Tecelã do Destino",    RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Mago       },
        new() { Id = IdThorvald, Nome = "Thorvald, o Arquiteto das Eras", RaridadeBase = Raridade.Estrela5, Arquetipo = Profissao.Ferreiro   },
        new() { Id = IdKaen,     Nome = "Kaen",                           RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Arqueiro   },
        new() { Id = IdNyra,     Nome = "Nyra",                           RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Ladino     },
        new() { Id = IdSeraph,   Nome = "Seraph",                         RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Paladino   },
        new() { Id = IdMira,     Nome = "Mira",                           RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Alquimista },
        new() { Id = IdGrom,     Nome = "Grom",                           RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Mineiro    },
        new() { Id = IdHana,     Nome = "Hana",                           RaridadeBase = Raridade.Estrela4, Arquetipo = Profissao.Cozinheiro },
    ];

    public static IEnumerable<HeroiUnlockConfig> UnlockConfigs() =>
    [
        new() { HeroiId = IdAldric,   TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 30 },
        new() { HeroiId = IdYuzara,   TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 60 },
        new() { HeroiId = IdThorvald, TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 60 },
        new() { HeroiId = IdKaen,     TipoUnlock = TipoUnlock.MarcoTorre,    AndarMarco = 10 },
        new() { HeroiId = IdSeraph,   TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 40 },
        new() { HeroiId = IdMira,     TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 35 },
        new() { HeroiId = IdGrom,     TipoUnlock = TipoUnlock.Fragmentos,    QuantidadeFragmentos = 30 },
        new() { HeroiId = IdNyra,     TipoUnlock = TipoUnlock.CondicaoUnica, CondicaoDescricao = "Completar o andar 15 com a party completa sem nenhum herói ser derrotado" },
        new() { HeroiId = IdHana,     TipoUnlock = TipoUnlock.CondicaoUnica, CondicaoDescricao = "Ter pelo menos 3 heróis com Humor >= 80 na cidade ao mesmo tempo" },
    ];

    public static IEnumerable<Bioma> Biomas() =>
    [
        new() { Id = IdBiomaFloresta,  Nome = "Floresta de Aelindra", AndarInicio = 1,  AndarFim = 10, Descricao = "Uma floresta antiga onde aventureiros escrevem suas primeiras histórias.", Tag = "Floresta"  },
        new() { Id = IdBiomaRuinas,    Nome = "Ruínas de Valdrek",    AndarInicio = 11, AndarFim = 25, Descricao = "Ruínas de uma civilização esquecida, repletas de armadilhas e segredos.",  Tag = "Ruinas"    },
        new() { Id = IdBiomaVulcanico, Nome = "Pico Vulcânico",       AndarInicio = 26, AndarFim = 50, Descricao = "O cume incandescente onde os guerreiros mais duros são forjados.",         Tag = "Vulcanico" },
    ];

    public static IEnumerable<BiomHeroPool> BiomHeroPools() =>
    [
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000001"), BiomeId = IdBiomaFloresta,  HeroiId = IdKaen,   Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000002"), BiomeId = IdBiomaFloresta,  HeroiId = IdHana,   Raridade = Raridade.Estrela4, DropWeight = 70, EHeroPrincipal = false },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000003"), BiomeId = IdBiomaRuinas,    HeroiId = IdSeraph, Raridade = Raridade.Estrela4, DropWeight = 30, EHeroPrincipal = true  },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000004"), BiomeId = IdBiomaRuinas,    HeroiId = IdNyra,   Raridade = Raridade.Estrela4, DropWeight = 70, EHeroPrincipal = false },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000005"), BiomeId = IdBiomaVulcanico, HeroiId = IdAldric, Raridade = Raridade.Estrela5, DropWeight = 20, EHeroPrincipal = true  },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000006"), BiomeId = IdBiomaVulcanico, HeroiId = IdMira,   Raridade = Raridade.Estrela4, DropWeight = 45, EHeroPrincipal = false },
        new() { Id = new Guid("c1000000-0000-0000-0000-000000000007"), BiomeId = IdBiomaVulcanico, HeroiId = IdGrom,   Raridade = Raridade.Estrela4, DropWeight = 35, EHeroPrincipal = false },
    ];
}

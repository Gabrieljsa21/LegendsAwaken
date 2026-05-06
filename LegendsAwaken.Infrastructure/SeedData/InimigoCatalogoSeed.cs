using System;
using System.Collections.Generic;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Infrastructure.SeedData;

/// <summary>
/// Catálogo de inimigos por bioma, derivado das tabelas de Monstros por Ambiente do D&amp;D 5E DMG.
/// CR original mapeado para faixa de andares dentro de cada bioma.
/// </summary>
public static class InimigoCatalogoSeed
{
    // ── GUIDs dos biomas existentes (B1–B5) ─────────────────────────────────
    private static readonly Guid B1 = new("b1000000-0000-0000-0000-000000000001"); // Floresta
    private static readonly Guid B2 = new("b1000000-0000-0000-0000-000000000002"); // Ruínas
    private static readonly Guid B3 = new("b1000000-0000-0000-0000-000000000003"); // Vulcânico
    private static readonly Guid B4 = new("b1000000-0000-0000-0000-000000000004"); // Abismo
    private static readonly Guid B5 = new("b1000000-0000-0000-0000-000000000005"); // Celestial

    // ── GUIDs dos novos biomas (B6–B13) ─────────────────────────────────────
    public static readonly Guid IdBiomaPlanie      = new("b2000000-0000-0000-0000-000000000001"); // Planície
    public static readonly Guid IdBiomaCosta       = new("b2000000-0000-0000-0000-000000000002"); // Costa
    public static readonly Guid IdBiomaPantano     = new("b2000000-0000-0000-0000-000000000003"); // Pântano
    public static readonly Guid IdBiomaDeserto     = new("b2000000-0000-0000-0000-000000000004"); // Deserto
    public static readonly Guid IdBiomaArtico      = new("b2000000-0000-0000-0000-000000000005"); // Ártico
    public static readonly Guid IdBiomaProfundezas = new("b2000000-0000-0000-0000-000000000006"); // Profundezas
    public static readonly Guid IdBiomaSubmundo    = new("b2000000-0000-0000-0000-000000000007"); // Submundo
    public static readonly Guid IdBiomaCidade      = new("b2000000-0000-0000-0000-000000000008"); // Cidade Corrompida

    // ─────────────────────────────────────────────────────────────────────────
    public static IEnumerable<Bioma> NovoBiomas() =>
    [
        new() { Id = IdBiomaPlanie,      Nome = "Planície dos Confrontos",  AndarInicio = 126, AndarFim = 150, Descricao = "Campos abertos onde hordas se formam no horizonte e não há onde se esconder.", Tag = "Planicie"    },
        new() { Id = IdBiomaCosta,       Nome = "Costa de Ferro",           AndarInicio = 151, AndarFim = 175, Descricao = "Penhascos sobre um mar furioso, dominado por sahuagins e dragões da tempestade.",  Tag = "Costa"       },
        new() { Id = IdBiomaPantano,     Nome = "Pântano do Esquecimento",  AndarInicio = 176, AndarFim = 200, Descricao = "Névoa que consome memórias. Yuan-tis e trolls habitam este lugar de podridão.",     Tag = "Pantano"     },
        new() { Id = IdBiomaDeserto,     Nome = "Deserto das Cinzas",       AndarInicio = 201, AndarFim = 225, Descricao = "Areia negra e calor eterno. Múmias e efreetis guardam segredos de eras extintas.",  Tag = "Deserto"     },
        new() { Id = IdBiomaArtico,      Nome = "Ártico das Almas",         AndarInicio = 226, AndarFim = 250, Descricao = "Tempestades eternas de neve onde yetis e dragões brancos reinam sobre o silêncio.", Tag = "Artico"      },
        new() { Id = IdBiomaProfundezas, Nome = "As Profundezas",           AndarInicio = 251, AndarFim = 275, Descricao = "Abismo subaquático sem luz. O Kraken dorme aqui e algo pior o vigia.",             Tag = "Profundezas" },
        new() { Id = IdBiomaSubmundo,    Nome = "O Submundo",               AndarInicio = 276, AndarFim = 300, Descricao = "A Underdark: cidades Drow, devouradores de mentes e liches em seus covos eternas.", Tag = "Submundo"    },
        new() { Id = IdBiomaCidade,      Nome = "A Cidade Corrompida",      AndarInicio = 301, AndarFim = 325, Descricao = "Uma metrópole tomada por vampiros, rakshasas e arquimagos sem escrúpulos.",         Tag = "Cidade"      },
    ];

    // ─────────────────────────────────────────────────────────────────────────
    private static Guid E(int n) => new($"e{n:D7}-0000-0000-0000-000000000000");

    public static IEnumerable<Inimigo> Inimigos() =>
    [
        // ══ B1 — Floresta de Aelindra (1–25) ═════════════════════════════════
        // Ref. D&D: Monstros da Floresta  CR 0–5
        new() { Id = E(101), BiomaId = B1, Nome = "Goblin",              Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas,    ElementoFraqueza = Elemento.Luz,     AndarMinimo = 1,  AndarMaximo = 12 },
        new() { Id = E(102), BiomaId = B1, Nome = "Kobold",              Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Fogo,      ElementoFraqueza = Elemento.Água,    AndarMinimo = 1,  AndarMaximo = 10 },
        new() { Id = E(103), BiomaId = B1, Nome = "Lobo",                Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Natureza,  ElementoFraqueza = Elemento.Fogo,    AndarMinimo = 3,  AndarMaximo = 15 },
        new() { Id = E(104), BiomaId = B1, Nome = "Aranha Gigante",      Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Trevas,    ElementoFraqueza = Elemento.Luz,     AndarMinimo = 8,  AndarMaximo = 18 },
        new() { Id = E(105), BiomaId = B1, Nome = "Sprite",              Tipo = TipoInimigo.Fada,           ElementoAfinidade = Elemento.Natureza,  ElementoFraqueza = Elemento.Trevas,  AndarMinimo = 6,  AndarMaximo = 16 },
        new() { Id = E(106), BiomaId = B1, Nome = "Bugbear",             Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas,    ElementoFraqueza = Elemento.Luz,     AndarMinimo = 12, AndarMaximo = 22 },
        new() { Id = E(107), BiomaId = B1, Nome = "Dríade",              Tipo = TipoInimigo.Fada,           ElementoAfinidade = Elemento.Natureza,  ElementoFraqueza = Elemento.Fogo,    AndarMinimo = 12, AndarMaximo = 22 },
        new() { Id = E(108), BiomaId = B1, Nome = "Ettercap",            Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Trevas,    ElementoFraqueza = null,             AndarMinimo = 15, AndarMaximo = 24 },
        new() { Id = E(109), BiomaId = B1, Nome = "Gnoll",               Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas,    ElementoFraqueza = null,             AndarMinimo = 16, AndarMaximo = 24 },
        new() { Id = E(110), BiomaId = B1, Nome = "Druida Corrompido",   Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Natureza,  ElementoFraqueza = Elemento.Luz,     AndarMinimo = 25, AndarMaximo = 25, EChefe = true },

        // ══ B2 — Ruínas de Valdrek (26–50) ═══════════════════════════════════
        // Ref. D&D: Monstros da Colina + Subterrâneo  CR 1/2–5
        new() { Id = E(201), BiomaId = B2, Nome = "Orc",                    Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,  AndarMinimo = 26, AndarMaximo = 36 },
        new() { Id = E(202), BiomaId = B2, Nome = "Hobgoblin",              Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Metal,   ElementoFraqueza = null,          AndarMinimo = 26, AndarMaximo = 38 },
        new() { Id = E(203), BiomaId = B2, Nome = "Esqueleto",              Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,  AndarMinimo = 28, AndarMaximo = 40 },
        new() { Id = E(204), BiomaId = B2, Nome = "Zumbi",                  Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,  AndarMinimo = 28, AndarMaximo = 40 },
        new() { Id = E(205), BiomaId = B2, Nome = "Ogro",                   Tipo = TipoInimigo.Gigante,    ElementoAfinidade = Elemento.Terra,   ElementoFraqueza = null,          AndarMinimo = 34, AndarMaximo = 44 },
        new() { Id = E(206), BiomaId = B2, Nome = "Carniçal",               Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,  AndarMinimo = 36, AndarMaximo = 46 },
        new() { Id = E(207), BiomaId = B2, Nome = "Gnoll Líder de Matilha", Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = null,          AndarMinimo = 38, AndarMaximo = 48 },
        new() { Id = E(208), BiomaId = B2, Nome = "Gárgula",                Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Terra, ElementoFraqueza = Elemento.Trevas, AndarMinimo = 40, AndarMaximo = 48 },
        new() { Id = E(209), BiomaId = B2, Nome = "Banshee",                Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,  AndarMinimo = 43, AndarMaximo = 47, EChefe = true },
        new() { Id = E(210), BiomaId = B2, Nome = "Ressurgido",             Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,  AndarMinimo = 50, AndarMaximo = 50, EChefe = true },

        // ══ B3 — Pico Vulcânico (51–75) ══════════════════════════════════════
        // Ref. D&D: Monstros da Montanha  CR 1/2–10
        new() { Id = E(301), BiomaId = B3, Nome = "Mephit do Magma",       Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Gelo,  AndarMinimo = 51, AndarMaximo = 62 },
        new() { Id = E(302), BiomaId = B3, Nome = "Magmin",                 Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Água,  AndarMinimo = 51, AndarMaximo = 62 },
        new() { Id = E(303), BiomaId = B3, Nome = "Salamandra",             Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Água,  AndarMinimo = 54, AndarMaximo = 66 },
        new() { Id = E(304), BiomaId = B3, Nome = "Cão Infernal",           Tipo = TipoInimigo.Corruptor,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Água,  AndarMinimo = 56, AndarMaximo = 66 },
        new() { Id = E(305), BiomaId = B3, Nome = "Manticora",              Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = null,           AndarMinimo = 58, AndarMaximo = 70 },
        new() { Id = E(306), BiomaId = B3, Nome = "Elemental do Fogo",      Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Água,  AndarMinimo = 60, AndarMaximo = 72 },
        new() { Id = E(307), BiomaId = B3, Nome = "Wyvern",                 Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Gelo,  AndarMinimo = 62, AndarMaximo = 73 },
        new() { Id = E(308), BiomaId = B3, Nome = "Gigante do Fogo",        Tipo = TipoInimigo.Gigante,        ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Água,  AndarMinimo = 64, AndarMaximo = 73 },
        new() { Id = E(309), BiomaId = B3, Nome = "Dragão Cromático Jovem", Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Gelo,  AndarMinimo = 70, AndarMaximo = 74, EChefe = true },
        new() { Id = E(310), BiomaId = B3, Nome = "Dragão Vermelho Adulto", Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Gelo,  AndarMinimo = 75, AndarMaximo = 75, EChefe = true },

        // ══ B4 — Abismo Sombrio (76–100) ══════════════════════════════════════
        // Ref. D&D: Subterrâneo profundo + Corruptores  CR 4–13
        new() { Id = E(401), BiomaId = B4, Nome = "Demônio das Sombras",  Tipo = TipoInimigo.Corruptor,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,   AndarMinimo = 76, AndarMaximo = 86 },
        new() { Id = E(402), BiomaId = B4, Nome = "Dretch",               Tipo = TipoInimigo.Corruptor,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,   AndarMinimo = 76, AndarMaximo = 86 },
        new() { Id = E(403), BiomaId = B4, Nome = "Capitão Hobgoblin",    Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = null,           AndarMinimo = 78, AndarMaximo = 88 },
        new() { Id = E(404), BiomaId = B4, Nome = "Duplo",                Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = null,           AndarMinimo = 82, AndarMaximo = 92 },
        new() { Id = E(405), BiomaId = B4, Nome = "Devorador de Mentes",  Tipo = TipoInimigo.Aberracao,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = null,           AndarMinimo = 84, AndarMaximo = 94 },
        new() { Id = E(406), BiomaId = B4, Nome = "Hezrou",               Tipo = TipoInimigo.Corruptor,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,   AndarMinimo = 86, AndarMaximo = 96 },
        new() { Id = E(407), BiomaId = B4, Nome = "Naga dos Ossos",       Tipo = TipoInimigo.MortoVivo,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,   AndarMinimo = 88, AndarMaximo = 95, EChefe = true },
        new() { Id = E(408), BiomaId = B4, Nome = "Glabrezu",             Tipo = TipoInimigo.Corruptor,      ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,   AndarMinimo = 88, AndarMaximo = 98 },
        new() { Id = E(409), BiomaId = B4, Nome = "Dragão das Sombras",   Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Trevas,  ElementoFraqueza = Elemento.Luz,   AndarMinimo = 92, AndarMaximo = 99 },
        new() { Id = E(410), BiomaId = B4, Nome = "Balor",                Tipo = TipoInimigo.Corruptor,      ElementoAfinidade = Elemento.Fogo,    ElementoFraqueza = Elemento.Luz,   AndarMinimo = 100, AndarMaximo = 100, EChefe = true },

        // ══ B5 — Domínio Celestial (101–125) ══════════════════════════════════
        // Ref. D&D: Celestiais, Corruptores de alto nível  CR 12–21
        new() { Id = E(501), BiomaId = B5, Nome = "Erínia",               Tipo = TipoInimigo.Corruptor,  ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Luz,  AndarMinimo = 101, AndarMaximo = 112 },
        new() { Id = E(502), BiomaId = B5, Nome = "Cria Vampírica",       Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 101, AndarMaximo = 112 },
        new() { Id = E(503), BiomaId = B5, Nome = "Deva Corrompida",      Tipo = TipoInimigo.Celestial,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 104, AndarMaximo = 114 },
        new() { Id = E(504), BiomaId = B5, Nome = "Cavaleiro da Morte",   Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Gelo,   ElementoFraqueza = Elemento.Luz,  AndarMinimo = 108, AndarMaximo = 120 },
        new() { Id = E(505), BiomaId = B5, Nome = "Diabo dos Chifres",    Tipo = TipoInimigo.Corruptor,  ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Luz,  AndarMinimo = 110, AndarMaximo = 120 },
        new() { Id = E(506), BiomaId = B5, Nome = "Dracolich",            Tipo = TipoInimigo.Dragao,     ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 112, AndarMaximo = 122 },
        new() { Id = E(507), BiomaId = B5, Nome = "Lich",                 Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 114, AndarMaximo = 122 },
        new() { Id = E(508), BiomaId = B5, Nome = "Solar Caído",          Tipo = TipoInimigo.Celestial,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 116, AndarMaximo = 124, EChefe = true },
        new() { Id = E(509), BiomaId = B5, Nome = "Planetário Corrompido",Tipo = TipoInimigo.Celestial,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 125, AndarMaximo = 125, EChefe = true },

        // ══ B6 — Planície dos Confrontos (126–150) ════════════════════════════
        // Ref. D&D: Monstros da Planície  CR 1–10
        new() { Id = E(601), BiomaId = IdBiomaPlanie, Nome = "Centauro",              Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Natureza, ElementoFraqueza = null,          AndarMinimo = 126, AndarMaximo = 136 },
        new() { Id = E(602), BiomaId = IdBiomaPlanie, Nome = "Grifo",                 Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Ar,       ElementoFraqueza = null,          AndarMinimo = 126, AndarMaximo = 138 },
        new() { Id = E(603), BiomaId = IdBiomaPlanie, Nome = "Tricerátops",           Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Terra,    ElementoFraqueza = null,          AndarMinimo = 128, AndarMaximo = 138 },
        new() { Id = E(604), BiomaId = IdBiomaPlanie, Nome = "Ankheg",                Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Terra,    ElementoFraqueza = null,          AndarMinimo = 130, AndarMaximo = 140 },
        new() { Id = E(605), BiomaId = IdBiomaPlanie, Nome = "Gnoll Presa de Yeenoghu",Tipo = TipoInimigo.Humanoide,    ElementoAfinidade = Elemento.Trevas,   ElementoFraqueza = Elemento.Luz,  AndarMinimo = 132, AndarMaximo = 144 },
        new() { Id = E(606), BiomaId = IdBiomaPlanie, Nome = "Gorgão",                Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Metal,    ElementoFraqueza = null,          AndarMinimo = 136, AndarMaximo = 146 },
        new() { Id = E(607), BiomaId = IdBiomaPlanie, Nome = "Quimera",               Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Fogo,     ElementoFraqueza = Elemento.Gelo, AndarMinimo = 138, AndarMaximo = 148 },
        new() { Id = E(608), BiomaId = IdBiomaPlanie, Nome = "Tiranossauro",          Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Terra,    ElementoFraqueza = null,          AndarMinimo = 140, AndarMaximo = 149, EChefe = true },
        new() { Id = E(609), BiomaId = IdBiomaPlanie, Nome = "Dragão de Ouro Jovem",  Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Luz,      ElementoFraqueza = Elemento.Trevas, AndarMinimo = 144, AndarMaximo = 149 },
        new() { Id = E(610), BiomaId = IdBiomaPlanie, Nome = "Dragão Ouro Adulto",    Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Luz,      ElementoFraqueza = Elemento.Trevas, AndarMinimo = 150, AndarMaximo = 150, EChefe = true },

        // ══ B7 — Costa de Ferro (151–175) ════════════════════════════════════
        // Ref. D&D: Monstros da Costa + Subaquáticos adjacentes  CR 1/2–9
        new() { Id = E(701), BiomaId = IdBiomaCosta, Nome = "Sahuagin",             Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 151, AndarMaximo = 162 },
        new() { Id = E(702), BiomaId = IdBiomaCosta, Nome = "Bruxa do Mar",         Tipo = TipoInimigo.Fada,       ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 151, AndarMaximo = 162 },
        new() { Id = E(703), BiomaId = IdBiomaCosta, Nome = "Harpia",               Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Ar, ElementoFraqueza = null,           AndarMinimo = 152, AndarMaximo = 164 },
        new() { Id = E(704), BiomaId = IdBiomaCosta, Nome = "Merrow",               Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Água, ElementoFraqueza = Elemento.Raio, AndarMinimo = 156, AndarMaximo = 166 },
        new() { Id = E(705), BiomaId = IdBiomaCosta, Nome = "Sacerdotisa Sahuagin", Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 158, AndarMaximo = 168 },
        new() { Id = E(706), BiomaId = IdBiomaCosta, Nome = "Barão Sahuagin",       Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 162, AndarMaximo = 172 },
        new() { Id = E(707), BiomaId = IdBiomaCosta, Nome = "Elemental da Água",    Tipo = TipoInimigo.Elemental,  ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 164, AndarMaximo = 174 },
        new() { Id = E(708), BiomaId = IdBiomaCosta, Nome = "Djinni",               Tipo = TipoInimigo.Elemental,  ElementoAfinidade = Elemento.Raio,  ElementoFraqueza = Elemento.Terra, AndarMinimo = 164, AndarMaximo = 173, EChefe = true },
        new() { Id = E(709), BiomaId = IdBiomaCosta, Nome = "Dragão de Bronze Jovem",Tipo = TipoInimigo.Dragao,   ElementoAfinidade = Elemento.Raio,  ElementoFraqueza = null,           AndarMinimo = 168, AndarMaximo = 174 },
        new() { Id = E(710), BiomaId = IdBiomaCosta, Nome = "Dragão Azul Jovem",    Tipo = TipoInimigo.Dragao,    ElementoAfinidade = Elemento.Raio,  ElementoFraqueza = Elemento.Terra, AndarMinimo = 175, AndarMaximo = 175, EChefe = true },

        // ══ B8 — Pântano do Esquecimento (176–200) ════════════════════════════
        // Ref. D&D: Monstros do Pântano  CR 1/4–8
        new() { Id = E(801), BiomaId = IdBiomaPantano, Nome = "Bullywug",              Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 176, AndarMaximo = 186 },
        new() { Id = E(802), BiomaId = IdBiomaPantano, Nome = "Yuan-Ti Puro-Sangue",   Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,           AndarMinimo = 176, AndarMaximo = 188 },
        new() { Id = E(803), BiomaId = IdBiomaPantano, Nome = "Crocodilo Gigante",     Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 178, AndarMaximo = 192 },
        new() { Id = E(804), BiomaId = IdBiomaPantano, Nome = "Lívido",                Tipo = TipoInimigo.MortoVivo,      ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,   AndarMinimo = 182, AndarMaximo = 194 },
        new() { Id = E(805), BiomaId = IdBiomaPantano, Nome = "Troll",                 Tipo = TipoInimigo.Gigante,        ElementoAfinidade = Elemento.Terra,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 180, AndarMaximo = 194 },
        new() { Id = E(806), BiomaId = IdBiomaPantano, Nome = "Bruxa Verde (Convenção)",Tipo = TipoInimigo.Fada,          ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,           AndarMinimo = 184, AndarMaximo = 198 },
        new() { Id = E(807), BiomaId = IdBiomaPantano, Nome = "Yuan-Ti Mestiço",       Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,           AndarMinimo = 186, AndarMaximo = 198 },
        new() { Id = E(808), BiomaId = IdBiomaPantano, Nome = "Hidra",                 Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Água,   ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 190, AndarMaximo = 198, EChefe = true },
        new() { Id = E(809), BiomaId = IdBiomaPantano, Nome = "Yuan-Ti Abominação",    Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,           AndarMinimo = 190, AndarMaximo = 199 },
        new() { Id = E(810), BiomaId = IdBiomaPantano, Nome = "Dragão Negro Adulto",   Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,   AndarMinimo = 200, AndarMaximo = 200, EChefe = true },

        // ══ B9 — Deserto das Cinzas (201–225) ════════════════════════════════
        // Ref. D&D: Monstros do Deserto  CR 1/2–17
        new() { Id = E(901), BiomaId = IdBiomaDeserto, Nome = "Gnoll",              Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Água,  AndarMinimo = 201, AndarMaximo = 212 },
        new() { Id = E(902), BiomaId = IdBiomaDeserto, Nome = "Yuan-Ti Mestiço",    Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,           AndarMinimo = 202, AndarMaximo = 214 },
        new() { Id = E(903), BiomaId = IdBiomaDeserto, Nome = "Múmia",              Tipo = TipoInimigo.MortoVivo,      ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,   AndarMinimo = 204, AndarMaximo = 216 },
        new() { Id = E(904), BiomaId = IdBiomaDeserto, Nome = "Escorpião Gigante",  Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Terra,  ElementoFraqueza = null,           AndarMinimo = 206, AndarMaximo = 218 },
        new() { Id = E(905), BiomaId = IdBiomaDeserto, Nome = "Efreeti",            Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Gelo,  AndarMinimo = 210, AndarMaximo = 222 },
        new() { Id = E(906), BiomaId = IdBiomaDeserto, Nome = "Naga Guardiã",       Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,   AndarMinimo = 212, AndarMaximo = 222 },
        new() { Id = E(907), BiomaId = IdBiomaDeserto, Nome = "Androesfinge",       Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Luz,    ElementoFraqueza = Elemento.Trevas,AndarMinimo = 214, AndarMaximo = 224 },
        new() { Id = E(908), BiomaId = IdBiomaDeserto, Nome = "Senhor das Múmias",  Tipo = TipoInimigo.MortoVivo,      ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,   AndarMinimo = 216, AndarMaximo = 224, EChefe = true },
        new() { Id = E(909), BiomaId = IdBiomaDeserto, Nome = "Dragão Azul Adulto", Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Raio,   ElementoFraqueza = Elemento.Terra, AndarMinimo = 218, AndarMaximo = 224 },
        new() { Id = E(910), BiomaId = IdBiomaDeserto, Nome = "Dragão Azul Ancião", Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Raio,   ElementoFraqueza = Elemento.Terra, AndarMinimo = 225, AndarMaximo = 225, EChefe = true },

        // ══ B10 — Ártico das Almas (226–250) ══════════════════════════════════
        // Ref. D&D: Monstros do Ártico  CR 1/2–20
        new() { Id = E(1001), BiomaId = IdBiomaArtico, Nome = "Mephit do Gelo",      Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 226, AndarMaximo = 236 },
        new() { Id = E(1002), BiomaId = IdBiomaArtico, Nome = "Yeti",                Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 226, AndarMaximo = 236 },
        new() { Id = E(1003), BiomaId = IdBiomaArtico, Nome = "Lobo Invernal",       Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 226, AndarMaximo = 238 },
        new() { Id = E(1004), BiomaId = IdBiomaArtico, Nome = "Remorhaz Jovem",      Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 228, AndarMaximo = 242 },
        new() { Id = E(1005), BiomaId = IdBiomaArtico, Nome = "Gigante do Gelo",     Tipo = TipoInimigo.Gigante,        ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 232, AndarMaximo = 244 },
        new() { Id = E(1006), BiomaId = IdBiomaArtico, Nome = "Yeti Abominável",     Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 234, AndarMaximo = 248 },
        new() { Id = E(1007), BiomaId = IdBiomaArtico, Nome = "Roca",                Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Terra, ElementoFraqueza = Elemento.Raio,  AndarMinimo = 238, AndarMaximo = 248, EChefe = true },
        new() { Id = E(1008), BiomaId = IdBiomaArtico, Nome = "Remorhaz",            Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 238, AndarMaximo = 249 },
        new() { Id = E(1009), BiomaId = IdBiomaArtico, Nome = "Dragão Branco Adulto",Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 242, AndarMaximo = 249 },
        new() { Id = E(1010), BiomaId = IdBiomaArtico, Nome = "Dragão Branco Ancião",Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Gelo,  ElementoFraqueza = Elemento.Fogo,  AndarMinimo = 250, AndarMaximo = 250, EChefe = true },

        // ══ B11 — As Profundezas (251–275) ════════════════════════════════════
        // Ref. D&D: Monstros Subaquáticos  CR 0–23
        new() { Id = E(1101), BiomaId = IdBiomaProfundezas, Nome = "Povo do Mar",          Tipo = TipoInimigo.Humanoide,      ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 251, AndarMaximo = 260 },
        new() { Id = E(1102), BiomaId = IdBiomaProfundezas, Nome = "Polvo Gigante",        Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 251, AndarMaximo = 262 },
        new() { Id = E(1103), BiomaId = IdBiomaProfundezas, Nome = "Sirenídeo",            Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Água,  ElementoFraqueza = null,           AndarMinimo = 254, AndarMaximo = 266 },
        new() { Id = E(1104), BiomaId = IdBiomaProfundezas, Nome = "Tubarão Gigante",      Tipo = TipoInimigo.Besta,          ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 256, AndarMaximo = 268 },
        new() { Id = E(1105), BiomaId = IdBiomaProfundezas, Nome = "Marid",                Tipo = TipoInimigo.Elemental,      ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 260, AndarMaximo = 272 },
        new() { Id = E(1106), BiomaId = IdBiomaProfundezas, Nome = "Abolete",              Tipo = TipoInimigo.Aberracao,      ElementoAfinidade = Elemento.Água,  ElementoFraqueza = null,           AndarMinimo = 262, AndarMaximo = 272, EChefe = true },
        new() { Id = E(1107), BiomaId = IdBiomaProfundezas, Nome = "Gigante da Tempestade",Tipo = TipoInimigo.Gigante,        ElementoAfinidade = Elemento.Raio,  ElementoFraqueza = null,           AndarMinimo = 264, AndarMaximo = 274 },
        new() { Id = E(1108), BiomaId = IdBiomaProfundezas, Nome = "Tartaruga-Dragão",     Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 264, AndarMaximo = 274 },
        new() { Id = E(1109), BiomaId = IdBiomaProfundezas, Nome = "Dragão Azul Ancião",   Tipo = TipoInimigo.Dragao,         ElementoAfinidade = Elemento.Raio,  ElementoFraqueza = Elemento.Terra, AndarMinimo = 268, AndarMaximo = 274 },
        new() { Id = E(1110), BiomaId = IdBiomaProfundezas, Nome = "Kraken",               Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Água,  ElementoFraqueza = Elemento.Raio,  AndarMinimo = 275, AndarMaximo = 275, EChefe = true },

        // ══ B12 — O Submundo (276–300) ════════════════════════════════════════
        // Ref. D&D: Monstros do Subterrâneo (profundo)  CR 5–22
        new() { Id = E(1201), BiomaId = IdBiomaSubmundo, Nome = "Drow Guerreiro de Elite",   Tipo = TipoInimigo.Humanoide, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 276, AndarMaximo = 288 },
        new() { Id = E(1202), BiomaId = IdBiomaSubmundo, Nome = "Drow Arcano",               Tipo = TipoInimigo.Humanoide, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 278, AndarMaximo = 290 },
        new() { Id = E(1203), BiomaId = IdBiomaSubmundo, Nome = "Drow Sacerdotisa de Lolth", Tipo = TipoInimigo.Humanoide, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 280, AndarMaximo = 292 },
        new() { Id = E(1204), BiomaId = IdBiomaSubmundo, Nome = "Drider",                    Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz, AndarMinimo = 280, AndarMaximo = 292 },
        new() { Id = E(1205), BiomaId = IdBiomaSubmundo, Nome = "Devorador de Mentes Arcanista", Tipo = TipoInimigo.Aberracao, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,        AndarMinimo = 284, AndarMaximo = 296 },
        new() { Id = E(1206), BiomaId = IdBiomaSubmundo, Nome = "Observador",                Tipo = TipoInimigo.Aberracao, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,          AndarMinimo = 286, AndarMaximo = 298 },
        new() { Id = E(1207), BiomaId = IdBiomaSubmundo, Nome = "Lich",                      Tipo = TipoInimigo.MortoVivo, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 288, AndarMaximo = 298, EChefe = true },
        new() { Id = E(1208), BiomaId = IdBiomaSubmundo, Nome = "Tirano da Morte",           Tipo = TipoInimigo.MortoVivo, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 290, AndarMaximo = 299 },
        new() { Id = E(1209), BiomaId = IdBiomaSubmundo, Nome = "Demilich",                  Tipo = TipoInimigo.MortoVivo, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 294, AndarMaximo = 299 },
        new() { Id = E(1210), BiomaId = IdBiomaSubmundo, Nome = "Lich Ancião",               Tipo = TipoInimigo.MortoVivo, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 300, AndarMaximo = 300, EChefe = true },

        // ══ B13 — A Cidade Corrompida (301–325) ═══════════════════════════════
        // Ref. D&D: Monstros Urbanos  CR 4–30
        new() { Id = E(1301), BiomaId = IdBiomaCidade, Nome = "Vampiro Vassalo",         Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 301, AndarMaximo = 312 },
        new() { Id = E(1302), BiomaId = IdBiomaCidade, Nome = "Rakshasa",                Tipo = TipoInimigo.Corruptor,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,          AndarMinimo = 302, AndarMaximo = 314 },
        new() { Id = E(1303), BiomaId = IdBiomaCidade, Nome = "Cambion",                 Tipo = TipoInimigo.Corruptor,  ElementoAfinidade = Elemento.Fogo,   ElementoFraqueza = Elemento.Luz,  AndarMinimo = 304, AndarMaximo = 316 },
        new() { Id = E(1304), BiomaId = IdBiomaCidade, Nome = "Súcubo",                  Tipo = TipoInimigo.Corruptor,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 306, AndarMaximo = 318 },
        new() { Id = E(1305), BiomaId = IdBiomaCidade, Nome = "Vampiro",                 Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 308, AndarMaximo = 320 },
        new() { Id = E(1306), BiomaId = IdBiomaCidade, Nome = "Arquimago Corrompido",    Tipo = TipoInimigo.Humanoide,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = null,          AndarMinimo = 310, AndarMaximo = 322 },
        new() { Id = E(1307), BiomaId = IdBiomaCidade, Nome = "Oni",                     Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz, AndarMinimo = 312, AndarMaximo = 322, EChefe = true },
        new() { Id = E(1308), BiomaId = IdBiomaCidade, Nome = "Vampiro Conjurador",      Tipo = TipoInimigo.MortoVivo,  ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 312, AndarMaximo = 323 },
        new() { Id = E(1309), BiomaId = IdBiomaCidade, Nome = "Dragão de Prata Corrompido", Tipo = TipoInimigo.Dragao, ElementoAfinidade = Elemento.Trevas, ElementoFraqueza = Elemento.Luz,  AndarMinimo = 316, AndarMaximo = 324 },
        new() { Id = E(1310), BiomaId = IdBiomaCidade, Nome = "Tarrasque",               Tipo = TipoInimigo.Monstruosidade, ElementoAfinidade = null,        ElementoFraqueza = null,          AndarMinimo = 325, AndarMaximo = 325, EChefe = true },
    ];
}

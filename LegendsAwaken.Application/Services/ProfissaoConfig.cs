using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public static class ProfissaoConfig
{
    // ── Initial stat distributions (total=60 per spec §4.2) ──────────────────
    // Order: STR, DEX, CON, INT, WIS, CHA
    public static readonly IReadOnlyDictionary<Profissao, AtributosBase> DistribuicaoInicial =
        new Dictionary<Profissao, AtributosBase>
        {
            { Profissao.Guerreiro,   B(14, 10, 12,  8,  9,  7) },
            { Profissao.Arqueiro,    B( 9, 14, 10,  8, 12,  7) },
            { Profissao.Mago,        B( 7,  9, 10, 14, 12,  8) },
            { Profissao.Ladino,      B( 8, 14,  7, 12,  9, 10) },
            { Profissao.Paladino,    B(14,  8, 10,  7,  9, 12) },
            { Profissao.Clerigo,     B( 7,  8, 10,  9, 14, 12) },
            { Profissao.Bardo,       B( 8, 10,  8,  8, 10, 16) },
            { Profissao.Invocador,   B( 7,  8, 10, 14, 12,  9) },
            { Profissao.Agricultor,  B(10,  9, 12,  8, 13,  8) },
            { Profissao.Pescador,    B( 9, 12, 10,  8, 12,  9) },
            { Profissao.Caçador,     B( 9, 14, 10,  8, 12,  7) },
            { Profissao.Lenhador,    B(14,  9, 12,  7, 10,  8) },
            { Profissao.Mineiro,     B(13,  8, 14,  8,  9,  8) },
            { Profissao.Cozinheiro,  B( 8,  9, 10, 11, 13,  9) },
            { Profissao.Ferreiro,    B(14,  8, 12, 10,  9,  7) },
            { Profissao.Alfaiate,    B( 7, 13,  8, 11,  9, 12) },
            { Profissao.Joalheiro,   B( 7, 11,  8, 13,  9, 12) },
            { Profissao.Alquimista,  B( 7,  9, 10, 14, 12,  8) },
            { Profissao.Construtor,  B(13,  8, 12, 11, 10,  6) },
            { Profissao.Pesquisador, B( 6,  9,  8, 14, 14,  9) },
        };

    // ── Base HP per combat profession ─────────────────────────────────────────
    public static readonly IReadOnlyDictionary<Profissao, int> BaseHpPorProfissao =
        new Dictionary<Profissao, int>
        {
            { Profissao.Guerreiro, 12 },
            { Profissao.Paladino,  12 },
            { Profissao.Arqueiro,  10 },
            { Profissao.Ladino,    10 },
            { Profissao.Bardo,     10 },
            { Profissao.Mago,       8 },
            { Profissao.Clerigo,    8 },
            { Profissao.Invocador,  8 },
        };

    private const int GanhoHpPorNivel = 1;

    public static int CalcularHpMaximo(Profissao? profissao, int nivel, int constituicao)
    {
        int baseHp = profissao.HasValue && BaseHpPorProfissao.TryGetValue(profissao.Value, out var b) ? b : 8;
        int modCon = (int)System.Math.Floor((constituicao - 10.0) / 2.0);
        int hp = baseHp + (nivel * GanhoHpPorNivel) + modCon;
        return System.Math.Max(1, hp);
    }

    // ── Initial proficiências per profissão ───────────────────────────────────
    public static readonly IReadOnlyDictionary<Profissao, Pericia[]> ProficienciasIniciais =
        new Dictionary<Profissao, Pericia[]>
        {
            { Profissao.Guerreiro,   [Pericia.Atletismo, Pericia.Intimidacao] },
            { Profissao.Arqueiro,    [Pericia.Furtividade, Pericia.Percepcao] },
            { Profissao.Mago,        [Pericia.Arcanismo, Pericia.Historia] },
            { Profissao.Ladino,      [Pericia.Prestidigitacao, Pericia.Furtividade, Pericia.Enganacao] },
            { Profissao.Paladino,    [Pericia.Atletismo, Pericia.Religiao, Pericia.Persuasao] },
            { Profissao.Clerigo,     [Pericia.Medicina, Pericia.Religiao, Pericia.Intuicao] },
            { Profissao.Bardo,       [Pericia.Persuasao, Pericia.Atuacao, Pericia.Enganacao] },
            { Profissao.Invocador,   [Pericia.Arcanismo, Pericia.Investigacao] },
            { Profissao.Agricultor,  [Pericia.Natureza, Pericia.Sobrevivencia] },
            { Profissao.Pescador,    [Pericia.Natureza, Pericia.Atletismo] },
            { Profissao.Caçador,     [Pericia.Sobrevivencia, Pericia.Furtividade, Pericia.Percepcao] },
            { Profissao.Lenhador,    [Pericia.Natureza, Pericia.Atletismo] },
            { Profissao.Mineiro,     [Pericia.Atletismo, Pericia.Historia] },
            { Profissao.Cozinheiro,  [Pericia.Medicina, Pericia.Natureza] },
            { Profissao.Ferreiro,    [Pericia.Atletismo, Pericia.Historia] },
            { Profissao.Alfaiate,    [Pericia.Prestidigitacao] },
            { Profissao.Joalheiro,   [Pericia.Historia, Pericia.Investigacao] },
            { Profissao.Alquimista,  [Pericia.Arcanismo, Pericia.Natureza, Pericia.Medicina] },
            { Profissao.Construtor,  [Pericia.Atletismo, Pericia.Historia] },
            { Profissao.Pesquisador, [Pericia.Arcanismo, Pericia.Historia, Pericia.Investigacao, Pericia.Religiao] },
        };

    public static AtributosBase ObterDistribuicao(Profissao? profissao)
        => profissao.HasValue && DistribuicaoInicial.TryGetValue(profissao.Value, out var d)
            ? d
            : AtributosBase.Distribute(60);

    private static AtributosBase B(int str, int dex, int con, int intel, int wis, int cha)
        => new AtributosBase
        {
            Forca        = str,
            Destreza     = dex,
            Constituicao = con,
            Inteligencia = intel,
            Sabedoria    = wis,
            Carisma      = cha,
        };
}

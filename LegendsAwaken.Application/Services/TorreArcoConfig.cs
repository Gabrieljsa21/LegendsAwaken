namespace LegendsAwaken.Application.Services;

public record ArcoDefinicao(
    int Numero,
    string Nome,
    int AndarInicio,
    int AndarFim,
    IReadOnlyList<AndarArcoDefinicao> Andares);

public record AndarArcoDefinicao(
    int Numero,
    string NarrativaDisplay,
    ObjetivoDefinicao? ObjetivoSecundario,
    ColecionavelDefinicao? Colecionavel,
    IReadOnlyList<BossModificador> ModificadoresBoss,
    IReadOnlyList<string> FlagsGeradasPossiveis);

public record ObjetivoDefinicao(
    string Descricao,
    string FlagNome,
    string EfeitoDescricao,
    string? RequererFlag = null);

public record ColecionavelDefinicao(
    string Nome,
    string Categoria,
    string Descricao,
    string? FlagCondicional = null);

public record BossModificador(
    string FlagNome,
    string EfeitoDescricao,
    double HpReductionPercent);

public record FlagCompostaDefinicao(
    string NomeComposta,
    IReadOnlyList<string> ComponentesNecessarios,
    string EfeitoDescricao);

public static class TorreArcoConfig
{
    public static IReadOnlyList<FlagCompostaDefinicao> FlagsCompostas { get; } =
    [
        new("identidade_revelada",
            ["grimorio_encontrado", "diario_rasgado"],
            "Boss Andar 4: -5% HP adicional do Carniçal"),
        new("rota_alternativa",
            ["contexto_obtido", "mapa_rabiscado"],
            "Andar 9: acesso a rota secundária evitando armadilhas"),
        new("andolyn_aliada",
            ["gendrew_resgatado", "prata_preservada"],
            "NPC permanente no hub: identificação de itens + 1 magia gratuita/semana"),
        new("woganpuck_rastreado",
            ["woganpuck_revelado"],
            "Ativa rota alternativa de confronto quando Woganpuck reaparecer"),
    ];

    public static IReadOnlyList<ArcoDefinicao> Arcos { get; } =
    [
        new ArcoDefinicao(1, "Torre em Ruínas", 1, 4,
        [
            new AndarArcoDefinicao(1,
                "Uma torre em colapso. Esqueletos vagam entre cinzas. Um grimório flutua sobre uma pilha de pedras.",
                new ObjetivoDefinicao(
                    "Examinar o grimório antes de destruí-lo",
                    "grimorio_encontrado",
                    "Boss Andar 4: -10% HP do Carniçal"),
                new ColecionavelDefinicao("moeda_arcana", "Economia",
                    "Moeda de facção arcana antiga — valor ao vender"),
                [],
                ["grimorio_encontrado"]),
            new AndarArcoDefinicao(2,
                "Esqueletos armados bloqueiam o corredor. Um altar pulsa com energia sombria ao fundo.",
                new ObjetivoDefinicao(
                    "Destruir o altar antes de avançar",
                    "altar_destruido",
                    "Andar 3: impede ressurgimento de mortos-vivos durante o combate"),
                new ColecionavelDefinicao("amuleto_de_osso", "Build",
                    "+5% resistência física, sem tradeoff"),
                [],
                ["altar_destruido"]),
            new AndarArcoDefinicao(3,
                "Zumbis lentos. Uma página rasgada de diário jaz no chão entre os destroços.",
                null,
                new ColecionavelDefinicao("diario_rasgado", "Arquivo",
                    "Páginas parciais — sozinho não revela nada"),
                [],
                ["diario_rasgado"]),
            new AndarArcoDefinicao(4,
                "Uma câmara escura no topo da ala. O Carniçal aguarda imóvel — você já o conhece?",
                null,
                new ColecionavelDefinicao("anel_do_mago", "Build",
                    "+8% dano mágico, sem tradeoff", "bossDerrotado"),
                [
                    new BossModificador("grimorio_encontrado",
                        "-10% HP do Carniçal", 0.10),
                    new BossModificador("identidade_revelada",
                        "-5% HP adicional (identidade revelada)", 0.05),
                ],
                []),
        ]),

        new ArcoDefinicao(2, "A Praga Ardente", 5, 10,
        [
            new AndarArcoDefinicao(5,
                "Kobolds contaminados. Os corpos têm marcas estranhas — nenhuma ferida de combate.",
                new ObjetivoDefinicao(
                    "Investigar os corpos antes de avançar",
                    "causa_investigada",
                    "Andar 8: desbloqueia diálogo com kobold sobrevivente"),
                null,
                [],
                ["causa_investigada"]),
            new AndarArcoDefinicao(6,
                "Um refeitório em chamas. Kobolds e trabalhadores infectados. Alguém ainda está vivo.",
                new ObjetivoDefinicao(
                    "Resgatar o sobrevivente durante o combate",
                    "sobrevivente_resgatado",
                    "Pós-arco: NPC permanente no hub"),
                null,
                [],
                ["sobrevivente_resgatado"]),
            new AndarArcoDefinicao(7,
                "Guardiões protegem uma fonte negra borbulhante. A contaminação vem daqui.",
                new ObjetivoDefinicao(
                    "Destruir a fonte de contaminação",
                    "fonte_destruida",
                    "Boss Andar 10: -15% HP de Jakk; Andar 8: reduz Exaustão do grupo"),
                new ColecionavelDefinicao("frasco_agua_pura", "Chave",
                    "Cancela mecânica de veneno em área de Jakk uma vez"),
                [],
                ["fonte_destruida"]),
            new AndarArcoDefinicao(8,
                "Uma câmara central. Um kobold encostado na parede observa com olhos inteligentes demais.",
                new ObjetivoDefinicao(
                    "Dialogar com o kobold sobrevivente",
                    "contexto_obtido",
                    "Gera rota_alternativa no Andar 9 com mapa_rabiscado",
                    RequererFlag: "causa_investigada"),
                null,
                [],
                ["contexto_obtido", "mapa_rabiscado"]),
            new AndarArcoDefinicao(9,
                "Um poço profundo. A água se move. Algo está abaixo da superfície.",
                null,
                new ColecionavelDefinicao("pedra_mana_contaminada", "Build",
                    "+12% dano mágico / +1 Fadiga por uso"),
                [],
                []),
            new AndarArcoDefinicao(10,
                "Jakk aguarda. À medida que você avança, zumbis começam a surgir das sombras.",
                null,
                new ColecionavelDefinicao("selo_de_jakk", "Arquivo",
                    "Combina com item futuro para revelar afiliação de Jakk"),
                [
                    new BossModificador("fonte_destruida",
                        "-15% HP de Jakk", 0.15),
                ],
                []),
        ]),

        new ArcoDefinicao(3, "A Cabana dos Experimentos", 11, 15,
        [
            new AndarArcoDefinicao(11,
                "Uma sala que cheira a madeira velha e pólvora. Livros que voam. Cordas que apertam. Algo aqui não quer que você passe.",
                new ObjetivoDefinicao(
                    "Examinar o livro aberto antes de destruí-lo",
                    "grimorio_golem_lido",
                    "Boss Andar 15: -15% HP do Golem"),
                new ColecionavelDefinicao("fragmento_livro_arcano", "Lore",
                    "Notas sobre imunidades de constructs", "grimorio_golem_lido"),
                [],
                ["grimorio_golem_lido", "objetos_destruidos"]),
            new AndarArcoDefinicao(12,
                "Silêncio. Mesa posta para dois. Velas frias. Prata reluzindo como se esperasse alguém que não veio.",
                new ObjetivoDefinicao(
                    "Preservar os objetos de prata sem quebrá-los",
                    "prata_preservada",
                    "Composta andolyn_aliada: NPC permanente"),
                new ColecionavelDefinicao("talheres_de_prata", "Economia",
                    "12 peças de prata — valor em ouro ao vender"),
                [],
                ["prata_preservada"]),
            new AndarArcoDefinicao(13,
                "Asas de morcego. Risos baixos. Um homem amarrado na cama, quase morto, quase respirando. Ainda há tempo.",
                new ObjetivoDefinicao(
                    "Estabilizar o prisioneiro durante ou após o combate",
                    "gendrew_resgatado",
                    "Informa fraqueza do boss + localização da Caixa de Poções"),
                null,
                [],
                ["gendrew_resgatado", "diabretes_derrotados", "woganpuck_revelado"]),
            new AndarArcoDefinicao(14,
                "O fogão está quente. Algo se move lá dentro. O ar fede a enxofre e a farinha queimada.",
                null,
                null,
                [],
                ["mephits_pacificados", "fraqueza_confirmada"]),
            new AndarArcoDefinicao(15,
                "No porão: uma criatura de massa e crosta. Seus punhos fumegam. Você sente o calor antes de vê-la.",
                null,
                new ColecionavelDefinicao("frasco_molho_fervente", "Build",
                    "+8% dano físico em ataque / +1 Fadiga ao usuário", "bossDerrotado"),
                [
                    new BossModificador("grimorio_golem_lido",
                        "-15% HP do Golem", 0.15),
                    new BossModificador("fraqueza_confirmada",
                        "-5% HP adicional (fraqueza confirmada)", 0.05),
                ],
                []),
        ]),
    ];

    public static ArcoDefinicao? ObterArcoPorAndar(int andar) =>
        Arcos.FirstOrDefault(a => andar >= a.AndarInicio && andar <= a.AndarFim);

    public static AndarArcoDefinicao? ObterAndar(int andar) =>
        ObterArcoPorAndar(andar)?.Andares.FirstOrDefault(a => a.Numero == andar);

    public static bool EBossFloor(int andar) =>
        ObterAndar(andar)?.ModificadoresBoss.Count > 0;
}

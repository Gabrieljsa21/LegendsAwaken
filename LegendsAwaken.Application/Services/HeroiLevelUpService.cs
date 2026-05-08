using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;

namespace LegendsAwaken.Application.Services
{
    /// <summary>
    /// Dados de configuração de uma raridade.
    /// Centraliza cap, stats base e ganhos de level-up em um único objeto imutável.
    /// Para ajustar uma raridade, basta alterar o registro correspondente em
    /// <see cref="HeroiLevelUpService.Configs"/> — sem mais números mágicos espalhados.
    /// </summary>
    /// <param name="Cap">Nível máximo alcançável nesta raridade.</param>
    /// <param name="BaseStatsTotal">Soma dos 5 atributos no nível 1, distribuídos igualmente.</param>
    /// <param name="GanhoPorNivel">Pontos de atributo por level-up na fase normal.</param>
    /// <param name="GanhoSuperacao">
    /// Pontos por level-up na fase de superação (acima do cap da raridade anterior).
    /// Zero significa que esta raridade não tem fase de superação.
    /// </param>
    public record RaridadeConfig(
        int Cap,
        int BaseStatsTotal,
        int GanhoPorNivel,
        int GanhoSuperacao = 0,
        int BaseXp = 0
    );

    /// <summary>
    /// Cálculos de progressão de nível: ganhos por level-up, stats base por raridade,
    /// grant de nivelamento na ascensão e bônus raciais permanentes.
    /// </summary>
    public class HeroiLevelUpService
    {
        // ── Fonte única de verdade ─────────────────────────────────────────────────
        // Tudo que define uma raridade está aqui. Mudar cap, base ou ganho em uma linha
        // propaga automaticamente para: CalcularPontosAtributosPorLevelUp,
        // CalcularTotalPontosNativo, CalcularGrantAscensao e ObterAtributosBase.
        //
        // GanhoSuperacao: ativa-se automaticamente quando nivelAtual > cap da raridade
        // anterior (ex: 5★ usa +12 acima do lv80, porque 80 = cap do 4★).
        // Se o cap do 4★ mudar, o limiar do 5★ muda junto — sem edição adicional.
        public static readonly IReadOnlyDictionary<int, RaridadeConfig> Configs =
            new Dictionary<int, RaridadeConfig>
            {
                { 1, new(Cap:  20, BaseStatsTotal:  60, GanhoPorNivel: 0,                   BaseXp:  80) },
                { 2, new(Cap:  40, BaseStatsTotal:  60, GanhoPorNivel: 0,                   BaseXp: 100) },
                { 3, new(Cap:  60, BaseStatsTotal:  60, GanhoPorNivel: 0,                   BaseXp: 120) },
                { 4, new(Cap:  80, BaseStatsTotal:  60, GanhoPorNivel: 0,                   BaseXp: 150) },
                { 5, new(Cap: 100, BaseStatsTotal:  60, GanhoPorNivel: 0, GanhoSuperacao: 1, BaseXp: 200) },
            };

        // ── Bônus raciais ──────────────────────────────────────────────────────────
        // Todas as raças não-humanas têm o mesmo bônus (+50) no atributo foco.
        // Humano: sem bônus fixo — os +3 à escolha são tratados no onboarding.
        // Racial bonus: +50 to the focus attribute, using AtributosBase.With so
        // no property names are hardcoded here. Adding a new race = add one line.
        public static readonly IReadOnlyDictionary<Raca, AtributosBase> BonusRacial =
            new Dictionary<Raca, AtributosBase>
            {
                { Raca.Humano,     new AtributosBase { Forca=1, Destreza=1, Constituicao=1, Inteligencia=1, Sabedoria=1, Carisma=1 } },
                { Raca.Bestial,    AtributosBase.With(Atributo.Forca,        2) },
                { Raca.Anao,       AtributosBase.With(Atributo.Constituicao, 2) },
                { Raca.Elfo,       AtributosBase.With(Atributo.Sabedoria,    2) },
                { Raca.Draconato,  AtributosBase.With(Atributo.Inteligencia, 2) },
                { Raca.Fada,       AtributosBase.With(Atributo.Destreza,     2) },
                { Raca.AnjoCaido,  AtributosBase.With(Atributo.Carisma,      2) },
                { Raca.Serafim,    AtributosBase.With(Atributo.Sabedoria,    2) },
            };

        // Multiplicador de XP por raça (1.0 = sem bônus)
        public static readonly IReadOnlyDictionary<Raca, double> MultiplicadorXpRacial =
            new Dictionary<Raca, double>
            {
                { Raca.Humano,     1.10 },
                { Raca.Bestial,    1.00 },
                { Raca.Anao,       1.00 },
                { Raca.Elfo,       1.00 },
                { Raca.Draconato,  1.00 },
                { Raca.Fada,       1.00 },
                { Raca.AnjoCaido,  1.00 },
                { Raca.Serafim,    1.00 },
            };

        // ── Métodos principais ─────────────────────────────────────────────────────

        /// <summary>
        /// Pontos de atributo ganhos ao subir do <paramref name="nivelAtual"/> para o próximo.
        /// A fase de superação é calculada comparando com o cap da raridade anterior —
        /// nenhum número mágico no código.
        /// </summary>
        public int CalcularPontosAtributosPorLevelUp(int nivelAtual, int raridade)
        {
            if (!Configs.TryGetValue(raridade, out var config)) return 0;

            // 5★ superação phase: 1 ASI point every level above 4★ cap
            if (config.GanhoSuperacao > 0
                && Configs.TryGetValue(raridade - 1, out var anterior)
                && nivelAtual > anterior.Cap)
                return config.GanhoSuperacao;

            // Normal phase: 1 ASI point every 4 levels
            return nivelAtual % 4 == 0 ? 1 : 0;
        }

        /// <summary>
        /// Total acumulado (base + todos os level-ups) de um herói NATIVO de
        /// <paramref name="raridade"/> que esteja no nível <paramref name="nivel"/>.
        /// </summary>
        public int CalcularTotalPontosNativo(int raridade, int nivel)
        {
            if (!Configs.TryGetValue(raridade, out var config)) return 0;

            int ganhos = 0;
            for (int n = 1; n < nivel; n++)
                ganhos += CalcularPontosAtributosPorLevelUp(n, raridade);

            return config.BaseStatsTotal + ganhos;
        }

        /// <summary>
        /// Pontos concedidos ao ascender da <paramref name="raridadeAtual"/> para a próxima,
        /// igualando o herói a um nativo da nova raridade no mesmo nível.
        /// </summary>
        public int CalcularGrantAscensao(int nivelAtual, int raridadeAtual)
        {
            if (!Configs.ContainsKey(raridadeAtual + 1)) return 0;

            return CalcularTotalPontosNativo(raridadeAtual + 1, nivelAtual)
                 - CalcularTotalPontosNativo(raridadeAtual,     nivelAtual);
        }

        /// <summary>
        /// Cap de nível para uma raridade. Derivado de <see cref="Configs"/>.
        /// </summary>
        public int CapParaRaridade(int raridade) =>
            Configs.TryGetValue(raridade, out var c) ? c.Cap : 0;

        /// <summary>
        /// Returns base stats distributed evenly across all 6 attrs.
        /// Used for CalcularGrantAscensao math only.
        /// Hero creation uses ProfissaoConfig.ObterDistribuicao(profissao) instead.
        /// </summary>
        public AtributosBase ObterAtributosBaseParaRaridade(int raridade)
        {
            if (!Configs.TryGetValue(raridade, out var config)) return new AtributosBase();
            return AtributosBase.Distribute(config.BaseStatsTotal);
        }

        // ── Sistema de XP ─────────────────────────────────────────────────────────

        /// <summary>
        /// XP necessário para subir do <paramref name="nivel"/> atual para o próximo.
        /// Fase 3A: fórmula linear <c>B_r × nivel</c>. Migrar para <c>B_r × nivel^1.25</c> no beta.
        /// </summary>
        public int XpParaProximoNivel(int nivel, int raridade)
        {
            if (!Configs.TryGetValue(raridade, out var config)) return int.MaxValue;
            return config.BaseXp * nivel;
        }

        /// <summary>
        /// Aplica <paramref name="xpGanho"/> ao herói, aplicando o multiplicador racial
        /// e processando level-ups em loop enquanto o limiar for atingido.
        /// Retorna o número de níveis ganhos. O chamador persiste o herói via repositório.
        /// </summary>
        public int AplicarXp(Heroi heroi, int xpGanho)
        {
            int raridade = (int)heroi.Raridade;
            if (!Configs.TryGetValue(raridade, out var config)) return 0;

            double mult = MultiplicadorXpRacial.GetValueOrDefault(heroi.Raca, 1.0);
            heroi.XP += (int)(xpGanho * mult);

            int niveisGanhos = 0;
            while (heroi.Nivel < config.Cap)
            {
                int xpNecessario = XpParaProximoNivel(heroi.Nivel, raridade);
                if (heroi.XP < xpNecessario) break;

                heroi.XP -= xpNecessario;
                int pontosGanhos = CalcularPontosAtributosPorLevelUp(heroi.Nivel, raridade);
                heroi.Nivel++;
                heroi.PontosAtributosDisponiveis += pontosGanhos;
                niveisGanhos++;
            }

            // No cap, XP excedente não acumula
            if (heroi.Nivel >= config.Cap)
                heroi.XP = 0;

            return niveisGanhos;
        }
    }
}

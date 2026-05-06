using LegendsAwaken.Domain.Enum;
using System.Collections.Generic;

namespace LegendsAwaken.Application.Services;

public record AndarItemDef(
    string Id,
    string Nome,
    TipoItemJogador Tipo,
    string Icone,
    string Efeito);

public static class AndarItemConfig
{
    private static readonly Dictionary<int, AndarItemDef> _itens = new()
    {
        // Andares 1–5 — Zona Ouro (itens básicos)
        [1]  = new("bioma_a_01", "Pó de Ouro",            TipoItemJogador.ComponenteCrafting, "💛", "Ingrediente básico de crafting"),
        [2]  = new("bioma_a_02", "Erva Medicinal",         TipoItemJogador.Consumivel,         "🌿", "Restaura 20% do HP de um herói na exploração"),
        [3]  = new("bioma_a_03", "Amuleto de Madeira",     TipoItemJogador.Equipamento,        "🪵", "+5 DEF quando equipado"),
        [4]  = new("bioma_a_04", "Pedra de Afiar",         TipoItemJogador.Consumivel,         "🗡️", "+15% ATK em um combate"),
        [5]  = new("bioma_a_05", "Diário do Explorador",   TipoItemJogador.ItemProgressao,     "📓", "Desbloqueia entrada de lore: Bioma A"),

        // Andares 6–10 — Zona Gema Rústica
        [6]  = new("bioma_a_06", "Gema Rústica Lascada",  TipoItemJogador.ComponenteCrafting, "🟡", "Componente de crafting intermediário"),
        [7]  = new("bioma_a_07", "Totem de Proteção",     TipoItemJogador.Equipamento,        "🛡️", "+8 DEF para o herói designado"),
        [8]  = new("bioma_a_08", "Frasco de Stamina",     TipoItemJogador.Consumivel,         "🧪", "Recupera vigor pós-operação (+1 slot por 4h)"),
        [9]  = new("bioma_a_09", "Cristal de Sintonização",TipoItemJogador.ItemProgressao,    "🔷", "Reduz duração de uma operação em 30 min"),
        [10] = new("bioma_a_10", "Mapa Parcial",          TipoItemJogador.ItemProgressao,     "🗺️", "Revela os itens dos andares 11–15"),

        // Andares 11–15 — Zona Essência Corrompida (início)
        [11] = new("bioma_a_11", "Essência Bruta",        TipoItemJogador.ComponenteCrafting, "🟣", "Componente de crafting raro"),
        [12] = new("bioma_a_12", "Amuleto das Sombras",   TipoItemJogador.Equipamento,        "🌑", "+5 AGI quando equipado"),
        [13] = new("bioma_a_13", "Elixir de Coragem",     TipoItemJogador.Consumivel,         "⚗️", "+20% ATK no próximo combate"),
        [14] = new("bioma_a_14", "Fragmento de Memória",  TipoItemJogador.ItemProgressao,     "🧩", "+5% XP do herói por 24h"),
        [15] = new("bioma_a_15", "Cristal de Ressonância",TipoItemJogador.Equipamento,        "💎", "Bônus de dano vs inimigos sombrios"),

        // Andares 16–20 — Zona Essência Corrompida (avançado)
        [16] = new("bioma_a_16", "Cinza de Fênix",        TipoItemJogador.Consumivel,         "🔥", "Revive um herói com 30% HP em exploração"),
        [17] = new("bioma_a_17", "Totem de Velocidade",   TipoItemJogador.Equipamento,        "⚡", "+10 AGI para o herói designado"),
        [18] = new("bioma_a_18", "Pó de Essência",        TipoItemJogador.ComponenteCrafting, "🫧", "Essência purificada para receitas avançadas"),
        [19] = new("bioma_a_19", "Símbolo de Ascensão",   TipoItemJogador.ItemProgressao,     "⬆️", "+1 margem no próximo rank-up de herói"),
        [20] = new("bioma_a_20", "Cristal de Sombra",     TipoItemJogador.ComponenteCrafting, "🌒", "Ingrediente raro para criação de relíquias"),

        // Andares 21–25 — Transição para Bioma B / Fragmento Arcano
        [21] = new("bioma_a_21", "Fragmento Arcano Bruto",TipoItemJogador.ComponenteCrafting, "🔵", "Base para itens do Bioma B"),
        [22] = new("bioma_a_22", "Anel do Abismo",        TipoItemJogador.Equipamento,        "💍", "+8 INT quando equipado"),
        [23] = new("bioma_a_23", "Vial de Mana Concentrada",TipoItemJogador.Consumivel,       "🫗", "Restaura 40% da mana de um herói"),
        [24] = new("bioma_a_24", "Runa de Proteção",      TipoItemJogador.Equipamento,        "🔮", "+10 RES quando equipado"),
        [25] = new("bioma_a_25", "Pedra-Chave do Bioma",  TipoItemJogador.ItemProgressao,     "🗝️", "Desbloqueia Bioma B e bônus de exploração permanente"),
    };

    public static AndarItemDef? ObterItemDoAndar(int andar)
        => _itens.TryGetValue(andar, out var def) ? def : null;

    public static string LabelCurto(AndarItemDef item)
        => $"{item.Icone} {item.Nome}";
}

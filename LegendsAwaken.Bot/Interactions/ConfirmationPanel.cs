// LegendsAwaken.Bot/Interactions/ConfirmationPanel.cs
using Discord;

namespace LegendsAwaken.Bot.Interactions;

public static class ConfirmationPanel
{
    /// <summary>
    /// Cria um embed efêmero de confirmação com botões [Confirmar] e [Cancelar].
    /// </summary>
    /// <param name="mensagem">Texto descritivo da ação a confirmar.</param>
    /// <param name="confirmId">CustomId do botão Confirmar. Formato: "sistema:acao:param"</param>
    /// <param name="cancelId">CustomId do botão Cancelar. Padrão: "global:cancelar"</param>
    public static PanelResult Criar(string mensagem, string confirmId, string cancelId = "global:cancelar")
    {
        var embed = new EmbedBuilder()
            .WithTitle("⚠️ Confirmar ação")
            .WithDescription(mensagem)
            .WithColor(Color.Orange)
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Confirmar", confirmId, ButtonStyle.Danger)
            .WithButton("Cancelar", cancelId, ButtonStyle.Secondary)
            .Build();

        return new PanelResult(embed, components);
    }
}

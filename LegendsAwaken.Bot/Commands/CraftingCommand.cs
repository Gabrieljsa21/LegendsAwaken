using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Enum;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class CraftingCommand
    {
        private readonly CraftingService _craftingService;
        private readonly HeroiService _heroiService;

        public CraftingCommand(CraftingService craftingService, HeroiService heroiService)
        {
            _craftingService = craftingService;
            _heroiService = heroiService;
        }

        public async Task ExecutarAsync(SocketSlashCommand command)
        {
            var acao = (string)command.Data.Options.First(o => o.Name == "acao").Value;

            switch (acao)
            {
                case "listar":
                    await ListarReceitasAsync(command);
                    break;
                case "fazer":
                    var receitaId = command.Data.Options.FirstOrDefault(o => o.Name == "receita")?.Value as string;
                    await FazerItemAsync(command, receitaId);
                    break;
                default:
                    await command.RespondAsync("Acao invalida.", ephemeral: true);
                    break;
            }
        }

        private async Task ListarReceitasAsync(SocketSlashCommand command)
        {
            var receitas = _craftingService.ListarReceitas();
            var sb = new StringBuilder();
            sb.AppendLine("**Receitas disponiveis:**\n");

            foreach (var r in receitas)
            {
                var custo = string.Join(", ", r.Custo.Select(kv => $"{kv.Value} {kv.Key}"));
                var bonus = string.Join(", ", r.BonusBase.Select(kv => $"+{kv.Value} {kv.Key}"));
                var slot = r.Slot switch
                {
                    SlotEquipamento.Arma      => "Arma",
                    SlotEquipamento.Armadura   => "Armadura",
                    SlotEquipamento.Acessorio  => "Acessorio",
                    _                          => r.Slot.ToString()
                };
                sb.AppendLine($"**{r.Nome}** (`{r.Id}`) [{slot}]");
                sb.AppendLine($"  Custo: {custo}");
                sb.AppendLine($"  Bonus: {bonus}\n");
            }

            await command.RespondAsync(sb.ToString(), ephemeral: true);
        }

        private async Task FazerItemAsync(SocketSlashCommand command, string? receitaId)
        {
            if (string.IsNullOrWhiteSpace(receitaId))
            {
                await command.RespondAsync("Informe o ID da receita. Use `/crafting acao:listar` para ver as opcoes.", ephemeral: true);
                return;
            }

            var (item, erro) = await _craftingService.CraftarAsync(command.User.Id, receitaId);

            if (erro != null)
            {
                await command.RespondAsync($"Erro: {erro}", ephemeral: true);
                return;
            }

            var bonusTexto = string.Join(", ", item!.Bonus.Select(b => $"+{b.Valor} {b.Atributo}"));
            await command.RespondAsync(
                $"Item **{item.Nome}** craftado com sucesso! ({item.Qualidade})\nBonus: {bonusTexto}\nID do item: `{item.Id}`",
                ephemeral: true);
        }
    }
}

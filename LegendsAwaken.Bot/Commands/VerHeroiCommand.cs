using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendsAwaken.Bot.Commands
{
    public class VerHeroiCommand
    {
        private readonly HeroiService _heroiService;

        public VerHeroiCommand(HeroiService heroiService)
        {
            _heroiService = heroiService;
        }
        public async Task ExecutarAsync(SocketSlashCommand command, string nomeHeroi)
        {
            var userId = command.User.Id;

            var herois = await _heroiService.ObterHeroisPorUsuarioAsync(userId);

            if (herois == null || !herois.Any())
            {
                await command.RespondAsync("Voc� n�o possui her�is cadastrados.", ephemeral: true);
                return;
            }

            // Busca o her�i pelo nome (case insensitive)
            var heroi = herois.FirstOrDefault(h => string.Equals(h.Nome, nomeHeroi, StringComparison.OrdinalIgnoreCase));
            if (heroi == null)
            {
                await command.RespondAsync($"Nenhum her�i encontrado com o nome '{nomeHeroi}'.", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Her�i: {heroi.Nome}")
                .WithDescription($"N�vel: {heroi.Nivel} | Raridade: {heroi.Raridade}")
                .AddField("Ra�a", heroi.Raca.ToString(), true)
                .AddField("Profiss�o", heroi.Profissao?.ToString() ?? "Nenhuma", true)
                .AddField("Atributos", MontarAtributos(heroi), false);

            var habilidadesTexto = MontarHabilidades(heroi);
            if (!string.IsNullOrEmpty(habilidadesTexto))
            {
                embedBuilder.AddField("Habilidades", habilidadesTexto, false);
            }

            embedBuilder.WithColor(Color.DarkBlue)
                .WithFooter("Legends Awaken")
                .WithCurrentTimestamp();

            await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
        }


        private string MontarAtributos(Domain.Entities.Heroi heroi)
        {
            var totalAtributos = heroi.ObterAtributosTotais(new Domain.Entities.AtributosBase());

            var sb = new StringBuilder();
            sb.AppendLine($"For�a: {totalAtributos.Forca}");
            sb.AppendLine($"Agilidade: {totalAtributos.Agilidade}");
            sb.AppendLine($"Vitalidade: {totalAtributos.Vitalidade}");
            sb.AppendLine($"Intelig�ncia: {totalAtributos.Inteligencia}");
            sb.AppendLine($"Percep��o: {totalAtributos.Percepcao}");
            sb.AppendLine($"Pontos Dispon�veis: {heroi.PontosAtributosDisponiveis}");

            return sb.ToString();
        }

        private string MontarHabilidades(Domain.Entities.Heroi heroi)
        {
            if (heroi.Habilidades == null || !heroi.Habilidades.Any())
                return "Nenhuma habilidade adquirida.";

            var sb = new StringBuilder();
            foreach (var habilidadeHeroi in heroi.Habilidades)
            {
                // Mostra o nome da habilidade + n�vel atual
                sb.AppendLine($"{habilidadeHeroi.Habilidade.Nome} - N�vel {habilidadeHeroi.Nivel}");
            }
            return sb.ToString();
        }
    }
}

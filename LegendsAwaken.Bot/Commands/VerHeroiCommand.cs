using Discord;
using Discord.WebSocket;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Extensions;
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
                await command.RespondAsync("Você não possui heróis cadastrados.", ephemeral: true);
                return;
            }

            // Busca o herói pelo nome (case insensitive)
            var heroi = herois.FirstOrDefault(h => string.Equals(h.Nome, nomeHeroi, StringComparison.OrdinalIgnoreCase));
            if (heroi == null)
            {
                await command.RespondAsync($"Nenhum herói encontrado com o nome '{nomeHeroi}'.", ephemeral: true);
                return;
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle($"Herói: {heroi.Nome}")
                .WithDescription($"Nível: {heroi.Nivel} | Raridade: {heroi.Raridade.ToStars()}")
                .AddField("Raça", heroi.Raca.ToString(), true)
                .AddField("Profissão", heroi.Profissao?.ToString() ?? "Nenhuma", true)
                .AddField("Atributos", MontarAtributos(heroi), false)
                .AddField("Status de Combate", MontarStatus(heroi), false)
                .WithColor(Color.DarkBlue)
                .WithFooter("Legends Awaken")
                .WithCurrentTimestamp();

            var habilidadesTexto = MontarHabilidades(heroi);
            if (!string.IsNullOrEmpty(habilidadesTexto))
            {
                embedBuilder.AddField("Habilidades", habilidadesTexto, false);
            }

            await command.RespondAsync(embed: embedBuilder.Build(), ephemeral: true);
        }
        private string MontarStatus(Domain.Entities.Heroi heroi)
        {
            var s = heroi.Status;
            return new StringBuilder()
                .AppendLine($"❤ Vida: {s.VidaAtual}/{s.VidaMaxima}")
                .AppendLine($"💧 Mana: {s.ManaAtual}/{s.ManaMaxima}")
                .ToString();
        }

        private string MontarAtributos(Domain.Entities.Heroi heroi)
        {
            var totalAtributos = heroi.ObterAtributosTotais(new Domain.Entities.AtributosBase());

            var sb = new StringBuilder();
            foreach (var (attr, valor) in totalAtributos.ToEnumerable())
                sb.AppendLine($"{NomeAtributo(attr)}: {valor}");

            sb.AppendLine($"Pontos Disponíveis: {heroi.PontosAtributosDisponiveis}");
            return sb.ToString();
        }

        // Display names for known attributes; unknown attributes fall back to enum name.
        // Update this when adding a new attribute if a localized label is needed.
        private static string NomeAtributo(Atributo attr) => attr switch
        {
            Atributo.Forca        => "Força",
            Atributo.Agilidade    => "Agilidade",
            Atributo.Vitalidade   => "Vitalidade",
            Atributo.Inteligencia => "Inteligência",
            Atributo.Percepcao    => "Percepção",
            _                     => attr.ToString()
        };

        private string MontarHabilidades(Domain.Entities.Heroi heroi)
        {
            if (heroi.Habilidades == null || !heroi.Habilidades.Any())
                return "Nenhuma habilidade adquirida.";

            var sb = new StringBuilder();
            foreach (var habilidadeHeroi in heroi.Habilidades)
            {
                // Mostra o nome da habilidade + nível atual
                sb.AppendLine($"{habilidadeHeroi.Habilidade.Nome} - Nível {habilidadeHeroi.Nivel}");
            }
            return sb.ToString();
        }
    }
}

using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface INotificacaoService
{
    Task NotificarEventoCheckpointAsync(ulong channelId, ulong discordUserId, TorreEvento evento);
    Task NotificarEventoMenorAsync(ulong channelId, ulong discordUserId, TorreEvento evento, string titulo, string descricaoResultado, int progressoBonus);
}

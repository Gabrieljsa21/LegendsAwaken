using LegendsAwaken.Domain.Entities;
using System.Threading.Tasks;

namespace LegendsAwaken.Domain.Interfaces;

public interface INotificacaoService
{
    Task NotificarEventoCheckpointAsync(ulong discordUserId, TorreEvento evento);
}

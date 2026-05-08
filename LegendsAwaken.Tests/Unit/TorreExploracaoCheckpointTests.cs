using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Enum;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class TorreExploracaoCheckpointTests
{
    [Fact]
    public void ThresholdParaFlag_RetornaFlagCorreta()
    {
        Assert.Equal(CheckpointFlags.P25,  TorreEventoService.ThresholdParaFlag(25));
        Assert.Equal(CheckpointFlags.P50,  TorreEventoService.ThresholdParaFlag(50));
        Assert.Equal(CheckpointFlags.P75,  TorreEventoService.ThresholdParaFlag(75));
        Assert.Equal(CheckpointFlags.P100, TorreEventoService.ThresholdParaFlag(100));
    }

    [Fact]
    public void CheckpointFlags_Bitmask_FuncionaCorretamente()
    {
        var flags = CheckpointFlags.P25 | CheckpointFlags.P50;
        Assert.True((flags & CheckpointFlags.P25) != 0);
        Assert.True((flags & CheckpointFlags.P50) != 0);
        Assert.False((flags & CheckpointFlags.P75) != 0);
        Assert.False((flags & CheckpointFlags.P100) != 0);
    }
}

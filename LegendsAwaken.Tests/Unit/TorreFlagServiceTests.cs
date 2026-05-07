using LegendsAwaken.Application.Services;
using LegendsAwaken.Infrastructure.Repositories;
using Moq;

namespace LegendsAwaken.Tests.Unit;

public class TorreFlagServiceTests
{
    private static (TorreFlagService svc, Mock<IAndarFlagProgressoRepository> repo) Criar()
    {
        var repo = new Mock<IAndarFlagProgressoRepository>();
        return (new TorreFlagService(repo.Object), repo);
    }

    [Fact]
    public async Task GerarFlag_DelegatesParaRepo()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();

        await svc.GerarFlagAsync(userId, 1, "grimorio_encontrado");

        repo.Verify(r => r.GerarFlagAsync(userId, 1, "grimorio_encontrado"), Times.Once);
    }

    [Fact]
    public async Task MarcarSecundarioExpirado_ChegaNoRepo_QuandoAndarTemSecundario()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();

        // Andar 1 tem secundário "grimorio_encontrado"
        await svc.MarcarSecundarioExpiradoAsync(userId, 1);

        repo.Verify(r => r.MarcarExpiradoAsync(userId, 1, "grimorio_encontrado"), Times.Once);
    }

    [Fact]
    public async Task MarcarSecundarioExpirado_NaoChegaNoRepo_QuandoAndarNaoTemSecundario()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();

        // Andar 4 é boss floor, sem secundário
        await svc.MarcarSecundarioExpiradoAsync(userId, 4);

        repo.Verify(r => r.MarcarExpiradoAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ObterFlagsCompostas_ReturnsComposta_QuandoComponentesAtivos()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();
        repo.Setup(r => r.ObterFlagsGeradasAsync(userId))
            .ReturnsAsync(new List<string> { "grimorio_encontrado", "diario_rasgado" });

        var compostas = await svc.ObterFlagsCompostasAtivasAsync(userId);

        Assert.Contains("identidade_revelada", compostas);
    }

    [Fact]
    public async Task ObterModificadoresBoss_SomaReducoes_QuandoFlagsAtivas()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();
        repo.Setup(r => r.ObterFlagsGeradasAsync(userId))
            .ReturnsAsync(new List<string> { "grimorio_encontrado" });

        var (total, descricoes) = await svc.ObterModificadoresBossAsync(userId, 4);

        Assert.Equal(0.10, total, precision: 2);
        Assert.Single(descricoes);
    }

    [Fact]
    public async Task ObterModificadoresBoss_IncluiComposta_QuandoComponentesAtivos()
    {
        var (svc, repo) = Criar();
        var userId = Guid.NewGuid();
        // grimorio_encontrado (+10%) + diario_rasgado → identidade_revelada (+5%) = 15%
        repo.Setup(r => r.ObterFlagsGeradasAsync(userId))
            .ReturnsAsync(new List<string> { "grimorio_encontrado", "diario_rasgado" });

        var (total, _) = await svc.ObterModificadoresBossAsync(userId, 4);

        Assert.Equal(0.15, total, precision: 2);
    }
}

using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities.Fragmento;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using Xunit;

namespace LegendsAwaken.Tests.Services;

public class BiomeServiceTests
{
    private readonly Mock<IBiomaRepository> _repoMock = new();
    private BiomeService CreateService() => new(_repoMock.Object);

    [Theory]
    [InlineData(1,  "Floresta de Aelindra")]
    [InlineData(5,  "Floresta de Aelindra")]
    [InlineData(10, "Floresta de Aelindra")]
    [InlineData(11, "Ruínas de Valdrek")]
    [InlineData(25, "Ruínas de Valdrek")]
    [InlineData(26, "Pico Vulcânico")]
    public async Task ObterBiomaPorAndarAsync_RetornaBiomaCorreto(int andar, string nomeEsperado)
    {
        _repoMock.Setup(r => r.ObterPorAndarAsync(andar))
            .ReturnsAsync(new Bioma { Nome = nomeEsperado, AndarInicio = 1, AndarFim = 10 });

        var service = CreateService();
        var bioma = await service.ObterBiomaPorAndarAsync(andar);

        Assert.Equal(nomeEsperado, bioma?.Nome);
    }

    [Theory]
    [InlineData(5,  true)]
    [InlineData(10, true)]
    [InlineData(15, true)]
    [InlineData(20, true)]
    [InlineData(25, true)]
    [InlineData(30, true)]
    [InlineData(3,  false)]
    [InlineData(11, false)]
    [InlineData(7,  false)]
    public void EAndarDeMarco_RetornaCorreto(int andar, bool esperado)
    {
        var service = CreateService();
        Assert.Equal(esperado, service.EAndarDeMarco(andar));
    }

    [Fact]
    public async Task EBiomaNovoAsync_RetornaTrue_QuandoBiomaMuda()
    {
        var biomaA = new Bioma { Id = Guid.NewGuid(), AndarInicio = 1,  AndarFim = 10 };
        var biomaB = new Bioma { Id = Guid.NewGuid(), AndarInicio = 11, AndarFim = 25 };

        _repoMock.Setup(r => r.ObterPorAndarAsync(11)).ReturnsAsync(biomaB);
        _repoMock.Setup(r => r.ObterPorAndarAsync(10)).ReturnsAsync(biomaA);

        var service = CreateService();
        var resultado = await service.EBiomaNovoAsync(11);

        Assert.True(resultado);
    }

    [Fact]
    public async Task EBiomaNovoAsync_RetornaFalse_QuandoBiomaNaoMuda()
    {
        var bioma = new Bioma { Id = Guid.NewGuid(), AndarInicio = 1, AndarFim = 10 };
        _repoMock.Setup(r => r.ObterPorAndarAsync(It.IsAny<int>())).ReturnsAsync(bioma);

        var service = CreateService();
        var resultado = await service.EBiomaNovoAsync(5);

        Assert.False(resultado);
    }
}

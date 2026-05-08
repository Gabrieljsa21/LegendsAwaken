using LegendsAwaken.Application.Config;
using LegendsAwaken.Application.Services;
using LegendsAwaken.Domain.Entities;
using LegendsAwaken.Domain.Enum;
using LegendsAwaken.Domain.Interfaces;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace LegendsAwaken.Tests.Unit;

public class TorreEventoServiceTests
{
    private readonly Mock<ITorreEventoRepository> _eventoRepo = new();
    private readonly Mock<ITorreExploracaoRepository> _exploracaoRepo = new();
    private TorreEventoService CreateService() =>
        new(_eventoRepo.Object, _exploracaoRepo.Object);

    private static TorreExploracao CriarExploracao(int andar = 5, int seed = 42) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        DiscordUserId = 123456789UL,
        AndarNumero = andar,
        Progresso = 25,
        Seed = seed,
        CheckpointsProcessados = CheckpointFlags.None,
        Status = StatusExploracao.Ativa,
        HeroisIds = "",
        ConsequenceTags = null
    };

    [Fact]
    public async Task GerarEventoAsync_RetornaEvento_ParaThreshold25()
    {
        var svc = CreateService();
        var exp = CriarExploracao();

        var evento = await svc.GerarEventoAsync(exp, threshold: 25);

        Assert.NotNull(evento);
        Assert.Equal(25, evento.ProgressoNoCheckpoint);
        Assert.Equal(exp.AndarNumero, evento.AndarOrigem);
        Assert.Equal(EventoStatus.Ativo, evento.Status);
    }

    [Fact]
    public async Task GerarEventoAsync_MesmoSeed_ProduceMesmoEvento()
    {
        var svc = CreateService();
        var exp1 = CriarExploracao(seed: 100);
        var exp2 = CriarExploracao(seed: 100);

        var e1 = await svc.GerarEventoAsync(exp1, 25);
        var e2 = await svc.GerarEventoAsync(exp2, 25);

        Assert.Equal(e1.EventoKey, e2.EventoKey);
        Assert.Equal(e1.EventoSeed, e2.EventoSeed);
    }

    [Fact]
    public async Task GerarEventoAsync_LancaInvalidOperation_SeThresholdJaProcessado()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        exp.CheckpointsProcessados = CheckpointFlags.P25;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.GerarEventoAsync(exp, 25));
    }

    [Fact]
    public async Task ResolverAsync_SetsStatus_Resolvido()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        exp.Status = StatusExploracao.AguardandoEscolha;
        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            ExploracaoId = exp.Id,
            Status = EventoStatus.Ativo,
            Tipo = TipoEvento.BlockingChoice,
            EventoKey = "encruzilhada_mercador",
            Tier = TierEvento.Maior,
            Exploracao = exp
        };
        _eventoRepo.Setup(r => r.ObterAtivoAsync(exp.Id)).ReturnsAsync(evento);

        await svc.ResolverAsync(evento.Id, "pagar", exp);

        Assert.Equal(EventoStatus.Resolvido, evento.Status);
        Assert.Equal(StatusExploracao.Ativa, exp.Status);
        Assert.Equal("pagar", evento.OpcaoKey);
        Assert.NotNull(evento.ResolvidoEm);
        _eventoRepo.Verify(r => r.AtualizarAsync(evento), Times.Once);
        _exploracaoRepo.Verify(r => r.AtualizarAsync(exp), Times.Once);
    }

    [Fact]
    public async Task ResolverAsync_LancaException_SeOpcaoKeyInvalida()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            EventoKey = "encruzilhada_mercador",
            Status = EventoStatus.Ativo,
            Tipo = TipoEvento.BlockingChoice,
            Exploracao = exp
        };
        _eventoRepo.Setup(r => r.ObterAtivoAsync(exp.Id)).ReturnsAsync(evento);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.ResolverAsync(evento.Id, "opcao_invalida", exp));
    }

    [Fact]
    public async Task ResolverAsync_NaoUltrapassaProximoCheckpoint_ComBonusProgresso()
    {
        var svc = CreateService();
        var exp = CriarExploracao();
        exp.Progresso = 25;
        exp.CheckpointsProcessados = CheckpointFlags.P25; // 50 é o próximo
        var evento = new TorreEvento
        {
            Id = Guid.NewGuid(),
            EventoKey = "trilha_oculta",
            Status = EventoStatus.Ativo,
            Tipo = TipoEvento.BlockingChoice,
            Exploracao = exp
        };
        _eventoRepo.Setup(r => r.ObterAtivoAsync(exp.Id)).ReturnsAsync(evento);

        await svc.ResolverAsync(evento.Id, "explorar", exp);

        // Bônus de explorar trilha_oculta não pode levar Progresso >= 50
        Assert.True(exp.Progresso < 50, $"Progresso foi para {exp.Progresso}, deveria ficar abaixo de 50");
    }
}

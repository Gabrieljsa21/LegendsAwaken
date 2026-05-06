using Discord.WebSocket;
using LegendsAwaken.Bot.Interactions;
using Moq;

namespace LegendsAwaken.Tests;

public class InteractionRouterTests
{
    [Fact]
    public void CanRoute_ReturnsFalse_WhenNoColon()
    {
        var router = new InteractionRouter();
        Assert.False(router.CanRoute("cidade_coletar"));
    }

    [Fact]
    public void CanRoute_ReturnsFalse_WhenPrefixNotRegistered()
    {
        var router = new InteractionRouter();
        Assert.False(router.CanRoute("cidade:coletar"));
    }

    [Fact]
    public void CanRoute_ReturnsTrue_WhenPrefixRegistered()
    {
        var router = new InteractionRouter();
        var handler = new Mock<IInteractionHandler>();
        handler.Setup(h => h.CustomIdPrefix).Returns("cidade");
        router.Register(handler.Object);

        Assert.True(router.CanRoute("cidade:coletar"));
    }

    [Fact]
    public void ParseParts_SplitsOnColon()
    {
        var parts = InteractionRouter.ParseParts("cidade:node_para_heroi:abc-123");
        Assert.Equal(new[] { "cidade", "node_para_heroi", "abc-123" }, parts);
    }

    [Fact]
    public void Register_OverwritesPreviousHandler_ForSamePrefix()
    {
        var router = new InteractionRouter();
        var h1 = new Mock<IInteractionHandler>();
        h1.Setup(h => h.CustomIdPrefix).Returns("cidade");
        var h2 = new Mock<IInteractionHandler>();
        h2.Setup(h => h.CustomIdPrefix).Returns("cidade");

        router.Register(h1.Object);
        router.Register(h2.Object);

        Assert.True(router.CanRoute("cidade:coletar"));
    }
}

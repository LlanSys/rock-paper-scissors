using FluentAssertions;

using Microsoft.Extensions.Options;

using Rps.Api.Http.Domain.Contracts.Implementations;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Handlers.CommandHandlers;
using Rps.Api.Http.Shared.Options;

using Rps.Api.UnitTests.Fakes;

using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace Rps.Api.UnitTests.Features.PlaySingle.v2;

public class PlaySingleHandlerTests
{
    private readonly StubComputerMoveSelector _stubSelector = new();
    private readonly GameEngine _engine = new(Options.Create(new GameOptions()));

    [Fact]
    public async Task PlaySingle_WhenRequestIsValid_ReturnsSuccessResponse()
    {
        // Arrange
        var handler = new PlaySingleCommandHandler(_engine, _stubSelector);
        var request = new PlaySingleCommand(new PlayRequest("fire"));
        _stubSelector.NextMove = Move.Wood; // Fire beats Wood

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PlayerMove.Should().Be("Fire");
        result.Data.ComputerMove.Should().Be("Wood");
        result.Data.Outcome.Should().Be("Winner");
        result.Data.Message.Should().Contain("Winner!");
    }
}

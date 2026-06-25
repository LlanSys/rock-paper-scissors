using FluentAssertions;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlaySingle.v2.Contracts.Interfaces;
using Rps.Api.Http.Features.PlaySingle.v2.Contracts.Implementations;
using Rps.Api.Http.Features.PlayMultiple.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Features.PlayMultiple.v2.Hanlders.CommandHandlers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

using Microsoft.Extensions.Options;
using Rps.Api.Http.Shared.Options;

namespace Rps.Api.UnitTests.Features.PlayMultiple.v2;

public class PlayMultipleHandlerTests
{
    private readonly StubComputerMoveSelector _stubSelector = new();
    private readonly GameEngine _engine = new(Options.Create(new GameOptions()));

    [Fact]
    public async Task PlayMultiple_WhenBatchIsValid_ReturnsAllOutcomes()
    {
        // Arrange
        var handler = new PlayMultipleCommandHandler(_engine, _stubSelector);
        var request = new PlayMultipleCommand(new PlayMultipleRequest(["fire", "water"]));
        _stubSelector.MoveSequence.Enqueue(Move.Wood);   // Fire vs Wood -> Winner
        _stubSelector.MoveSequence.Enqueue(Move.Earth);  // Water vs Earth -> Loser

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);

        result.Data![0].PlayerMove.Should().Be("Fire");
        result.Data[0].ComputerMove.Should().Be("Wood");
        result.Data[0].Outcome.Should().Be("Winner");

        result.Data[1].PlayerMove.Should().Be("Water");
        result.Data[1].ComputerMove.Should().Be("Earth");
        result.Data[1].Outcome.Should().Be("Loser");
    }
}

public class StubComputerMoveSelector : IComputerMoveSelector
{
    public Move NextMove { get; set; } = Move.Fire;
    public Queue<Move> MoveSequence { get; } = new();

    public async Task<Move> SelectMoveAsync()
    {
        await Task.CompletedTask;
        if (MoveSequence.Count > 0)
        {
            return MoveSequence.Dequeue();
        }
        return NextMove;
    }
}

using FluentAssertions;

using Microsoft.Extensions.Options;

using Rps.Api.Http.Domain.Contracts.Implementations;
using Rps.Api.Http.Domain.Enums.Common;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Shared.Options;

using System.Threading.Tasks;

using Xunit;

namespace Rps.Api.UnitTests.Features.PlaySingle.v2;

public class GameEngineTests
{
    private readonly GameEngine _engine = new(Options.Create(new GameOptions()));

    [Theory]
    // The 10 winning matchups
    [InlineData(Move.Fire, Move.Wood, GameOutcome.Winner, "burns")]
    [InlineData(Move.Fire, Move.Metal, GameOutcome.Winner, "melts")]
    [InlineData(Move.Wood, Move.Earth, GameOutcome.Winner, "displaces")]
    [InlineData(Move.Wood, Move.Water, GameOutcome.Winner, "absorbs")]
    [InlineData(Move.Water, Move.Fire, GameOutcome.Winner, "extinguishes")]
    [InlineData(Move.Water, Move.Metal, GameOutcome.Winner, "rusts")]
    [InlineData(Move.Earth, Move.Fire, GameOutcome.Winner, "extinguishes")]
    [InlineData(Move.Earth, Move.Water, GameOutcome.Winner, "absorbs")]
    [InlineData(Move.Metal, Move.Wood, GameOutcome.Winner, "cuts")]
    [InlineData(Move.Metal, Move.Earth, GameOutcome.Winner, "displaces")]
    public async Task Evaluate_WhenPlayerMoveDefeatsComputerMove_ReturnsWinner(
        Move playerMove, Move computerMove, GameOutcome expectedOutcome, string expectedVerb)
    {
        // Act
        var result = await _engine.EvaluateAsync(playerMove, computerMove);

        // Assert
        result.Outcome.Should().Be(expectedOutcome);
        result.Message.Should().Contain("Winner!");
        result.Message.Should().Contain($"{playerMove} {expectedVerb} {computerMove}");
    }

    [Theory]
    // The 10 losing matchups (inverse outcomes)
    [InlineData(Move.Wood, Move.Fire, GameOutcome.Loser, "burns")]
    [InlineData(Move.Metal, Move.Fire, GameOutcome.Loser, "melts")]
    [InlineData(Move.Earth, Move.Wood, GameOutcome.Loser, "displaces")]
    [InlineData(Move.Water, Move.Wood, GameOutcome.Loser, "absorbs")]
    [InlineData(Move.Fire, Move.Water, GameOutcome.Loser, "extinguishes")]
    [InlineData(Move.Metal, Move.Water, GameOutcome.Loser, "rusts")]
    [InlineData(Move.Fire, Move.Earth, GameOutcome.Loser, "extinguishes")]
    [InlineData(Move.Water, Move.Earth, GameOutcome.Loser, "absorbs")]
    [InlineData(Move.Wood, Move.Metal, GameOutcome.Loser, "cuts")]
    [InlineData(Move.Earth, Move.Metal, GameOutcome.Loser, "displaces")]
    public async Task Evaluate_WhenComputerMoveDefeatsPlayerMove_ReturnsLoser(
        Move playerMove, Move computerMove, GameOutcome expectedOutcome, string expectedVerb)
    {
        // Act
        var result = await _engine.EvaluateAsync(playerMove, computerMove);

        // Assert
        result.Outcome.Should().Be(expectedOutcome);
        result.Message.Should().Contain("Loser!");
        result.Message.Should().Contain($"{computerMove} {expectedVerb} {playerMove}");
    }

    [Theory]
    // The 5 draw matchups
    [InlineData(Move.Fire)]
    [InlineData(Move.Water)]
    [InlineData(Move.Wood)]
    [InlineData(Move.Metal)]
    [InlineData(Move.Earth)]
    public async Task Evaluate_WhenMovesAreEqual_ReturnsDraw(Move move)
    {
        // Act
        var result = await _engine.EvaluateAsync(move, move);

        // Assert
        result.Outcome.Should().Be(GameOutcome.Draw);
        result.Message.Should().Contain("It's a draw!");
    }
}

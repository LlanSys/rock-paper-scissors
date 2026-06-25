using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Rps.Api.Http.Domain.Contracts.Interfaces;
using Rps.Api.Http.Domain.Enums.Common;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Shared.Dtos;
using Rps.Api.Http.Shared.Options;

namespace Rps.Api.Http.Domain.Contracts.Implementations;

public class GameEngine(IOptions<GameOptions> options) : IGameEngine
{
    private readonly GameOptions _options = options.Value;

    public async Task<GameResult> EvaluateAsync(Move playerMove, Move computerMove)
    {
        await Task.Yield();

        if (playerMove == computerMove)
        {
            return new GameResult(GameOutcome.Draw, $"I play {computerMove}. {_options.DrawTemplate}");
        }

        if (Beats(playerMove, computerMove))
        {
            var verb = GetVerb(playerMove, computerMove);
            return new GameResult(GameOutcome.Winner, $"I play {computerMove}. {_options.WinnerTemplate} {playerMove} {verb} {computerMove}.");
        }
        else
        {
            var verb = GetVerb(computerMove, playerMove);
            return new GameResult(GameOutcome.Loser, $"I play {computerMove}. {_options.LoserTemplate} {computerMove} {verb} {playerMove}.");
        }
    }

    private static bool Beats(Move attacker, Move defender) => (attacker, defender) switch
    {
        (Move.Fire, Move.Wood) => true,
        (Move.Fire, Move.Metal) => true,
        (Move.Wood, Move.Earth) => true,
        (Move.Wood, Move.Water) => true,
        (Move.Water, Move.Fire) => true,
        (Move.Water, Move.Metal) => true,
        (Move.Earth, Move.Fire) => true,
        (Move.Earth, Move.Water) => true,
        (Move.Metal, Move.Wood) => true,
        (Move.Metal, Move.Earth) => true,
        _ => false,
    };

    private static string GetVerb(Move winner, Move loser) => (winner, loser) switch
    {
        (Move.Fire, Move.Wood) => "burns",
        (Move.Fire, Move.Metal) => "melts",
        (Move.Wood, Move.Earth) => "displaces",
        (Move.Wood, Move.Water) => "absorbs",
        (Move.Water, Move.Fire) => "extinguishes",
        (Move.Water, Move.Metal) => "rusts",
        (Move.Earth, Move.Fire) => "extinguishes",
        (Move.Earth, Move.Water) => "absorbs",
        (Move.Metal, Move.Wood) => "cuts",
        (Move.Metal, Move.Earth) => "displaces",
        _ => throw new ArgumentOutOfRangeException(nameof(winner), $"No verb defined for {winner} vs {loser}."),
    };
}

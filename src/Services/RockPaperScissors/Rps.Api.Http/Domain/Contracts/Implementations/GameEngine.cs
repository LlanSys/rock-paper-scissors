using System;

using Microsoft.Extensions.Options;

using Rps.Api.Http.Domain.Contracts.Interfaces;
using Rps.Api.Http.Domain.Enums.Common;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Domain.Models;
using Rps.Api.Http.Shared.Options;

namespace Rps.Api.Http.Domain.Contracts.Implementations;

public class GameEngine(IOptions<GameOptions> options) : IGameEngine
{
    private readonly GameOptions _options = options.Value;

    public Task<GameResult> EvaluateAsync(Move playerMove, Move computerMove)
    {

        if (playerMove == computerMove)
        {
            return Task.FromResult(new GameResult(GameOutcome.Draw, FormatTemplate(_options.DrawTemplate, playerMove, computerMove, null)));
        }

        if (Beats(playerMove, computerMove))
        {
            var verb = GetVerb(playerMove, computerMove);
            return Task.FromResult(new GameResult(GameOutcome.Winner, FormatTemplate(_options.WinnerTemplate, playerMove, computerMove, verb)));
        }
        else
        {
            var verb = GetVerb(computerMove, playerMove);
            return Task.FromResult(new GameResult(GameOutcome.Loser, FormatTemplate(_options.LoserTemplate, playerMove, computerMove, verb)));
        }
    }

    private static string FormatTemplate(string template, Move playerMove, Move computerMove, string? verb)
    {
        var result = template
            .Replace("{PlayerMove}", playerMove.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("{ComputerMove}", computerMove.ToString(), StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrEmpty(verb))
        {
            result = result.Replace("{Verb}", verb, StringComparison.OrdinalIgnoreCase);
        }

        return result;
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

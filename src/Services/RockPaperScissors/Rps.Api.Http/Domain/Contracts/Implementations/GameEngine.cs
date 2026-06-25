using System;
using System.Collections.Generic;
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

    private static readonly Dictionary<Move, HashSet<Move>> Beats = new()
    {
        { Move.Fire, [Move.Wood, Move.Metal] },
        { Move.Wood, [Move.Earth, Move.Water] },
        { Move.Water, [Move.Fire, Move.Metal] },
        { Move.Earth, [Move.Fire, Move.Water] },
        { Move.Metal, [Move.Wood, Move.Earth] }
    };

    private static readonly Dictionary<(Move Winner, Move Loser), string> Verbs = new()
    {
        { (Move.Fire, Move.Wood), "burns" },
        { (Move.Fire, Move.Metal), "melts" },
        { (Move.Wood, Move.Earth), "displaces" },
        { (Move.Wood, Move.Water), "absorbs" },
        { (Move.Water, Move.Fire), "extinguishes" },
        { (Move.Water, Move.Metal), "rusts" },
        { (Move.Earth, Move.Fire), "extinguishes" },
        { (Move.Earth, Move.Water), "absorbs" },
        { (Move.Metal, Move.Wood), "cuts" },
        { (Move.Metal, Move.Earth), "displaces" }
    };

    public Task<GameResult> EvaluateAsync(Move playerMove, Move computerMove)
    {
        if (playerMove == computerMove)
        {
            return Task.FromResult(new GameResult(GameOutcome.Draw, $"I play {computerMove}. {_options.DrawTemplate}"));
        }

        if (Beats[playerMove].Contains(computerMove))
        {
            var verb = Verbs[(playerMove, computerMove)];
            return Task.FromResult(new GameResult(GameOutcome.Winner, $"I play {computerMove}. {_options.WinnerTemplate} {playerMove} {verb} {computerMove}."));
        }
        else
        {
            var verb = Verbs[(computerMove, playerMove)];
            return Task.FromResult(new GameResult(GameOutcome.Loser, $"I play {computerMove}. {_options.LoserTemplate} {computerMove} {verb} {playerMove}."));
        }
    }
}

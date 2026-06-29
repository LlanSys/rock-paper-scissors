using System;

using Rps.Api.Http.Domain.Models;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Shared.Dtos;
using Rps.Api.Http.Shared.Extensions;

namespace Rps.Api.Http.Shared.Mappings;

public static class GameMapping
{
    public static PlayResponse ToPlayResponse(this GameResult result, Move playerMove, Move computerMove)
    {
        ArgumentNullException.ThrowIfNull(result, nameof(result));
        
        return new PlayResponse(
            PlayerMove: playerMove.ToDisplayString(),
            ComputerMove: computerMove.ToDisplayString(),
            Outcome: result.Outcome.ToDisplayString(),
            Message: result.Message
        );
    }
}

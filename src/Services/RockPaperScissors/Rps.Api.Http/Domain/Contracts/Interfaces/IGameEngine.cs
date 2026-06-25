using Rps.Api.Http.Domain.Enums.Common;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Shared.Dtos;

using System.Threading.Tasks;

namespace Rps.Api.Http.Domain.Contracts.Interfaces;

public interface IGameEngine
{
    Task<GameResult> EvaluateAsync(Move playerMove, Move computerMove);
}



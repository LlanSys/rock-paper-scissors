using MediatR;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlaySingle.v2.Contracts.Interfaces;
using Rps.Api.Http.Features.PlayMultiple.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Mappings;
using Rps.Api.Http.Shared.Dtos;
using Rps.Api.Http.Shared.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Features.PlayMultiple.v2.Hanlders.CommandHandlers;

public sealed record PlayMultipleCommand(PlayMultipleRequest PlayMultipleRequest) : IRequest<AppResponse<IReadOnlyList<PlayResponse>>>;

internal sealed class PlayMultipleCommandHandler(IGameEngine _gameEngine, IComputerMoveSelector _moveSelector) 
    : IRequestHandler<PlayMultipleCommand, AppResponse<IReadOnlyList<PlayResponse>>>
{
    public async Task<AppResponse<IReadOnlyList<PlayResponse>>> Handle(PlayMultipleCommand request, CancellationToken cancellationToken)
    {
        var results = new List<PlayResponse>();
        foreach (var action in request.PlayMultipleRequest.Actions)
        {
            var playerMove = action.ToEnum<Move>();
            var computerMove = await _moveSelector.SelectMoveAsync();
            var result = await _gameEngine.EvaluateAsync(playerMove, computerMove);

            results.Add(result.ToPlayResponse(playerMove, computerMove));
        }

        return AppResponse.Success<IReadOnlyList<PlayResponse>>(results);
    }
}

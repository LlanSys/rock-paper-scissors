using MediatR;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlaySingle.v2.Contracts.Interfaces;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Mappings;
using Rps.Api.Http.Shared.Dtos;
using Rps.Api.Http.Shared.Extensions;
using System.Threading;
using System.Threading.Tasks;

namespace Rps.Api.Http.Features.PlaySingle.v2.Hanlders.CommandHandlers;

public sealed record PlaySingleCommand(PlayRequest PlayRequest) : IRequest<AppResponse<PlayResponse>>;

internal sealed class PlaySingleCommandHandler(
    IGameEngine _gameEngine,
    IComputerMoveSelector _moveSelector) 
    : IRequestHandler<PlaySingleCommand, AppResponse<PlayResponse>>
{
    public async Task<AppResponse<PlayResponse>> Handle(PlaySingleCommand request, CancellationToken cancellationToken)
    {
        var playerMove = request.PlayRequest.Action.ToEnum<Move>();
        var computerMove = await _moveSelector.SelectMoveAsync();
        var result = await _gameEngine.EvaluateAsync(playerMove, computerMove);

        var response = result.ToPlayResponse(playerMove, computerMove);
        return AppResponse.Success(response);
    }
}

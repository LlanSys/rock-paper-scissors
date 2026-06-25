using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rps.Api.Http.Features.PlaySingle.v2.Dtos;
using Rps.Api.Http.Features.PlaySingle.v2.Hanlders.CommandHandlers;
using Rps.Api.Http.Shared.Controllers;
using Rps.Api.Http.Shared.Dtos;
using System.Threading.Tasks;

namespace Rps.Api.Http.Features.PlaySingle.v2.Controllers;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/game")]
[ApiController]
public class GameController(ISender sender) : BaseController
{
    [HttpPost("playsingle")]
    public async Task<ActionResult<AppResponse<PlayResponse>>> PlaySingle([FromBody] PlayRequest request)
    {
        var result = await sender.Send(new PlaySingleCommand(request));
        return result.IsSuccess ? Ok(result) : HandleFailure(result);
    }
}

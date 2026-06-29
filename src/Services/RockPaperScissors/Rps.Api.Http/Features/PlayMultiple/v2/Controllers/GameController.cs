using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Rps.Api.Http.Features.PlayMultiple.v2.Dtos;
using Rps.Api.Http.Features.PlayMultiple.v2.Handlers.CommandHandlers;
using Rps.Api.Http.Shared.Controllers;
using Rps.Api.Http.Shared.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rps.Api.Http.Features.PlayMultiple.v2.Controllers;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/game")]
[ApiController]
public class GameController(ISender sender) : BaseController
{
    [HttpPost("playmultiple")]
    public async Task<ActionResult<AppResponse<IReadOnlyList<PlayResponse>>>> PlayMultiple([FromBody] PlayMultipleRequest request)
    {
        var result = await sender.Send(new PlayMultipleCommand(request));
        return result.IsSuccess ? Ok(result) : HandleFailure(result);
    }
}

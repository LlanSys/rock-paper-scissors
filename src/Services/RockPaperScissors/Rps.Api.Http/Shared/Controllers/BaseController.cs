using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rps.Api.Http.Shared.Dtos;
using System;
using System.Collections.Generic;

namespace Rps.Api.Http.Shared.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult HandleFailure<T>(AppResponse<T> response)
    {
        if (response.IsSuccess || response.Error == null)
        {
            throw new InvalidOperationException("Cannot handle failure for a successful response.");
        }

        var error = response.Error;
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            ErrorType.DependencyUnavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type.ToString(),
            Detail = error.Message,
            Extensions = new Dictionary<string, object?>
            {
                { "errorCode", error.Code }
            }
        };

        if (error.ValidationErrors != null)
        {
            problemDetails.Extensions.Add("errors", error.ValidationErrors);
        }

        return StatusCode(statusCode, problemDetails);
    }
}

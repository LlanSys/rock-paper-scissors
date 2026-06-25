using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Rps.Api.Http.Features.PlaySingle.v1.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rps.Api.Http.Features.PlaySingle.v1.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/game")]
[ApiController]
public class GameController : ControllerBase
{
    private static readonly string[] _actions = ["rock", "paper", "scissors"];

    [HttpPost]
    [Route("playsingle")]
    public string PlaySingle([FromBody] PlayRequest request)
    {
        string playerAction = request.Action?.ToLowerInvariant() ?? string.Empty;

        if (!_actions.Contains(playerAction))
        {
            return "You have to play rock, paper or scissors";
        }

        string computerAction = Random.Shared.GetItems(_actions, 1)[0];

        if (computerAction == playerAction)
        {
            return "I play " + computerAction + ". It's a draw!";
        }

        if (computerAction == "rock" && playerAction == "paper")
        {
            return "I play " + computerAction + ". Winner!";
        }
        else if (computerAction == "rock" && playerAction == "scissors")
        {
            return "I play " + computerAction + ". Loser!";
        }
        else if (computerAction == "paper" && playerAction == "scissors")
        {
            return "I play " + computerAction + ". Winner!";
        }
        else if (computerAction == "paper" && playerAction == "rock")
        {
            return "I play " + computerAction + ". Loser!";
        }
        else if (computerAction == "scissors" && playerAction == "paper")
        {
            return "I play " + computerAction + ". Loser!";
        }
        else if (computerAction == "scissors" && playerAction == "rock")
        {
            return "I play " + computerAction + ". Winner!";
        }

        return "You have to play rock, paper or scissors";
    }
}

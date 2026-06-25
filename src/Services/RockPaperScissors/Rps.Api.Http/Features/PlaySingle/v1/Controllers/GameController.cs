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
    private readonly List<string> actions = new List<string>();

    [HttpPost]
    [Route("playsingle")]
    public string PlaySingle([FromBody] PlayRequest request)
    {
        actions.Add("rock");
        actions.Add("paper");
        actions.Add("scissors");

        string action = actions.OrderBy(a => new Random().Next()).First();

        if (action == request.Action)
        {
            return "I play " + action + ". It's a draw!";
        }

        if (action == "rock" && request.Action == "paper")
        {
            return "I play " + action + ". Winner!";
        }
        if (action == "rock" && request.Action == "scissors")
        {
            return "I play " + action + ". Loser!";
        }
        if (action == "paper" && request.Action == "scissors")
        {
            return "I play " + action + ". Winner!";
        }
        if (action == "paper" && request.Action == "rock")
        {
            return "I play " + action + ". Loser!";
        }
        if (action == "scissors" && request.Action == "paper")
        {
            return "I play " + action + ". Loser!";
        }
        if (action == "scissors" && request.Action == "rock")
        {
            return "I play " + action + ". Winner!";
        }

        return "You have to play rock, paper or scissors";
    }
}

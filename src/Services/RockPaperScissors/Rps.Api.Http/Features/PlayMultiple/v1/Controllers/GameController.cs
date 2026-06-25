using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Rps.Api.Http.Features.PlayMultiple.v1.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rps.Api.Http.Features.PlayMultiple.v1.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/game")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly List<string> actions = [];

    [HttpPost]
    [Route("playmultiple")]
    public string[] PlayMultiple([FromBody] MultiplePlayRequest request)
    {
        actions.Add("rock");
        actions.Add("paper");
        actions.Add("scissors");

        string action = actions.OrderBy(a => new Random().Next()).First();

        List<string> results = [];
        
        if (request?.Actions != null)
        {
            foreach (string player in request.Actions)
            {
                if (action == player)
                {
                    results.Add("I play " + action + ". It's a draw!");
                }

                if (action == "rock" && player == "paper")
                {
                    results.Add("I play " + action + ". Winner!");
                }
                if (action == "rock" && player == "scissors")
                {
                    results.Add("I play " + action + ". Loser!");
                }
                if (action == "paper" && player == "scissors")
                {
                    results.Add("I play " + action + ". Winner!");
                }
                if (action == "paper" && player == "rock")
                {
                    results.Add("I play " + action + ". Loser!");
                }
                if (action == "scissors" && player == "paper")
                {
                    results.Add("I play " + action + ". Loser!");
                }
                if (action == "scissors" && player == "rock")
                {
                    results.Add("I play " + action + ". Winner!");
                }

                results.Add("You have to play rock, paper or scissors");
            }
        }

        return [.. results];
    }
}

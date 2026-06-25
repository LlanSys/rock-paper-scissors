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
    private static readonly string[] _actions = ["rock", "paper", "scissors"];

    [HttpPost]
    [Route("playmultiple")]
    public string[] PlayMultiple([FromBody] MultiplePlayRequest request)
    {
        List<string> results = [];
        
        if (request?.Actions != null)
        {
            foreach (string rawPlayer in request.Actions)
            {
                string playerAction = rawPlayer?.ToLowerInvariant() ?? string.Empty;

                if (!_actions.Contains(playerAction))
                {
                    results.Add("You have to play rock, paper or scissors");
                    continue;
                }

                // Generates a new random move for each item in the array, as requested (zero-allocation implementation).
                //string computerAction = Random.Shared.GetItems(_actions, 1)[0];

                string computerAction = _actions[Random.Shared.Next(_actions.Length)];

                if (computerAction == playerAction)
                {
                    results.Add("I play " + computerAction + ". It's a draw!");
                }
                else if (computerAction == "rock" && playerAction == "paper")
                {
                    results.Add("I play " + computerAction + ". Winner!");
                }
                else if (computerAction == "rock" && playerAction == "scissors")
                {
                    results.Add("I play " + computerAction + ". Loser!");
                }
                else if (computerAction == "paper" && playerAction == "scissors")
                {
                    results.Add("I play " + computerAction + ". Winner!");
                }
                else if (computerAction == "paper" && playerAction == "rock")
                {
                    results.Add("I play " + computerAction + ". Loser!");
                }
                else if (computerAction == "scissors" && playerAction == "paper")
                {
                    results.Add("I play " + computerAction + ". Loser!");
                }
                else if (computerAction == "scissors" && playerAction == "rock")
                {
                    results.Add("I play " + computerAction + ". Winner!");
                }
            }
        }

        return [.. results];
    }
}

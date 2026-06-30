using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using Rps.Api.Http.Features.PlaySingle.v1.Dtos;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

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
        // ToDo: REMOVE - This creates a brand-new string object in memory every time someone calls the API.
        //string playerAction = request.Action?.ToLowerInvariant() ?? string.Empty;
        
        string playerAction = request.Action ?? string.Empty;

        if (string.IsNullOrEmpty(playerAction) || !IsValidAction(playerAction))
        {
            return "You have to play rock, paper or scissors";
        }

        //ToDo: REMOVE - This allocates a new array of size 1 on every request.
        //string computerAction = Random.Shared.GetItems(_actions, 1)[0];

        // Zero-allocation random selection
        string computerAction = _actions[Random.Shared.Next(_actions.Length)];

        // Zero-allocation result generation using exact string matches
        if (string.Equals(computerAction, playerAction, StringComparison.OrdinalIgnoreCase))
        {
            return $"I play {computerAction}. It's a draw!";
        }

        bool playerWins = (computerAction == "rock" && playerAction.Equals("paper", StringComparison.OrdinalIgnoreCase)) ||
                          (computerAction == "paper" && playerAction.Equals("scissors", StringComparison.OrdinalIgnoreCase)) ||
                          (computerAction == "scissors" && playerAction.Equals("rock", StringComparison.OrdinalIgnoreCase));

        return playerWins
            ? $"I play {computerAction}. Winner!"
            : $"I play {computerAction}. Loser!";

    }

    private static bool IsValidAction(string action)
    {
        foreach (var item in _actions)
        {
            if (string.Equals(item, action, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}

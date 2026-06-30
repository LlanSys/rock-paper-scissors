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
        if (request?.Actions == null || request.Actions.Length == 0)
        {
            return [];
        }

        // Pre-size the list capacity to prevent internal array reallocations as items are added
        string[] results = new string[request.Actions.Length];

        for (int i = 0; i < request.Actions.Length; i++)
        {
            string? rawPlayer = request.Actions[i];
            if (string.IsNullOrEmpty(rawPlayer))
            {
                results[i] = "You have to play rock, paper or scissors";
                continue;
            }

            // Case-insensitive verification without allocating new strings or LINQ enumerators
            string? matchedAction = FindValidAction(rawPlayer);

            if (matchedAction == null)
            {
                results[i] = "You have to play rock, paper or scissors";
                continue;
            }

            // Zero-allocation random single selection
            string computerAction = _actions[Random.Shared.Next(_actions.Length)];

            // Evaluation logic using the strictly matched internal static strings
            if (computerAction == matchedAction)
            {
                results[i] = $"I play {computerAction}. It's a draw!";
                continue;
            }

            bool playerWins = (computerAction == "rock" && matchedAction == "paper") ||
                              (computerAction == "paper" && matchedAction == "scissors") ||
                              (computerAction == "scissors" && matchedAction == "rock");

            results[i] = playerWins
                ? $"I play {computerAction}. Winner!"
                : $"I play {computerAction}. Loser!";
        }

        return results;
    }

    private static string? FindValidAction(string input)
    {
        foreach (var action in _actions)
        {
            if (string.Equals(action, input, StringComparison.OrdinalIgnoreCase))
            {
                return action; // Returns reference to our static string instance
            }
        }
        return null;
    }
}

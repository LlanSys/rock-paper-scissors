using Rps.Api.Http.Domain.Enums.Common;

namespace Rps.Api.Http.Domain.Models;

public sealed record GameResult(GameOutcome Outcome, string Message);

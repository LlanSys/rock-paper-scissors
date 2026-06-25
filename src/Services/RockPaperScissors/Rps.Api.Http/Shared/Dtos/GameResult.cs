using Rps.Api.Http.Domain.Enums.Common;

namespace Rps.Api.Http.Shared.Dtos;

public sealed record GameResult(GameOutcome Outcome, string Message);

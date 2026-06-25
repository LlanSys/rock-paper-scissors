using System.Collections.Generic;

namespace Rps.Api.Http.Features.PlayMultiple.v2.Dtos;

public sealed record PlayMultipleRequest(IReadOnlyList<string> Actions);

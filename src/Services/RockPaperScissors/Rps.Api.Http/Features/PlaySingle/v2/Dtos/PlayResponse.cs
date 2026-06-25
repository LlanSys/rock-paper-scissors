namespace Rps.Api.Http.Features.PlaySingle.v2.Dtos;

public sealed record PlayResponse(
    string PlayerMove,
    string ComputerMove,
    string Outcome,
    string Message
);

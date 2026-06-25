namespace Rps.Api.Http.Shared.Dtos;

public sealed record PlayResponse(
    string PlayerMove,
    string ComputerMove,
    string Outcome,
    string Message
);

using System;
using System.Threading.Tasks;
using Rps.Api.Http.Domain.Enums.v2;
using Rps.Api.Http.Features.PlaySingle.v2.Contracts.Interfaces;

namespace Rps.Api.Http.Features.PlaySingle.v2.Contracts.Implementations;

public class ComputerMoveSelector : IComputerMoveSelector
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    public async Task<Move> SelectMoveAsync()
    {
        await Task.CompletedTask;
        return Moves[Random.Shared.Next(Moves.Length)];
    }
}

using System;
using System.Threading.Tasks;

using Rps.Api.Http.Domain.Contracts.Interfaces;
using Rps.Api.Http.Domain.Enums.v2;

namespace Rps.Api.Http.Domain.Contracts.Implementations;

public class ComputerMoveSelector : IComputerMoveSelector
{
    private static readonly Move[] Moves = Enum.GetValues<Move>();

    public ValueTask<Move> SelectMoveAsync()
    {
        var move = Moves[Random.Shared.Next(Moves.Length)];
        return ValueTask.FromResult(move);
    }
}

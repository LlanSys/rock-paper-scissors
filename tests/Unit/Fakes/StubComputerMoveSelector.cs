using Rps.Api.Http.Domain.Contracts.Interfaces;
using Rps.Api.Http.Domain.Enums.v2;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rps.Api.UnitTests.Fakes;

/// <summary>
/// A controllable test double for <see cref="IComputerMoveSelector"/>.
/// Set <see cref="NextMove"/> for a fixed move, or enqueue specific moves
/// via <see cref="MoveSequence"/> for multi-round scenarios.
/// </summary>
public class StubComputerMoveSelector : IComputerMoveSelector
{
    public Move NextMove { get; set; } = Move.Fire;
    public Queue<Move> MoveSequence { get; } = new();

    public ValueTask<Move> SelectMoveAsync()
    {
        if (MoveSequence.Count > 0)
        {
            return ValueTask.FromResult(MoveSequence.Dequeue());
        }

        return ValueTask.FromResult(NextMove);
    }
}

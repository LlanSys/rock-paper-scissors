using System.Threading.Tasks;
using Rps.Api.Http.Domain.Enums.v2;

namespace Rps.Api.Http.Features.PlaySingle.v2.Contracts.Interfaces;

public interface IComputerMoveSelector
{
    Task<Move> SelectMoveAsync();
}

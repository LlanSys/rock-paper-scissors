using System.ComponentModel;

namespace Rps.Api.Http.Domain.Enums.Common;

public enum GameOutcome
{
    [Description("Draw")]
    Draw,

    [Description("Winner")]
    Winner,

    [Description("Loser")]
    Loser
}

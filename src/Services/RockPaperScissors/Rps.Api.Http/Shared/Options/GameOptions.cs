namespace Rps.Api.Http.Shared.Options;

public class GameOptions
{
    public const string SectionName = "GameOptions";

    public string WinnerTemplate { get; set; } = "Winner!";
    public string LoserTemplate { get; set; } = "Loser!";
    public string DrawTemplate { get; set; } = "It's a draw!";
}

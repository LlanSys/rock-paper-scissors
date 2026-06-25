namespace Rps.Api.Http.Shared.Options;

public class GameOptions
{
    public const string SectionName = "GameOptions";

    public string WinnerTemplate { get; set; } = "I play {ComputerMove}. Winner! {PlayerMove} {Verb} {ComputerMove}.";
    public string LoserTemplate { get; set; } = "I play {ComputerMove}. Loser! {ComputerMove} {Verb} {PlayerMove}.";
    public string DrawTemplate { get; set; } = "I play {ComputerMove}. It's a draw!";
}

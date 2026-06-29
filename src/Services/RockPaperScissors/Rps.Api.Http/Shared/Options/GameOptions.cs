using System.ComponentModel.DataAnnotations;

namespace Rps.Api.Http.Shared.Options;

public sealed class GameOptions
{
    public const string SectionName = "GameOptions";

    [Required]
    public string WinnerTemplate { get; init; } = "I play {ComputerMove}. Winner! {PlayerMove} {Verb} {ComputerMove}.";

    [Required]
    public string LoserTemplate { get; init; } = "I play {ComputerMove}. Loser! {ComputerMove} {Verb} {PlayerMove}.";

    [Required]
    public string DrawTemplate { get; init; } = "I play {ComputerMove}. It's a draw!";
}

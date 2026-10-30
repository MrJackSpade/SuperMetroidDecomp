using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The game-completion facts the ending credits present (#633).</summary>
internal sealed record EndingCreditsSetup
{
    /// <summary>Clear-time hours: the game clock's hours (at most 99) or the configured override's whole hours.</summary>
    public required int GameTimeHours { get; init; }

    /// <summary>Clear-time minutes, 0 through 59.</summary>
    public required int GameTimeMinutes { get; init; }

    /// <summary>Inventory.</summary>
    public EndingInventorySnapshot Inventory { get; init; } = default;

    /// <summary>Japanese text.</summary>
    public bool JapaneseText { get; init; } = false;

    /// <summary>Ending text.</summary>
    public EndingTypewriterText? EndingText { get; init; } = null;

    /// <summary>Critters escaped.</summary>
    public bool CrittersEscaped { get; init; } = false;
}

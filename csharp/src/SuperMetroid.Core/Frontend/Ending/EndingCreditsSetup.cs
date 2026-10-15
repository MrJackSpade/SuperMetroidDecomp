using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The game-completion facts the ending credits present (#633).</summary>
internal sealed record EndingCreditsSetup
{
    /// <summary>Game time hours.</summary>
    public required ushort GameTimeHours { get; init; }

    /// <summary>Game time minutes.</summary>
    public required ushort GameTimeMinutes { get; init; }

    /// <summary>Inventory.</summary>
    public EndingInventorySnapshot Inventory { get; init; } = default;

    /// <summary>Japanese text.</summary>
    public bool JapaneseText { get; init; } = false;

    /// <summary>Ending text.</summary>
    public EndingTextPresentation? EndingText { get; init; } = null;

    /// <summary>Critters escaped.</summary>
    public bool CrittersEscaped { get; init; } = false;
}

using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The game-completion facts the ending credits present (#633).</summary>
internal sealed record EndingCreditsSetup
{
    /// <summary>Clear-time hours: the game clock's hours (at most 99) or the configured override's whole hours.</summary>
    public required int GameTimeHours { get; init; }

    /// <summary>Clear-time minutes, 0 through 59.</summary>
    public required int GameTimeMinutes { get; init; }

    /// <summary>The items and capacities Samus collected, from which the ending reckons her item percentage.</summary>
    public EndingInventorySnapshot Inventory { get; init; } = default;

    /// <summary>Whether the game shows its Japanese text, which the credits' closing text follows.</summary>
    public bool JapaneseText { get; init; } = false;

    /// <summary>The installed text the ending types, or null when none is installed.</summary>
    public EndingTypewriterText? EndingText { get; init; } = null;

    /// <summary>Whether Samus freed the animals, whose ship then escapes Zebes beside the gunship.</summary>
    public bool CrittersEscaped { get; init; } = false;
}

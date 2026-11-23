using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The game-completion facts the ending credits present (#633).</summary>
internal sealed partial record EndingCreditsSetup
{
    /// <summary>
    /// Clear-time hours: the game clock's hours or the configured override's whole hours, 0 through the longest play time's
    /// hours, so each of its two digits is 0 through 9.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The hours are a play time the clock never reaches.</exception>
    public required int GameTimeHours
    {
        get;
        init => field = Ensure.BetweenInclusive(0, GameTimeState.LongestPlayMinutes / GameClockReading.UnitsPerNextUnit, value);
    }

    /// <summary>Clear-time minutes, 0 through 59, so each of its two digits is 0 through 9.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The minutes lie outside an hour.</exception>
    public required int GameTimeMinutes
    {
        get;
        init => field = Ensure.BetweenInclusive(0, GameClockReading.UnitsPerNextUnit - 1, value);
    }

    /// <summary>The items and capacities Samus collected, from which the ending reckons her item percentage.</summary>
    public required EndingInventorySnapshot Inventory { get; init; }

    /// <summary>Whether the game shows its Japanese text, which the credits' closing text follows.</summary>
    public required TextLanguage Language { get; init; }

    /// <summary>Whether Samus freed the animals, whose ship then escapes Zebes beside the gunship.</summary>
    public required bool CrittersEscaped { get; init; }
}

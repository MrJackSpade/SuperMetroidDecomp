namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The counter cadences and phases of Kraid's fight (#1606): his rising rocks, his spit rocks and his flashes.</summary>
internal static class KraidCadenceDefinitions
{
    /// <summary>
    /// The phase of Kraid's hurt count on which he shows his hurt colours: set on its odd counts, so the hurt colours
    /// and his health band's colours alternate every update while it counts down.
    /// </summary>
    internal static readonly UpdateCounterField HurtColoursShown = new(0, 1);

    /// <summary>
    /// The updates on which Kraid's fully open mouth spits a rock: every sixteenth value of his head's countdown, on the
    /// period's first place, including the update the open mouth reloads it.
    /// </summary>
    internal static readonly UpdateCadence SpitRockEverySixteenthMouthUpdate = new(16, 0);

    /// <summary>
    /// The phase of a charging lint's countdown on which it shows Kraid's colours: set on its odd counts, so it
    /// alternates with the white hurt colours every update.
    /// </summary>
    internal static readonly UpdateCounterField LintChargeShowsOwnColours = new(0, 1);

    /// <summary>
    /// The updates on which Kraid, grown and breaking the ceiling's platforms, throws up a rising rock: every eighth
    /// update of his body's own update count, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence CeilingBreakRockEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The updates on which Kraid's first rise through the floor throws up a rising rock: every sixteenth value of his
    /// rising-rock countdown, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence FirstRiseRockEverySixteenthUpdate = new(16, 0);

    /// <summary>
    /// The updates on which Kraid's faster rise throws up a rising rock: every eighth value of his rising-rock countdown,
    /// on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence FasterRiseRockEveryEighthUpdate = new(8, 0);

    /// <summary>
    /// The first of the two updates in every eight of the room's earthquake on which Kraid's raised body throws up a
    /// rising rock: the earthquake's remaining count at place two of its period.
    /// </summary>
    internal static readonly UpdateCadence RaisedBodyFirstRockOfEight = new(8, 2);

    /// <summary>
    /// The second of the two updates in every eight of the room's earthquake on which Kraid's raised body throws up a
    /// rising rock: the earthquake's remaining count at place zero of its period, two updates after the first.
    /// </summary>
    internal static readonly UpdateCadence RaisedBodySecondRockOfEight = new(8, 0);
}

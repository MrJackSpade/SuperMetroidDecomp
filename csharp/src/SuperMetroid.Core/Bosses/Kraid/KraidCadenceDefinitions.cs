namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The counter cadences and phases of Kraid's fight (#1606, #1619): his rising rocks, his spit rocks, his flashes and the
/// sway and ceiling rocks his height paces as he rises.
/// </summary>
internal static class KraidCadenceDefinitions
{
    /// <summary>
    /// The phase of Kraid's height that sets which way his rising body sways (#1619): set for two pixels of height, then
    /// clear for two, so rising a pixel an update he sways one way for two updates and back for two. Breaking the ceiling's
    /// platforms he sways left while it is set, and raising his body through the floor he sways right.
    /// </summary>
    internal static readonly UpdateCounterField RiseSwayPhase = new(1, 1);

    /// <summary>
    /// The heights at which Kraid, grown and breaking the ceiling's platforms, drops the next ceiling rock (#1619): every
    /// fourth pixel of the height he rises to, one a pixel an update, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence CeilingRockEveryFourthRow = new(4, 0);

    /// <summary>
    /// The heights at which Kraid's sinking body reaches a row of his sinking schedule (#1619): every eighth pixel of the
    /// height he sinks through, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence SinkRowEveryEighthPixel = new(8, 0);

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

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The counter cadences and phases of Kraid's fight (#1606, #1619): his rising rocks, his spit rocks, his lints' charge
/// flash and the sway and ceiling rocks his height paces as he rises.
/// </summary>
internal static class KraidCadenceDefinitions
{
    /// <summary>
    /// The phase of Kraid's height that sets which way his rising body sways (#1619): for two pixels of height, then
    /// not for two, so rising a pixel an update he sways one way for two updates and back for two. Breaking the ceiling's
    /// platforms he sways left in the alternation's second half, and raising his body through the floor he sways right.
    /// </summary>
    internal static readonly UpdateAlternation RiseSwayPhase = new(2);

    /// <summary>
    /// The heights at which Kraid, grown and breaking the ceiling's platforms, drops the next ceiling rock (#1619): every
    /// fourth pixel of the height he rises to, one a pixel an update, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence CeilingRockRow = new(4, 0);

    /// <summary>
    /// The updates on which Kraid's fully open mouth spits a rock, counted on the updates left of his head's hold on the
    /// open mouth at the period's first place, including the update the open mouth shows and starts it.
    /// </summary>
    internal static readonly UpdateCadence MouthSpitRock = new(16, 0);

    /// <summary>
    /// The phase of a charging lint's updates left on which it shows Kraid's colours: on its odd updates left, so it
    /// alternates with the white hurt colours every update.
    /// </summary>
    internal static readonly UpdateAlternation LintChargeShowsOwnColours = new(1);

    /// <summary>
    /// The updates on which Kraid, grown and breaking the ceiling's platforms, throws up a rising rock, counted on his
    /// body's own update count at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence CeilingBreakRock = new(8, 0);

    /// <summary>
    /// The updates on which Kraid's first rise through the floor throws up a rising rock, counted on the updates left of
    /// the rise's slow rocks at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence FirstRiseRock = new(16, 0);

    /// <summary>
    /// The updates on which Kraid's faster rise throws up a rising rock, counted on the updates left of the rise's fast
    /// rocks at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence FasterRiseRock = new(8, 0);

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

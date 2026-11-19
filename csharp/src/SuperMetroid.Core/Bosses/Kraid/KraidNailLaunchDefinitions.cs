namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The launch speeds of Kraid's paired fingernails: every nail flies off one pixel an update leftward, and up or down
/// against its sibling's vertical travel. The random number its launch draws changes neither.
/// </summary>
public static class KraidNailLaunchDefinitions
{
    /// <summary>The launch's horizontal speed: one pixel per frame leftward.</summary>
    public static SignedSixteenSixteen Leftward { get; } = SignedSixteenSixteen.FromWholePixels(-1);

    /// <summary>The launch's vertical speed when the sibling moves upward: one pixel per frame downward.</summary>
    public static SignedSixteenSixteen Downward { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The launch's vertical speed when the sibling moves downward or holds: one pixel per frame upward.</summary>
    public static SignedSixteenSixteen Upward { get; } = SignedSixteenSixteen.FromWholePixels(-1);
}

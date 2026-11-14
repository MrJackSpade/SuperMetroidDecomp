namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The launch speeds of Kraid's paired fingernails.</summary>
/// <remarks>
/// The two nail launch tables select four launches each. All four RNG choices within each table are identical:
/// X=-1, fractions zero, and Y opposite the sibling's sign. RNG remains relevant to spawn-mode selection.
/// </remarks>
public static class KraidNailLaunchDefinitions
{
    /// <summary>The launch's horizontal speed: one pixel per frame leftward.</summary>
    public static SignedSixteenSixteen Leftward { get; } = SignedSixteenSixteen.FromWholePixels(-1);

    /// <summary>The launch's vertical speed when the sibling moves upward: one pixel per frame downward.</summary>
    public static SignedSixteenSixteen Downward { get; } = SignedSixteenSixteen.FromWholePixels(1);

    /// <summary>The launch's vertical speed when the sibling moves downward or holds: one pixel per frame upward.</summary>
    public static SignedSixteenSixteen Upward { get; } = SignedSixteenSixteen.FromWholePixels(-1);
}

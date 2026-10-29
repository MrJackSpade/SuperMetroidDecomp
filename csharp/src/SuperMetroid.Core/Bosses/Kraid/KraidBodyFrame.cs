namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The body frame Kraid's body layer shows (#1352, #1466): how each half of his body last appeared there, and the head
/// pose drawn over the upper half since it was last shown.
/// </summary>
/// <param name="Upper">How the upper half shows.</param>
/// <param name="Head">The head pose drawn over the upper half's first rows, or null when the upper half has been shown since.</param>
/// <param name="Lower">How the lower half shows.</param>
internal sealed record KraidBodyFrame(KraidBodyAppearance Upper, KraidHeadFrame? Head, KraidBodyAppearance Lower)
{
    /// <summary>The frame before Kraid shows anything: both halves hidden.</summary>
    public static KraidBodyFrame Hidden { get; } = new(KraidBodyAppearance.Hidden, null, KraidBodyAppearance.Hidden);
}

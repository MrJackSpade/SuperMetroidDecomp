namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The body frame Kraid shows on BG2 (#1352): what each visible page last received, and the head pose drawn over the
/// top page since its last upload.
/// </summary>
/// <param name="Top">The upper page.</param>
/// <param name="Head">The head pose drawn over the upper page's first rows, or null when a later page upload covered it.</param>
/// <param name="Bottom">The lower page.</param>
internal sealed record KraidBodyFrame(KraidBodyPage Top, KraidHeadFrame? Head, KraidBodyPage Bottom)
{
    /// <summary>The frame before Kraid uploads anything: both pages blank.</summary>
    public static KraidBodyFrame Blank { get; } = new(KraidBodyPage.Blank, null, KraidBodyPage.Blank);
}

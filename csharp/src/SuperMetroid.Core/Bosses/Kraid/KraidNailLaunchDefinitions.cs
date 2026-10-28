namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled fixed-point launch words for Kraid's paired fingernails.</summary>
/// <remarks>
/// $A7:BE3E/BE46 indirect tables select records BE4E..BE8D. All four RNG choices within each table are identical:
/// X=-1, fractions zero, and Y opposite the sibling's sign. RNG remains relevant to spawn-mode selection.
/// </remarks>
public static class KraidNailLaunchDefinitions
{
    /// <summary>The launch's whole X word: one pixel per frame leftward.</summary>
    public const ushort LeftwardWhole = ushort.MaxValue;

    /// <summary>The launch's whole Y word when the sibling moves upward: one pixel per frame downward.</summary>
    public const ushort DownwardWhole = 1;

    /// <summary>The launch's whole Y word when the sibling moves downward or holds: one pixel per frame upward.</summary>
    public const ushort UpwardWhole = ushort.MaxValue;
}

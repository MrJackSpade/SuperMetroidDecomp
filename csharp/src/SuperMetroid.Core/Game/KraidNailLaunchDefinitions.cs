namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed-point launch words for Kraid's paired fingernails.</summary>
public static class KraidNailLaunchDefinitions
{
    /// <summary>
    /// $A7:BE3E/BE46 indirect tables select records BE4E..BE8D. All four RNG
    /// choices within each table are identical: X=-1, fractions zero, and Y
    /// opposite the sibling's sign. RNG remains relevant to spawn-mode selection.
    /// </summary>
    public static (ushort XFraction, ushort XWhole, ushort YFraction, ushort YWhole)
        FromSiblingVelocity(ushort siblingYWhole) =>
        (0, ushort.MaxValue, 0, unchecked((short)siblingYWhole) < 0 ? (ushort)1 : ushort.MaxValue);
}

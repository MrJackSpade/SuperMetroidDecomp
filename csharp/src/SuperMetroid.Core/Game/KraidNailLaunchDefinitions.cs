namespace SuperMetroid.Core.Game;

/// <summary>Compiled fixed-point launch words for Kraid's paired fingernails.</summary>
public static class KraidNailLaunchDefinitions
{
    /// <summary>
    /// $A7:BE3E/BE46 indirect tables select records BE4E..BE8D. All four RNG
    /// choices within each table are identical: X=-1, fractions zero, and Y
    /// opposite the sibling's sign. RNG remains relevant to spawn-mode selection.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the native $A7:BD60-BD98 selector
    /// and all eight records in the pinned NTSC J/U v1.0 source. CMP #0 / BPL
    /// tests the signed whole word: zero belongs to the nonnegative branch.
    /// All 65,536 input words are supported. The three constant fields and the
    /// sign-selected Y whole word have separate original-record proofs; no
    /// random choice changes these words or requires a stored lookup.
    /// </remarks>
    public static (ushort XFraction, ushort XWhole, ushort YFraction, ushort YWhole)
        FromSiblingVelocity(ushort siblingYWhole) =>
        (0, ushort.MaxValue, 0, unchecked((short)siblingYWhole) < 0 ? (ushort)1 : ushort.MaxValue);
}

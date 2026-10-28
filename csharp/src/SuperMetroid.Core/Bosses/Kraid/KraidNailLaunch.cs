namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Kraid's second fingernail opposite its sibling's vertical travel.</summary>
public static class KraidNailLaunch
{
    /// <summary>The launch velocity for a sibling moving at whole Y word <paramref name="siblingYWhole"/>.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the native $A7:BD60-BD98 selector
    /// and all eight records in the pinned NTSC J/U v1.0 source. CMP #0 / BPL
    /// tests the signed whole word: zero belongs to the nonnegative branch.
    /// All 65,536 input words are supported. The three constant fields and the
    /// sign-selected Y whole word have separate original-record proofs; no
    /// random choice changes these words or requires a stored lookup.
    /// </remarks>
    public static FixedVector<RoomPixels> FromSiblingVelocity(int siblingYWhole) =>
        FixedVector<RoomPixels>.FromWords(KraidNailLaunchDefinitions.LeftwardWhole, 0,
            unchecked((short)siblingYWhole) < 0 ? KraidNailLaunchDefinitions.DownwardWhole : KraidNailLaunchDefinitions.UpwardWhole, 0);
}

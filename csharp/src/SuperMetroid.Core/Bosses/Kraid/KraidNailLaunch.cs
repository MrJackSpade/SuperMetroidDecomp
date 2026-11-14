namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Kraid's second fingernail opposite its sibling's vertical travel.</summary>
public static class KraidNailLaunch
{
    /// <summary>The launch velocity for a sibling moving <paramref name="siblingYWhole"/> whole pixels a frame vertically.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the nail launch selector
    /// and all eight launches in the pinned Japanese and North American v1.0 source. The signed compare
    /// tests the sibling's signed whole-pixel vertical speed: zero belongs to the nonnegative branch.
    /// Every whole-pixel speed is supported. The three constant fields and the
    /// sign-selected vertical speed have separate game-data proofs; no
    /// random choice changes these speeds or requires a stored lookup.
    /// </remarks>
    public static FixedVector<RoomPixels> FromSiblingVelocity(int siblingYWhole) =>
        new(KraidNailLaunchDefinitions.Leftward,
            CoordinateAxis.AsSigned(siblingYWhole) < 0 ? KraidNailLaunchDefinitions.Downward : KraidNailLaunchDefinitions.Upward);
}

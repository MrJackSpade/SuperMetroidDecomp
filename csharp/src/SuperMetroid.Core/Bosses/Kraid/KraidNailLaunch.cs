namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Kraid's second fingernail opposite its sibling's vertical travel.</summary>
public static class KraidNailLaunch
{
    /// <summary>The launch velocity for a sibling moving <paramref name="siblingYWhole"/> whole pixels a frame vertically.</summary>
    /// <remarks>
    /// The launch reads the sibling's whole-pixel vertical speed as signed: a sibling moving up sends this nail down, and
    /// one moving down or holding still sends it up. Every whole-pixel speed is supported. All eight of the game's nail
    /// launches share these speeds; no random choice changes them.
    /// </remarks>
    public static FixedVector<RoomPixels> FromSiblingVelocity(int siblingYWhole) =>
        new(KraidNailLaunchDefinitions.Leftward,
            CoordinateAxis.AsSigned(siblingYWhole) < 0 ? KraidNailLaunchDefinitions.Downward : KraidNailLaunchDefinitions.Upward);
}

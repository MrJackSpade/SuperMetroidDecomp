namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Launches Kraid's second fingernail opposite its sibling's vertical travel.</summary>
public static class KraidNailLaunch
{
    /// <summary>The launch velocity for a sibling moving <paramref name="siblingY"/> a frame vertically.</summary>
    /// <remarks>
    /// A sibling moving up sends this nail down, and one moving down or holding still sends it up. Every vertical speed
    /// is supported. All eight of the game's nail launches share these speeds; no random choice changes them.
    /// </remarks>
    public static FixedVector<RoomPixels> FromSiblingVelocity(SignedSixteenSixteen siblingY) =>
        new(KraidNailLaunchDefinitions.Leftward,
            siblingY.IsNegative ? KraidNailLaunchDefinitions.Downward : KraidNailLaunchDefinitions.Upward);
}

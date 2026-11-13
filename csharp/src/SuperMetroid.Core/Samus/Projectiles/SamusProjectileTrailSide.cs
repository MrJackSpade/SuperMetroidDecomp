namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One sparkle of a projectile trail, running its own program.</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>Room-pixel trail-sprite position.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
    /// <summary>Updates remaining before the current trail cue advances; zero once the sparkle's program has ended.</summary>
    public int FrameHold { get; internal set; }
    /// <summary>
    /// The side's place in its trail program: the step it runs when its timer next expires. Null before any program
    /// starts; such a side's timer is zero, so it never runs a step.
    /// </summary>
    public ProjectileTrailCursor? Cursor { get; internal set; }
    /// <summary>The pose this side last showed, which it draws; null before its program shows one, when it draws nothing.</summary>
    public ProjectileTrailFrame? ShownPose { get; internal set; }
    /// <summary>
    /// Room-pixel position the side shows this frame: <see cref="Position"/> when the side last advanced. The trail update
    /// draws each side right after advancing it, so a sibling's later move command shows only on the next frame.
    /// </summary>
    public Position16<RoomPixels> DisplayedPosition { get; internal set; }
}

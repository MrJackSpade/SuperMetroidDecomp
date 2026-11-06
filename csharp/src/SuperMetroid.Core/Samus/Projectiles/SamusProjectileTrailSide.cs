namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One side of a two-stream projectile-trail animation.</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>Room-pixel trail-sprite position.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
    /// <summary>Updates remaining before the current trail instruction advances.</summary>
    public int FrameHold { get; internal set; }
    /// <summary>
    /// The side's place in its trail program: the step it runs when its timer next expires. Null once cleared, before
    /// any program starts; a cleared side's timer is zero, so it never runs a step.
    /// </summary>
    public ProjectileTrailCursor? Cursor { get; internal set; }
    /// <summary>
    /// The pose this side last showed, which it draws; null once cleared. Allocation does not clear it, so a new trail
    /// shows the slot's previous pose until its own first one, as the game's retained tile word does.
    /// </summary>
    public ProjectileTrailFrame? ShownPose { get; internal set; }
    /// <summary>
    /// Room-pixel position the side shows this frame: <see cref="Position"/> when the side last advanced. The trail update
    /// draws each side right after advancing it, so a sibling's later move command shows only on the next frame.
    /// </summary>
    public Position16<RoomPixels> DisplayedPosition { get; internal set; }

    /// <summary>Resets the sprite position, timer, program cursor and shown pose for one trail side.</summary>
    internal void ClearFields()
    {
        Position = Position16<RoomPixels>.Zero;
        FrameHold = 0;
        Cursor = null;
        ShownPose = null;
        DisplayedPosition = Position16<RoomPixels>.Zero;
    }
}

using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One sparkle of a projectile trail, running its own animation (#2348).</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>Room-pixel trail-sprite position.</summary>
    public Position16<RoomPixels> Position { get; internal set; }
    /// <summary>Updates the shown frame has left before the sparkle's animation plays on; meaningful while it plays.</summary>
    public int FrameHold { get; internal set; }
    /// <summary>The sparkle's animation and where it stands in it; null once its animation has ended, when it draws nothing.</summary>
    internal AnimationPlayback<ProjectileTrailSegment>? Playback { get; set; }
    /// <summary>The frame this side last showed, which it draws; null before its animation shows one, when it draws nothing.</summary>
    public ProjectileTrailFrame? ShownPose { get; internal set; }
    /// <summary>
    /// Room-pixel position the side shows this frame: <see cref="Position"/> when the side last advanced. The trail update
    /// draws each side right after advancing it, so a sibling's later move shows only on the next frame.
    /// </summary>
    public Position16<RoomPixels> DisplayedPosition { get; internal set; }

    /// <summary>Whether the sparkle's animation has ended, or never started, so it shows nothing more.</summary>
    internal bool HasEnded => Playback is null;
}

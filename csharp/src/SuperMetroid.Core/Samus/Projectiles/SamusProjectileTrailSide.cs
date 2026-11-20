using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One sparkle of a projectile trail, running its own animation (#2348).</summary>
public sealed class SamusProjectileTrailSide
{
    /// <summary>
    /// Where the sparkle stands from its trail's <c>Origin</c>: its placement from the shot,
    /// four pixels up and left where its picture is drawn from, and each pixel its cues have since moved it down.
    /// </summary>
    public Offset16<RoomPixels> Offset { get; internal set; }
    /// <summary>
    /// The sparkle's animation (#2458): the frame it shows, the updates that frame has left and where its play stands; null
    /// before the sparkle starts, when it draws nothing.
    /// </summary>
    internal AnimationPlayer<ProjectileTrailRun, ProjectileTrailFrame, ProjectileTrailCue>? Animation { get; set; }
    /// <summary>The frame this side last showed, which it draws; null before its animation shows one, when it draws nothing.</summary>
    public ProjectileTrailFrame? ShownPose => Animation?.Shown;
    /// <summary>
    /// Where the side shows this frame from its trail's origin: <see cref="Offset"/> when the side last advanced. The trail
    /// update draws each side right after advancing it, so a sibling's later move shows only on the next frame.
    /// </summary>
    public Offset16<RoomPixels> DisplayedOffset { get; internal set; }

    /// <summary>Whether the sparkle's animation has ended, or never started, so it shows nothing more.</summary>
    internal bool HasEnded => Animation is not { } animation || animation.HasStopped;
}

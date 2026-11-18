using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Projectile animations the collision and drawing fixtures use (#1301, #1600, #2458).</summary>
internal static class ProjectileFixturePlaybacks
{
    /// <summary>Where <see cref="Active"/>'s animation stands: the start of the Power Beam's rightward animation.</summary>
    internal static AnimationPlayback<SamusProjectileSegment> ActivePlayback { get; } =
        new(SamusProjectileAnimations.StartingWith(SamusProjectileSegment.PowerRight), AnimationPlace.StartOfAnimation);

    /// <summary>
    /// A live shot's animation for fixtures that only need the slot active (its native pointer nonzero): they check
    /// collision, damage or drawing and never play the shot's animation. It shows nothing until its
    /// <paramref name="updates"/>th update.
    /// </summary>
    internal static AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> Active(int updates = 1) =>
        StartingAfter(SamusProjectileSegment.PowerRight, updates);

    /// <summary>
    /// The animation starting at <paramref name="segment"/>, showing nothing until its <paramref name="updates"/>th update
    /// plays it to its first frame.
    /// </summary>
    internal static AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue> StartingAfter(
        SamusProjectileSegment segment, int updates) =>
        AnimationPlayer<SamusProjectileSegment, SamusProjectileFrameId, NoAnimationCue>.StartingAfter(
            SamusProjectileAnimations.Segments, SamusProjectileAnimations.StartingWith(segment), updates);
}

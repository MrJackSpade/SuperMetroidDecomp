using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using D = SuperMetroid.Core.Enemies.OldTourianWall.OldTourianEscapeShaftWallExplosionAnimationDefinitions;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Rooms.RoomObjects.OldTourianWallExplosionCue>.Unpaused;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape-shaft fake-wall explosion's typed animation.</summary>
/// <param name="owner">The family's cue owner, which carries out the events the animation raises (#2571).</param>
/// <param name="segment">Where the animation starts.</param>
internal sealed class OldTourianWallExplosionAnimation(IEnemyProjectileCues<OldTourianWallExplosionCue> owner, OldTourianWallExplosionSegment segment)
    : EnemyProjectileTypedAnimation<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue>(AllAnimations[segment], EnemyProjectileCueRunner<OldTourianWallExplosionSegment, OldTourianWallExplosionCue>.By(owner))
{
    /// <summary>
    /// The segments' beats: the setup that stops the movement, the burst that scatters, plays its sound and shows the six
    /// pictures of a small explosion, and the deletion after the burst's last picture (#2276).
    /// </summary>
    private static UnpausedAnimationSegments<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue> AllSegments { get; } = new(
        new Dictionary<OldTourianWallExplosionSegment, Beat[]>
        {
            [OldTourianWallExplosionSegment.Start] =
            [
                new Beat.Event(OldTourianWallExplosionCue.StopMoving),
            ],
            [OldTourianWallExplosionSegment.Burst] =
            [
                new Beat.Event(OldTourianWallExplosionCue.Scatter),
                new Beat.Event(OldTourianWallExplosionCue.ExplosionSound),
                .. D.Beats.Select(beat => new Beat.Frame(beat.Frame, beat.Duration)),
            ],
            [OldTourianWallExplosionSegment.Deletion] =
            [
                new Beat.Event(OldTourianWallExplosionCue.Delete),
            ],
        });

    /// <summary>
    /// The explosion's animation: the setup, the burst once, then the deletion, holding after it; each phase plays its
    /// segment whole.
    /// </summary>
    internal static IReadOnlyDictionary<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>> AllAnimations { get; } =
        new Dictionary<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>>
        {
            [OldTourianWallExplosionSegment.Start] = new(
            [
                AllSegments.Whole(OldTourianWallExplosionSegment.Start),
                AllSegments.Whole(OldTourianWallExplosionSegment.Burst),
                AllSegments.Whole(OldTourianWallExplosionSegment.Deletion),
            ], AnimationEnd.Holds),
        };

    /// <inheritdoc/>
    private protected override UnpausedAnimationSegments<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue> Segments => AllSegments;

    /// <inheritdoc/>
    private protected override IEnumerable<KeyValuePair<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>>> StartingAt => AllAnimations;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}

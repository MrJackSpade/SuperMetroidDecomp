using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Animation;
using D = SuperMetroid.Core.Enemies.OldTourianWall.OldTourianEscapeShaftWallExplosionAnimationDefinitions;
using Beat = SuperMetroid.Core.Animation.AnimationBeat<SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFrameId, SuperMetroid.Core.Rooms.RoomObjects.OldTourianWallExplosionCue>;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape-shaft fake-wall explosion's typed animation.</summary>
internal sealed class OldTourianWallExplosionAnimation(OldTourianWallExplosionSegment segment)
    : EnemyProjectileTypedAnimation<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue>(AllAnimations[segment])
{
    /// <summary>
    /// The segments' beats: the setup that stops the movement, the burst that scatters, sounds and plays its six poses
    /// small explosions, and the deletion after its last play (#2276).
    /// </summary>
    internal static AnimationSegments<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue> AllSegments { get; } = new(
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
    private protected override AnimationSegments<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue> Segments => AllSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>> StartingAt => AllAnimations;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}

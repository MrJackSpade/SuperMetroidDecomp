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
    /// Both segments' beats: the setup that stops the movement, then the burst that scatters, sounds, plays its six poses
    /// small explosions and, after its last play, deletes the explosion.
    /// </summary>
    internal static AnimationSegments<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue> AllSegments { get; } = new(
        new Dictionary<OldTourianWallExplosionSegment, Beat[]>
        {
            [OldTourianWallExplosionSegment.Start] =
            [
                new Beat.Event(OldTourianWallExplosionCue.ClearFrameBehavior),
            ],
            [OldTourianWallExplosionSegment.Burst] =
            [
                new Beat.Event(OldTourianWallExplosionCue.Scatter),
                new Beat.Event(OldTourianWallExplosionCue.ExplosionSound),
                .. Enumerable.Range(0, D.Frames.Count).Select(place => new Beat.Frame(D.Frames[place], D.FrameDurations[place])),
                new Beat.Event(OldTourianWallExplosionCue.Delete),
            ],
        });

    /// <summary>
    /// The explosion's animation: the setup, the burst counted <see cref="OldTourianEscapeShaftWallExplosionAnimationDefinitions.BurstCount"/> times on the projectile's
    /// timer (loaded even for one play, as the game's animation loads its count before the burst), then the delete, holding after it.
    /// </summary>
    internal static IReadOnlyDictionary<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>> AllAnimations { get; } =
        new Dictionary<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>>
        {
            [OldTourianWallExplosionSegment.Start] = new(
            [
                new(OldTourianWallExplosionSegment.Start, 0, 1, 1),
                new(OldTourianWallExplosionSegment.Burst, 0, 2 + D.FrameDurations.Length, D.BurstCount, AnimationPhaseCounting.EveryPlay),
                new(OldTourianWallExplosionSegment.Burst, 2 + D.FrameDurations.Length, 1, 1),
            ], AnimationEnd.Holds),
        };

    /// <inheritdoc/>
    private protected override AnimationSegments<OldTourianWallExplosionSegment, EnemyProjectileFrameId, OldTourianWallExplosionCue> Segments => AllSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<OldTourianWallExplosionSegment, Animation<OldTourianWallExplosionSegment>> StartingAt => AllAnimations;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(EnemyProjectileFrameId frame) => frame;
}

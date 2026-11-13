using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using D = SuperMetroid.Core.Enemies.OldTourianWall.OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions;
using Beat = SuperMetroid.Core.Programs.AnimationBeat<int, SuperMetroid.Core.Rooms.RoomObjects.OldTourianWallExplosionCue>;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape-shaft fake-wall explosion's typed animation, in place of its list.</summary>
internal sealed class OldTourianWallExplosionAnimation(OldTourianWallExplosionProgram program)
    : EnemyProjectileTypedAnimation<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>(AllAnimations[program])
{
    /// <summary>
    /// Both segments' beats: the setup that stops the movement, then the burst that scatters, sounds, plays its six poses
    /// and, after its last play, deletes the explosion.
    /// </summary>
    internal static AnimationSegments<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue> AllSegments { get; } = new(
        new Dictionary<OldTourianWallExplosionProgram, Beat[]>
        {
            [OldTourianWallExplosionProgram.Start] =
            [
                new Beat.Event(OldTourianWallExplosionCue.ClearFrameBehavior),
            ],
            [OldTourianWallExplosionProgram.Burst] =
            [
                new Beat.Event(OldTourianWallExplosionCue.Scatter),
                new Beat.Event(OldTourianWallExplosionCue.ExplosionSound),
                .. Enumerable.Range(0, D.FrameDurations.Length).Select(frame => new Beat.Frame(frame, D.FrameDurations[frame])),
                new Beat.Event(OldTourianWallExplosionCue.Delete),
            ],
        });

    /// <summary>
    /// The explosion's animation: the setup, the burst counted <see cref="OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions.BurstCount"/> times on the projectile's
    /// timer (loaded even for one play, as the list loads its count before the burst), then the delete, holding after it.
    /// </summary>
    internal static IReadOnlyDictionary<OldTourianWallExplosionProgram, Animation<OldTourianWallExplosionProgram>> AllAnimations { get; } =
        new Dictionary<OldTourianWallExplosionProgram, Animation<OldTourianWallExplosionProgram>>
        {
            [OldTourianWallExplosionProgram.Start] = new(
            [
                new(OldTourianWallExplosionProgram.Start, 0, 1, 1),
                new(OldTourianWallExplosionProgram.Burst, 0, 2 + D.FrameDurations.Length, D.BurstCount, AnimationPhaseCounting.EveryPlay),
                new(OldTourianWallExplosionProgram.Burst, 2 + D.FrameDurations.Length, 1, 1),
            ], AnimationEnd.Holds),
        };

    /// <inheritdoc/>
    private protected override AnimationSegments<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue> Segments => AllSegments;

    /// <inheritdoc/>
    private protected override IReadOnlyDictionary<OldTourianWallExplosionProgram, Animation<OldTourianWallExplosionProgram>> StartingAt => AllAnimations;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(int frame) =>
        ShownProjectileFrames[frame];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<int, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<int, EnemyProjectileFrameId>
        {
            [0] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFirst,
            [1] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionSecond,
            [2] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionThird,
            [3] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFourth,
            [4] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFifth,
            [5] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionSixth,
        };
}

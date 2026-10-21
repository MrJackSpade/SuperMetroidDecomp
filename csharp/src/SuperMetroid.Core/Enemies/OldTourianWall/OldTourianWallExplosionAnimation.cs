using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using D = SuperMetroid.Core.Rooms.Plms.OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.OldTourianWallExplosionProgram, int, SuperMetroid.Core.Rooms.Plms.OldTourianWallExplosionCue>;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies.OldTourianWall;

/// <summary>The old Tourian escape-shaft fake-wall explosion's typed program, in place of its bank-$86 list.</summary>
internal sealed class OldTourianWallExplosionAnimation(OldTourianWallExplosionProgram program)
    : EnemyProjectileProgramAnimation<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>(program)
{
    /// <summary>Both programs: the setup, then the counted burst.</summary>
    internal static AnimationProgramSet<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue> AllPrograms { get; } = new(
        new Dictionary<OldTourianWallExplosionProgram, AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>[]>
        {
            [OldTourianWallExplosionProgram.Start] =
            [
                Step.Publish(OldTourianWallExplosionCue.ClearPreInstruction),
                Step.SetCounter(D.BurstCount),
                Step.FallInto(OldTourianWallExplosionProgram.Burst),
            ],
            [OldTourianWallExplosionProgram.Burst] =
            [
                Step.Publish(OldTourianWallExplosionCue.Scatter),
                Step.Publish(OldTourianWallExplosionCue.ExplosionSound),
                .. Enumerable.Range(0, D.FrameDurations.Length).Select(frame => Step.Show(frame, D.FrameDurations[frame])),
                Step.RepeatWhileCounting(OldTourianWallExplosionProgram.Burst),
                Step.Publish(OldTourianWallExplosionCue.Delete),
                Step.Hold,
            ],
        });

    /// <inheritdoc/>
    private protected override AnimationProgramSet<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue> Programs => AllPrograms;

    /// <inheritdoc/>
    private protected override EnemyProjectileFrameId Present(int frame) =>
        ShownProjectileFrames[frame];

    /// <summary>The projectile frame each frame of the family shows.</summary>
    internal static IReadOnlyDictionary<int, EnemyProjectileFrameId> ShownProjectileFrames { get; } =
        new Dictionary<int, EnemyProjectileFrameId>
        {
            [0] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFrame00,
            [1] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFrame01,
            [2] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFrame02,
            [3] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFrame03,
            [4] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFrame04,
            [5] = EnemyProjectileFrameId.OldTourianEscapeWallExplosionFrame05,
        };
}

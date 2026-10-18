using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using D = SuperMetroid.Core.Rooms.Plms.OldTourianEscapeShaftWallExplosionInstructionProgramDefinitions;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Rooms.Plms.OldTourianWallExplosionProgram, int, SuperMetroid.Core.Rooms.Plms.OldTourianWallExplosionCue>;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The old Tourian escape-shaft fake-wall explosion's typed program, in place of its bank-$86 list.</summary>
internal sealed class OldTourianWallExplosionAnimation(OldTourianWallExplosionProgram program)
    : EnemyProjectileProgramAnimation<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>(program)
{
    /// <summary>Both programs: the setup, then the counted burst.</summary>
    internal static AnimationProgramSet<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue> AllPrograms { get; } = new(
        new Dictionary<OldTourianWallExplosionProgram, AnimationProgramStep<OldTourianWallExplosionProgram, int, OldTourianWallExplosionCue>[]>
        {
            // $86:B443
            [OldTourianWallExplosionProgram.Start] =
            [
                Step.Publish(OldTourianWallExplosionCue.ClearPreInstruction),
                Step.SetCounter(D.BurstCount),
                Step.FallInto(OldTourianWallExplosionProgram.Burst),
            ],
            // $86:B449
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

    /// <summary>The burst's poses.</summary>
    internal static int PoseCount => D.FrameDurations.Length;

    /// <inheritdoc/>
    private protected override EnemyProjectileFramePresentation Present(int frame) =>
        EnemyProjectileFramePresentation.Installed(PresentationKey(frame));

    /// <summary>The installed artwork key of pose <paramref name="frame"/>: its native visual operand.</summary>
    internal static ushort PresentationKey(int frame) => frame switch
        {
            0 => D.FirstFrameOperand,
            1 => D.SecondFrameOperand,
            2 => D.ThirdFrameOperand,
            3 => D.FourthFrameOperand,
            4 => D.FifthFrameOperand,
            5 => D.SixthFrameOperand,
            _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "The wall explosion has six poses."),
        };
}

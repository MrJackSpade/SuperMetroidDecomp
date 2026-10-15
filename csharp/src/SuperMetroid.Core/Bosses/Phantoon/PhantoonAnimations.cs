using SuperMetroid.Core.Programs;
using C = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartCue;
using F = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartFrameKind;
using P = SuperMetroid.Core.Bosses.Phantoon.PhantoonPartProgram;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed body, eye, tentacle and mouth programs (#641), in place of their bank-$A7 lists.</summary>
internal static class PhantoonAnimations
{
    /// <summary>The eyelid's dwell on each of its two moving poses ($A7:CC53/CC57/CC85/CC95).</summary>
    private const int EyelidFrameDuration = 10;
    /// <summary>The tentacles' dwell on each of their four sway poses ($A7:CCD7..CCE3).</summary>
    private const int TentacleFrameDuration = 8;
    /// <summary>The mouth's dwell on each of its two opening poses ($A7:CCEB/CCEF).</summary>
    private const int MouthFrameDuration = 5;

    /// <summary>Every body, eye, tentacle and mouth program.</summary>
    internal static AnimationProgramSet<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> Programs { get; } = new(
        new Dictionary<PhantoonPartProgram, AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>[]>
    {
        [P.InvulnerableBody] = Still(new(F.InvulnerableBody, 0)),
        [P.FullHitboxBody] = Still(new(F.FullHitboxBody, 0)),
        [P.EyeHitboxBody] = Still(new(F.EyeHitboxBody, 0)),
        [P.EyeOpen] =
        [
            Show(new(F.OpeningEye, 0), EyelidFrameDuration),
            Show(new(F.OpeningEye, 1), EyelidFrameDuration),
            Show(new(F.OpeningEye, 2), 1),
            Cue(C.PlayMaterializationSound),
            Cue(C.BeginEyeTracking),
            Hold,
        ],
        [P.EyeClosed] = Still(new(F.ClosedEye, 0)),
        [P.EyeCloseAndPickNewPattern] =
        [
            Show(new(F.OpeningEye, 2), 1),
            Show(new(F.OpeningEye, 1), EyelidFrameDuration),
            Cue(C.PickNewPattern),
            GoTo(P.EyeClosed),
        ],
        [P.EyeClose] =
        [
            Show(new(F.OpeningEye, 2), 1),
            Show(new(F.OpeningEye, 1), EyelidFrameDuration),
            GoTo(P.EyeClosed),
        ],
        [P.EyeballCentered] =
        [
            Show(new(F.CenteredEye, 0), 1),
            Cue(C.PlayMaterializationSound),
            Hold,
        ],
        [P.EyeLookingUp] = Still(new(F.Gaze, 0)),
        [P.EyeLookingUpRight] = Still(new(F.Gaze, 1)),
        [P.EyeLookingRight] = Still(new(F.Gaze, 2)),
        [P.EyeLookingDownRight] = Still(new(F.Gaze, 3)),
        [P.EyeLookingDown] = Still(new(F.Gaze, 4)),
        [P.EyeLookingDownLeft] = Still(new(F.Gaze, 5)),
        [P.EyeLookingLeft] = Still(new(F.Gaze, 6)),
        [P.EyeLookingUpLeft] = Still(new(F.Gaze, 7)),
        [P.Tentacles] =
        [
            Show(new(F.Tentacle, 0), TentacleFrameDuration),
            Show(new(F.Tentacle, 1), TentacleFrameDuration),
            Show(new(F.Tentacle, 2), TentacleFrameDuration),
            Show(new(F.Tentacle, 1), TentacleFrameDuration),
            GoTo(P.Tentacles),
        ],
        [P.MouthSpawnFlame] =
        [
            Show(new(F.Mouth, 2), MouthFrameDuration),
            Show(new(F.Mouth, 1), MouthFrameDuration),
            Cue(C.SpawnCasualFlame),
            FallInto(P.MouthInitial),
        ],
        [P.MouthInitial] = Still(new(F.Mouth, 0)),
    });

    /// <summary>The eye program gazing in clockwise direction <paramref name="ordinal"/> from up.</summary>
    internal static PhantoonPartProgram Gaze(int ordinal) =>
        (uint)ordinal < 8
            ? P.EyeLookingUp + ordinal
            : throw new ArgumentOutOfRangeException(nameof(ordinal), ordinal, "Phantoon gazes in eight directions.");

    /// <summary>Shows one frame for an update, then holds it as the native sleep does.</summary>
    private static AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>[] Still(PhantoonPartFrame frame) =>
        [Show(frame, 1), Hold];

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> Show(PhantoonPartFrame frame, int duration) =>
        AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>.Show(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> Cue(PhantoonPartCue cue) =>
        AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>.Publish(cue);

    /// <summary>Continues at <paramref name="program"/>.</summary>
    private static AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> GoTo(PhantoonPartProgram program) =>
        AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>, authored next.</summary>
    private static AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> FallInto(PhantoonPartProgram program) =>
        AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>.FallInto(program);

    /// <summary>Holds the last frame, as the native sleep does.</summary>
    private static AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue> Hold =>
        AnimationProgramStep<PhantoonPartProgram, PhantoonPartFrame, PhantoonPartCue>.Hold;
}

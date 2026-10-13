using SuperMetroid.Core.Assets;
using K = SuperMetroid.Core.Game.KraidPartCue;
using P = SuperMetroid.Core.Game.KraidPartProgram;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The programs of Kraid's foot, arm, lints and fingernails, declared in the order the cartridge authors them in
/// bank $A7. Kraid's routines depend on that order: they ask whether a part's program lies before a given program,
/// as the native routines compare instruction pointers.
/// </summary>
public enum KraidPartProgram
{
    /// <summary><c>InstList_KraidFoot_Initial</c> (<c>$A7:86E7</c>).</summary>
    FootInitial,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_Neutral</c> (<c>$A7:86ED</c>).</summary>
    FootNeutral,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_0</c> (<c>$A7:86F3</c>).</summary>
    FootWalkingForward,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingForward_1</c> (<c>$A7:87BB</c>).</summary>
    FootWalkingForwardFinished,
    /// <summary><c>InstList_KraidFoot_LungeForward_0</c> (<c>$A7:87BD</c>).</summary>
    FootLungeForward,
    /// <summary><c>InstList_KraidFoot_LungeForward_1</c> (<c>$A7:8885</c>).</summary>
    FootLungeForwardFinished,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_0</c> (<c>$A7:8887</c>).</summary>
    FootWalkingBackward,
    /// <summary><c>InstList_KraidFoot_KraidIsBig_WalkingBackwards_1</c> (<c>$A7:8939</c>).</summary>
    FootWalkingBackwardLoop,
    /// <summary><c>InstList_KraidArm_Normal_0</c> (<c>$A7:89F3</c>).</summary>
    ArmNormal,
    /// <summary><c>InstList_KraidArm_Normal_1</c> (<c>$A7:8A37</c>).</summary>
    ArmNormalPause,
    /// <summary><c>InstList_KraidArm_Slow</c> (<c>$A7:8A41</c>).</summary>
    ArmSlow,
    /// <summary><c>InstList_KraidArm_RisingSinking</c> (<c>$A7:8AA4</c>).</summary>
    ArmRisingOrSinking,
    /// <summary><c>InstList_KraidArm_Dying_PreparingToLungeForward</c> (<c>$A7:8AF0</c>).</summary>
    ArmDyingOrPreparingToLunge,
    /// <summary><c>kKraid_Ilist_8AFE</c> (<c>$A7:8AFE</c>).</summary>
    LintInitial,
    /// <summary><c>kKraid_Ilist_8B04</c> (<c>$A7:8B04</c>).</summary>
    LintPostGrowth,
    /// <summary><c>InstList_KraidNail</c> (<c>$A7:8B0A</c>).</summary>
    NailLoop,
}

/// <summary>Which run of Kraid's part spritemaps a <see cref="KraidPartFrame"/> indexes.</summary>
public enum KraidPartFrameKind
{
    /// <summary>The foot's standing pose before the rise ends.</summary>
    FootStanding,
    /// <summary>The 34 foot poses.</summary>
    Foot,
    /// <summary>The 22 arm poses.</summary>
    Arm,
    /// <summary>The two belly-lint poses: initial and after growth.</summary>
    Lint,
    /// <summary>The eight fingernail poses.</summary>
    Nail,
}

/// <summary>One frame of Kraid's part programs.</summary>
/// <param name="Kind">The run the frame belongs to.</param>
/// <param name="Index">The frame's position in that run.</param>
public readonly record struct KraidPartFrame(KraidPartFrameKind Kind, int Index)
{
    /// <summary>The installed extended spritemap the frame shows.</summary>
    internal ushort Spritemap => KraidPartFrameDefinitions.Spritemap(this);
}

/// <summary>Gameplay cues of Kraid's part programs.</summary>
internal enum KraidPartCue
{
    /// <summary>Raises Kraid's body one pixel as the foot lifts.</summary>
    RaiseBody,
    /// <summary>Lowers Kraid's body one pixel as the foot lands, shaking the screen.</summary>
    LowerBodyAndShake,
    /// <summary>Queues the footstep sound.</summary>
    StepSound,
    /// <summary>Moves Kraid's body three pixels left.</summary>
    StepLeft,
    /// <summary>Moves Kraid's body three pixels right.</summary>
    StepRight,
    /// <summary>Switches the arm to its slow swing once Kraid is below half health.</summary>
    SlowArmBelowHalfHealth,
}

/// <summary>Kraid's typed part programs (#641), in place of their bank-$A7 instruction lists.</summary>
internal static class KraidAnimations
{
    /// <summary>Every foot, arm, lint and fingernail program.</summary>
    internal static AnimationProgramSet<KraidPartProgram, KraidPartFrame, KraidPartCue> Programs { get; } = new(
        new Dictionary<KraidPartProgram, AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>[]>
    {
        [P.FootInitial] =
        [
            Show(FootStanding(), 32767),
            Hold,
        ],
        [P.FootNeutral] =
        [
            Show(Foot(0), 32767),
            Hold,
        ],
        [P.FootWalkingForward] =
        [
            Show(Foot(0), 4),
            Show(Foot(1), 4),
            Show(Foot(2), 4),
            Show(Foot(3), 4),
            Show(Foot(4), 4),
            Show(Foot(5), 4),
            Show(Foot(6), 4),
            Show(Foot(7), 4),
            Show(Foot(8), 4),
            Show(Foot(9), 4),
            Show(Foot(10), 4),
            Show(Foot(11), 3),
            Show(Foot(12), 1),
            Show(Foot(13), 1),
            Show(Foot(14), 1),
            Show(Foot(15), 1),
            Show(Foot(16), 1),
            Show(Foot(17), 16),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(18), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 3),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(21), 1),
            Show(Foot(21), 3),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(22), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(23), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(24), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(Foot(25), 1),
            Cue(K.StepLeft),
            Show(Foot(26), 1),
            Cue(K.StepLeft),
            Show(Foot(27), 1),
            Cue(K.StepLeft),
            Show(Foot(28), 1),
            Cue(K.StepLeft),
            Show(Foot(29), 1),
            Cue(K.StepLeft),
            Show(Foot(30), 1),
            Cue(K.StepLeft),
            Show(Foot(31), 1),
            Show(Foot(0), 1),
            FallInto(P.FootWalkingForwardFinished),
        ],
        [P.FootWalkingForwardFinished] =
        [
            Hold,
        ],
        [P.FootLungeForward] =
        [
            Show(Foot(0), 1),
            Show(Foot(1), 1),
            Show(Foot(2), 1),
            Show(Foot(3), 1),
            Show(Foot(4), 1),
            Show(Foot(5), 1),
            Show(Foot(6), 1),
            Show(Foot(7), 1),
            Show(Foot(8), 1),
            Show(Foot(9), 1),
            Show(Foot(10), 1),
            Show(Foot(11), 1),
            Show(Foot(12), 1),
            Show(Foot(13), 1),
            Show(Foot(14), 1),
            Show(Foot(15), 1),
            Show(Foot(16), 1),
            Show(Foot(17), 4),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(18), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(19), 1),
            Show(Foot(19), 1),
            Cue(K.RaiseBody),
            Cue(K.StepLeft),
            Show(Foot(21), 1),
            Show(Foot(21), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(22), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(23), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Show(Foot(24), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepLeft),
            Cue(K.StepSound),
            Show(Foot(25), 1),
            Cue(K.StepLeft),
            Show(Foot(26), 1),
            Cue(K.StepLeft),
            Show(Foot(27), 1),
            Cue(K.StepLeft),
            Show(Foot(28), 1),
            Cue(K.StepLeft),
            Show(Foot(29), 1),
            Cue(K.StepLeft),
            Show(Foot(30), 1),
            Show(Foot(31), 1),
            Cue(K.StepLeft),
            Show(Foot(0), 1),
            FallInto(P.FootLungeForwardFinished),
        ],
        [P.FootLungeForwardFinished] =
        [
            Hold,
        ],
        [P.FootWalkingBackward] =
        [
            Cue(K.StepRight),
            Show(Foot(33), 4),
            Cue(K.StepRight),
            Show(Foot(32), 1),
            Cue(K.StepRight),
            Show(Foot(30), 1),
            Cue(K.StepRight),
            Show(Foot(29), 1),
            Cue(K.StepRight),
            Show(Foot(28), 1),
            Cue(K.StepRight),
            Show(Foot(27), 1),
            Cue(K.StepRight),
            Show(Foot(26), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(25), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(24), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(23), 1),
            Cue(K.RaiseBody),
            Cue(K.StepRight),
            Show(Foot(22), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(Foot(21), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(Foot(20), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepRight),
            Show(Foot(19), 1),
            Cue(K.LowerBodyAndShake),
            Cue(K.StepSound),
            Show(Foot(18), 1),
            Show(Foot(17), 20),
            Show(Foot(16), 4),
            Show(Foot(15), 4),
            Show(Foot(14), 4),
            Show(Foot(13), 4),
            Show(Foot(12), 4),
            Show(Foot(11), 4),
            Show(Foot(10), 4),
            Show(Foot(9), 4),
            Show(Foot(8), 1),
            Show(Foot(7), 1),
            Show(Foot(6), 1),
            Show(Foot(5), 1),
            Show(Foot(4), 1),
            Show(Foot(3), 1),
            Show(Foot(2), 1),
            Show(Foot(1), 1),
            FallInto(P.FootWalkingBackwardLoop),
        ],
        [P.FootWalkingBackwardLoop] =
        [
            GoTo(P.FootWalkingBackward),
        ],
        [P.ArmNormal] =
        [
            Show(Arm(0), 6),
            Show(Arm(1), 6),
            Show(Arm(2), 6),
            Show(Arm(3), 6),
            Show(Arm(4), 6),
            Show(Arm(5), 6),
            Show(Arm(6), 6),
            Show(Arm(7), 6),
            Show(Arm(8), 6),
            Show(Arm(9), 6),
            Show(Arm(7), 6),
            Show(Arm(6), 6),
            Show(Arm(5), 6),
            Show(Arm(4), 6),
            Show(Arm(3), 6),
            Show(Arm(2), 6),
            Show(Arm(1), 6),
            FallInto(P.ArmNormalPause),
        ],
        [P.ArmNormalPause] =
        [
            Show(Arm(0), 32),
            Cue(K.SlowArmBelowHalfHealth),
            GoTo(P.ArmNormal),
        ],
        [P.ArmSlow] =
        [
            Show(Arm(0), 8),
            Show(Arm(1), 8),
            Show(Arm(2), 8),
            Show(Arm(3), 8),
            Show(Arm(4), 8),
            Show(Arm(5), 8),
            Show(Arm(6), 8),
            Show(Arm(7), 8),
            Show(Arm(8), 8),
            Show(Arm(9), 8),
            Show(Arm(7), 8),
            Show(Arm(6), 8),
            Show(Arm(5), 8),
            Show(Arm(4), 8),
            Show(Arm(3), 8),
            Show(Arm(2), 8),
            Show(Arm(1), 8),
            Show(Arm(0), 48),
            Cue(K.SlowArmBelowHalfHealth),
            GoTo(P.ArmSlow),
        ],
        [P.ArmRisingOrSinking] =
        [
            Show(Arm(10), 6),
            Show(Arm(11), 6),
            Show(Arm(12), 6),
            Show(Arm(13), 6),
            Show(Arm(14), 6),
            Show(Arm(15), 6),
            Show(Arm(16), 6),
            Show(Arm(17), 6),
            Show(Arm(18), 6),
            Show(Arm(19), 6),
            Show(Arm(17), 6),
            Show(Arm(16), 6),
            Show(Arm(15), 6),
            Show(Arm(14), 6),
            Show(Arm(13), 6),
            Show(Arm(12), 6),
            Show(Arm(11), 6),
            Show(Arm(10), 32),
            GoTo(P.ArmRisingOrSinking),
        ],
        [P.ArmDyingOrPreparingToLunge] =
        [
            Show(Arm(0), 6),
            Show(Arm(20), 6),
            Show(Arm(21), 32767),
            Hold,
        ],
        [P.LintInitial] =
        [
            Show(Lint(0), 32767),
            Hold,
        ],
        [P.LintPostGrowth] =
        [
            Show(Lint(1), 32767),
            Hold,
        ],
        [P.NailLoop] =
        [
            Show(Nail(0), 3),
            Show(Nail(1), 3),
            Show(Nail(2), 3),
            Show(Nail(3), 3),
            Show(Nail(4), 3),
            Show(Nail(5), 3),
            Show(Nail(6), 3),
            Show(Nail(7), 3),
            GoTo(P.NailLoop),
        ],
    });

    /// <summary>The spritemaps the <paramref name="programs"/> show, in step order, repeats included.</summary>
    internal static IEnumerable<ushort> Spritemaps(params KraidPartProgram[] programs) =>
        programs.SelectMany(program => Programs.Steps(program))
            .Where(step => step.Kind == AnimationProgramStepKind.Show)
            .Select(step => step.Frame.Spritemap);

    /// <summary>The foot's standing pose.</summary>
    private static KraidPartFrame FootStanding() => new(KraidPartFrameKind.FootStanding, 0);

    /// <summary>A foot pose.</summary>
    private static KraidPartFrame Foot(int index) => new(KraidPartFrameKind.Foot, index);

    /// <summary>An arm pose.</summary>
    private static KraidPartFrame Arm(int index) => new(KraidPartFrameKind.Arm, index);

    /// <summary>A belly-lint pose.</summary>
    private static KraidPartFrame Lint(int index) => new(KraidPartFrameKind.Lint, index);

    /// <summary>A fingernail pose.</summary>
    private static KraidPartFrame Nail(int index) => new(KraidPartFrameKind.Nail, index);

    /// <summary>Shows <paramref name="frame"/> for <paramref name="duration"/> updates.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> Show(KraidPartFrame frame, int duration) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.Show(frame, duration);

    /// <summary>Publishes <paramref name="cue"/>.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> Cue(KraidPartCue cue) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.Publish(cue);

    /// <summary>Continues at the start of <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> GoTo(KraidPartProgram program) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.GoTo(program);

    /// <summary>Runs on into <paramref name="program"/>.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> FallInto(KraidPartProgram program) =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.FallInto(program);

    /// <summary>Holds the last frame: the native sleep.</summary>
    private static AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue> Hold =>
        AnimationProgramStep<KraidPartProgram, KraidPartFrame, KraidPartCue>.Hold;
}

using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's typed rock programs (#641), in place of their bank-$86 instruction lists.</summary>
internal static class KraidRockAnimations
{
    /// <summary>Frames in the spat rock's burst.</summary>
    internal const int ShotPhaseCount = 5;

    /// <summary>The still poses last the native maximum, so the rock holds them for good in play.</summary>
    private const int StillFrameDuration = 32767;

    /// <summary>The timeline of <paramref name="program"/>.</summary>
    internal static AnimationTimeline<KraidRockFrame, KraidRockCue> Timeline(KraidRockProgram program) => program switch
    {
        KraidRockProgram.SharedPose => SharedPose,
        KraidRockProgram.RisingRightPose => RisingRightPose,
        KraidRockProgram.SpitRockShot => SpitRockShot,
        _ => throw new ArgumentOutOfRangeException(nameof(program), program, "Undefined Kraid rock program."),
    };

    /// <summary>The shared still pose.</summary>
    private static readonly AnimationTimeline<KraidRockFrame, KraidRockCue> SharedPose = new(
        AnimationStep<KraidRockFrame, KraidRockCue>.Show(new(KraidRockProgram.SharedPose, 0), StillFrameDuration),
        AnimationStep<KraidRockFrame, KraidRockCue>.Hold);

    /// <summary>The right rising rock's still pose.</summary>
    private static readonly AnimationTimeline<KraidRockFrame, KraidRockCue> RisingRightPose = new(
        AnimationStep<KraidRockFrame, KraidRockCue>.Show(new(KraidRockProgram.RisingRightPose, 0), StillFrameDuration),
        AnimationStep<KraidRockFrame, KraidRockCue>.Hold);

    /// <summary>The spat rock's burst: the palette pre-instruction, five four-update frames, then deletion.</summary>
    private static readonly AnimationTimeline<KraidRockFrame, KraidRockCue> SpitRockShot = new(
    [
        AnimationStep<KraidRockFrame, KraidRockCue>.Publish(KraidRockCue.UsePaletteZero),
        .. Enumerable.Range(0, ShotPhaseCount)
            .Select(phase => AnimationStep<KraidRockFrame, KraidRockCue>.Show(new(KraidRockProgram.SpitRockShot, phase), 4)),
        AnimationStep<KraidRockFrame, KraidRockCue>.Publish(KraidRockCue.Delete),
        AnimationStep<KraidRockFrame, KraidRockCue>.Hold,
    ]);
}

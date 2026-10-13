using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>Kraid's rock projectile programs, which the Kago bug's initial pose shares.</summary>
public enum KraidRockProgram
{
    /// <summary><c>InstList_EnemyProjectile_KraidRocks_KagoBug</c> ($86:9C7D): one still pose, shared by the spat, ceiling and left rising rocks and the Kago bug.</summary>
    SharedPose,
    /// <summary><c>InstList_EnemyProjectile_KraidFloorRocks_Right</c> ($86:9C83): the right rising rock's still pose.</summary>
    RisingRightPose,
    /// <summary><c>InstList_EnemyProjectile_Shot_KraidRockSpit</c> ($86:9C89): the spat rock's five-frame burst after a Samus shot.</summary>
    SpitRockShot,
}

/// <summary>One Kraid rock frame: its program and its position there.</summary>
/// <param name="Program">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct KraidRockFrame(KraidRockProgram Program, int Phase);

/// <summary>Gameplay cues of Kraid's rock programs.</summary>
internal enum KraidRockCue
{
    /// <summary>Gives the bursting rock the pre-instruction that draws it with palette zero.</summary>
    UsePaletteZero,
    /// <summary>Deletes the rock.</summary>
    Delete,
}

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

/// <summary>A Kraid rock's (or the Kago bug's initial) typed animation.</summary>
internal sealed class KraidRockAnimation(KraidRockProgram program) : EnemyProjectileTimelineAnimation<KraidRockFrame, KraidRockCue>
{
    /// <summary>The program played.</summary>
    internal KraidRockProgram Program { get; } = program;

    /// <inheritdoc/>
    private protected override AnimationTimeline<KraidRockFrame, KraidRockCue> Timeline => KraidRockAnimations.Timeline(Program);

    /// <inheritdoc/>
    private protected override ushort PresentationKey(KraidRockFrame frame) => KraidRockVisualDefinitions.PresentationKey(frame);
}

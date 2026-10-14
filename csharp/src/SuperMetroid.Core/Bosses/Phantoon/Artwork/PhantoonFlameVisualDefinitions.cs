namespace SuperMetroid.Core.Bosses.Phantoon.Artwork;

/// <summary>
/// The presentation boundary of Phantoon's flames (#641): the installed artwork key of every typed frame, the
/// bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class PhantoonFlameVisualDefinitions
{
    /// <summary>Distinct frames, in installed-artwork order.</summary>
    internal const int ProjectileFrameCount = 31;

    /// <summary><c>InstList_EnemyProjectile_PhantoonDestroyableFlame_Idle</c> at $86:975C.</summary>
    internal const ushort DestroyableIdleProgram = 0x975c;
    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_HitGround</c> at $86:976C.</summary>
    private const ushort CasualHitGroundProgram = 0x976c;
    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Bouncing</c> at $86:9772.</summary>
    private const ushort CasualBouncingProgram = 0x9772;
    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Resetting</c> at $86:9782.</summary>
    private const ushort CasualRestingProgram = 0x9782;
    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Dying</c> at $86:979A.</summary>
    private const ushort DyingProgram = 0x979a;
    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Rain_Falling</c> at $86:97AC.</summary>
    private const ushort RainImpactProgram = 0x97ac;
    /// <summary><c>InstList_EnemyProj_PhantoonDestroyableFlame_Casual_Falling_0</c> at $86:97B4.</summary>
    private const ushort CasualFallingProgram = 0x97b4;
    /// <summary><c>InstList_EnemyProjectile_PhantoonStartingFlames</c> at $86:97E8.</summary>
    internal const ushort StartingFlameProgram = 0x97e8;
    /// <summary><c>InstList_EnemyProjectile_Shot_PhantoonDestroyableFlames</c> at $86:97FA.</summary>
    internal const ushort DestroyableShotProgram = 0x97fa;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;
    /// <summary>Each falling phase loads its counter, shows two frames and repeats: sixteen bytes.</summary>
    private const int FallingPhaseBytes = 16;
    /// <summary>A falling phase's first frame follows its counter-loading instruction.</summary>
    private const int FallingFrameOffset = 4;

    /// <summary>Every frame in installed-artwork order.</summary>
    internal static IReadOnlyList<PhantoonFlameFrame> ProjectileFrames { get; } =
    [
        .. Frames(PhantoonFlameFrameKind.DestroyableIdle, PhantoonFlameAnimations.LoopPoseCount),
        .. Frames(PhantoonFlameFrameKind.CasualHitGround, 1),
        .. Frames(PhantoonFlameFrameKind.CasualBouncing, PhantoonFlameAnimations.LoopPoseCount),
        .. Frames(PhantoonFlameFrameKind.CasualResting, PhantoonFlameAnimations.RestingPoseCount),
        .. Frames(PhantoonFlameFrameKind.Dying, PhantoonFlameAnimations.DyingPoseCount),
        .. Frames(PhantoonFlameFrameKind.RainImpact, 1),
        .. Frames(PhantoonFlameFrameKind.CasualFalling, PhantoonFlameAnimations.FallingPhaseCount * PhantoonFlameAnimations.FallingPoseCount),
        .. Frames(PhantoonFlameFrameKind.StartingFlame, PhantoonFlameAnimations.LoopPoseCount),
        .. Frames(PhantoonFlameFrameKind.DestroyableShot, PhantoonFlameAnimations.ShotPoseCount),
    ];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(PhantoonFlameFrame frame) => unchecked((ushort)(frame.Kind switch
    {
        PhantoonFlameFrameKind.DestroyableIdle => DestroyableIdleProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.CasualHitGround => CasualHitGroundProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.CasualBouncing => CasualBouncingProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.CasualResting => CasualRestingProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.Dying => DyingProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.RainImpact => RainImpactProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.CasualFalling => CasualFallingProgram +
            FallingPhaseBytes * (frame.Phase / PhantoonFlameAnimations.FallingPoseCount) +
            FallingFrameOffset + FrameBytes * (frame.Phase % PhantoonFlameAnimations.FallingPoseCount),
        PhantoonFlameFrameKind.StartingFlame => StartingFlameProgram + FrameBytes * frame.Phase,
        PhantoonFlameFrameKind.DestroyableShot => DestroyableShotProgram + FrameBytes * frame.Phase,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Phantoon flame frame."),
    } + OperandOffset));

    /// <summary>The <paramref name="count"/> frames of <paramref name="kind"/>.</summary>
    private static IEnumerable<PhantoonFlameFrame> Frames(PhantoonFlameFrameKind kind, int count) =>
        Enumerable.Range(0, count).Select(phase => new PhantoonFlameFrame(kind, phase));
}
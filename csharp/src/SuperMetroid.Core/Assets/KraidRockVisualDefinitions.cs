using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The presentation boundary of Kraid's rocks (#641): the installed artwork key of every typed frame, the bank-$86
/// address of the spritemap operand that drew it.
/// </summary>
internal static class KraidRockVisualDefinitions
{
    /// <summary>Distinct frames: the two still poses and five burst frames.</summary>
    internal const int ProjectileFrameCount = 7;

    /// <summary><c>InstList_EnemyProjectile_KraidRocks_KagoBug</c> at $86:9C7D.</summary>
    private const ushort SharedPoseProgram = 0x9c7d;
    /// <summary><c>InstList_EnemyProjectile_KraidFloorRocks_Right</c> at $86:9C83.</summary>
    private const ushort RisingRightPoseProgram = 0x9c83;
    /// <summary>$86:9C8D: the burst's first frame, after the four-byte pre-instruction command at $86:9C89.</summary>
    private const ushort SpitRockShotFrames = 0x9c8d;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every frame in installed-artwork order: the two poses, then the burst.</summary>
    internal static IReadOnlyList<KraidRockFrame> ProjectileFrames { get; } =
    [
        new KraidRockFrame(KraidRockProgram.SharedPose, 0),
        new KraidRockFrame(KraidRockProgram.RisingRightPose, 0),
        .. Enumerable.Range(0, KraidRockAnimations.ShotPhaseCount).Select(phase => new KraidRockFrame(KraidRockProgram.SpitRockShot, phase)),
    ];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(KraidRockFrame frame) => unchecked((ushort)(frame.Program switch
    {
        KraidRockProgram.SharedPose => SharedPoseProgram,
        KraidRockProgram.RisingRightPose => RisingRightPoseProgram,
        KraidRockProgram.SpitRockShot => SpitRockShotFrames,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Kraid rock frame."),
    } + FrameBytes * frame.Phase + OperandOffset));
}


namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>
/// The presentation boundary of Mother Brain's turrets (#641): the installed artwork key of every typed frame, the
/// bank-$86 address of the spritemap operand that drew it.
/// </summary>
internal static class MotherBrainTurretVisualDefinitions
{
    /// <summary>Distinct frames: eight turret poses, eight bullet poses and five smoke frames.</summary>
    internal const int ProjectileFrameCount = 2 * MotherBrainTurretDefinitions.DirectionCount + MotherBrainTurretAnimations.ImpactPhaseCount;

    /// <summary>The first turret pose's list at $86:C101.</summary>
    private const ushort TurretPoses = 0xc101;
    /// <summary>The first bullet pose's list at $86:C143.</summary>
    private const ushort BulletPoses = 0xc143;
    /// <summary>The impact smoke's first frame at $86:C19E, after its palette and pre-instruction commands.</summary>
    private const ushort ImpactFrames = 0xc19e;
    /// <summary>Bytes in each pose's list: one frame and the sleep.</summary>
    private const int PoseBytes = 6;
    /// <summary>A frame's spritemap operand follows its two-byte duration.</summary>
    private const int OperandOffset = 2;
    /// <summary>Each timed frame is a duration and an operand.</summary>
    private const int FrameBytes = 4;

    /// <summary>Every frame in installed-artwork order: turret poses, bullet poses, then the smoke.</summary>
    internal static IReadOnlyList<MotherBrainTurretFrame> ProjectileFrames { get; } =
    [
        .. Enum.GetValues<MotherBrainTurretDirection>().Select(direction => new MotherBrainTurretFrame(MotherBrainTurretProgram.TurretPose, (int)direction)),
        .. Enum.GetValues<MotherBrainTurretDirection>().Select(direction => new MotherBrainTurretFrame(MotherBrainTurretProgram.BulletPose, (int)direction)),
        .. Enumerable.Range(0, MotherBrainTurretAnimations.ImpactPhaseCount).Select(phase => new MotherBrainTurretFrame(MotherBrainTurretProgram.BulletImpact, phase)),
    ];

    /// <summary>The installed presentation key of <paramref name="frame"/>.</summary>
    internal static ushort PresentationKey(MotherBrainTurretFrame frame) => unchecked((ushort)(frame.Program switch
    {
        MotherBrainTurretProgram.TurretPose => TurretPoses + PoseBytes * frame.Index + OperandOffset,
        MotherBrainTurretProgram.BulletPose => BulletPoses + PoseBytes * frame.Index + OperandOffset,
        MotherBrainTurretProgram.BulletImpact => ImpactFrames + FrameBytes * frame.Index + OperandOffset,
        _ => throw new ArgumentOutOfRangeException(nameof(frame), frame, "Undefined Mother Brain turret frame."),
    }));
}

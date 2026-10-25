namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts at $A6:D2D6.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>$A6:D2DD-D2E0: InitializeTailParts writes this bound independently of facing.</summary>
    internal static SnesAngle InitialMinimumClockwise { get; } = SnesAngle.FromRaw(0x3ff0);
    /// <summary>$A6:D2E4-D2E7: InitializeTailParts writes this bound before the live tail controller adjusts it.</summary>
    internal static SnesAngle InitialMaximumCounterClockwise { get; } = SnesAngle.FromRaw(0x4040);

    /// <summary>$A6:BC0A sets ideal tail spacing while releasing Samus.</summary>
    internal const ushort CarryReleaseInterSegmentSpacing = 8;
    /// <summary>$A6:BC11/$BC54 restores tail extension speed on carry phase expiry.</summary>
    internal const ushort CarryReleaseExtensionSpeed = 240;
    /// <summary>$A6:CB78/CB88, straight-down target angle.</summary>
    internal static SnesAngle DownAngle { get; } = SnesAngle.QuarterTurn;
    /// <summary>$A6:CD52/CDD8, pogo clockwise whip target.</summary>
    internal static SnesAngle PogoWhipAngle { get; } = SnesAngle.FromRaw(0x3f00);
    /// <summary>The whip's extension request while a segment chases a whip target.</summary>
    internal const ushort WhipDistance = 0x0c00;
    /// <summary>The axis a facing change reflects tail angles around: half of the 16-bit turn ($A6:D3F9).</summary>
    internal static SnesAngle MirrorAxisAngle { get; } = SnesAngle.HalfTurn;
    /// <summary>The first byte angle of the forward interval a whip may not aim through Ridley's body.</summary>
    internal const byte ForbiddenForwardStart = 0x18;
    /// <summary>The byte angle past the forbidden forward interval; a left-facing target clamps here.</summary>
    internal const byte ForbiddenForwardEnd = 0xe8;
    /// <summary>The bank-$3F expansion of a left-facing whip target's byte angle.</summary>
    internal const ushort LeftWhipTargetBase = 0x3f00;
    /// <summary>The bank-$40 expansion of a right-facing whip target's byte angle.</summary>
    internal const ushort RightWhipTargetBase = 0x4000;

    /// <summary>The longest each segment may extend while whipping, base through tip ($A6:CF5A-$D09E).</summary>
    internal static ushort MaximumDistance(int segmentIndex) => segmentIndex switch
    {
        0 => 0,
        1 or 2 => 0x1800,
        3 or 4 => 0x1600,
        5 => 0x1200,
        6 => 0x0500,
        _ => throw new ArgumentOutOfRangeException(nameof(segmentIndex)),
    };
    /// <summary>$A6:CE08, descending stab extension request.</summary>
    internal const ushort StabDistance = 0x0a00;
    /// <summary>$A6:B76E, bounce extension request.</summary>
    internal const ushort BounceDistance = 0x0c00;
    /// <summary>$A6:CC12, SetRidleyTailAngleExtrema_minClockwiseAngle for left/front/right facing.</summary>
    internal static SnesAngle MinimumClockwise(RidleyFacing facing) => SnesAngle.FromRaw(facing == RidleyFacing.Right ? (ushort)0x3fc0 : (ushort)0x3ff0);
    /// <summary>$A6:CC18, SetRidleyTailAngleExtrema_maxCounterClockwiseAngle for left/front/right facing.</summary>
    internal static SnesAngle MaximumCounterClockwise(RidleyFacing facing) => SnesAngle.FromRaw(facing == RidleyFacing.Right ? (ushort)0x4010 : (ushort)0x4040);

    /// <summary>$A6:D38A begins InitializeTailParts.angles at $4000, one quarter of the 16-bit full turn.</summary>
    private const int InitialBaseAngle = 0x10000 / 4;
    /// <summary>$A6:D2FD loads $0010 into Ridley.idealInterSegmentTailAngle; the initial angles advance by that separation.</summary>
    internal const ushort IdealInterSegmentSpacing = 0x0010;
    /// <summary>$A6:D37C initial base separation, in8.8 pixels. The two-pixel length is an authored scalar (see residualScalarInputsReview).</summary>
    private const ushort BaseRestDistance = 2 << 8;
    /// <summary>$A6:D37E..D387 initial shaft separation and $CF7F/$CFB5/$CFEB/$D021/$D057 shrink thresholds. The eight-pixel length is an authored scalar.</summary>
    private const ushort ShaftRestDistance = 8 << 8;
    /// <summary>$A6:D388 initial tip separation and $D08D shrink threshold. The five-pixel length is an authored scalar.</summary>
    private const ushort TipRestDistance = 5 << 8;

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static ushort RestDistance(int segmentIndex) => segmentIndex switch
    {
        0 => BaseRestDistance,
        >= 1 and <= 5 => ShaftRestDistance,
        6 => TipRestDistance,
        _ => throw new ArgumentOutOfRangeException(nameof(segmentIndex)),
    };

    /// <summary>$A6:D38A..D397 contains seven successive initial link angles, base through tip.</summary>
    internal static ushort InitialAngle(int segmentIndex) => (uint)segmentIndex < 7
        ? (ushort)(InitialBaseAngle + segmentIndex * IdealInterSegmentSpacing)
        : throw new ArgumentOutOfRangeException(nameof(segmentIndex));
}

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts at $A6:D2D6.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>$A6:D2DD-D2E0: InitializeTailParts writes this bound independently of facing.</summary>
    internal static RotationAngle InitialMinimumClockwise { get; } = RotationAngle.FromRaw(16368);
    /// <summary>$A6:D2E4-D2E7: InitializeTailParts writes this bound before the live tail controller adjusts it.</summary>
    internal static RotationAngle InitialMaximumCounterClockwise { get; } = RotationAngle.FromRaw(16448);

    /// <summary>$A6:BC0A sets ideal tail spacing while releasing Samus.</summary>
    internal const ushort CarryReleaseInterSegmentSpacing = 8;
    /// <summary>$A6:BC11/$BC54 restores tail extension speed on carry phase expiry.</summary>
    internal const ushort CarryReleaseExtensionSpeed = 240;
    /// <summary>$A6:CB78/CB88, straight-down target angle.</summary>
    internal static RotationAngle DownAngle { get; } = RotationAngle.QuarterTurn;
    /// <summary>$A6:CD52/CDD8, pogo clockwise whip target.</summary>
    internal static RotationAngle PogoWhipAngle { get; } = RotationAngle.FromRaw(16128);
    /// <summary>The whip's extension request while a segment chases a whip target.</summary>
    internal const ushort WhipDistance = 3072;
    /// <summary>The axis a facing change reflects tail angles around: half of the 16-bit turn ($A6:D3F9).</summary>
    internal static RotationAngle MirrorAxisAngle { get; } = RotationAngle.HalfTurn;
    /// <summary>The first byte angle of the forward interval a whip may not aim through Ridley's body.</summary>
    internal const byte ForbiddenForwardStart = 24;
    /// <summary>The byte angle past the forbidden forward interval; a left-facing target clamps here.</summary>
    internal const byte ForbiddenForwardEnd = 232;
    /// <summary>The bank-$3F expansion of a left-facing whip target's byte angle.</summary>
    internal const ushort LeftWhipTargetBase = 16128;
    /// <summary>The bank-$40 expansion of a right-facing whip target's byte angle.</summary>
    internal const ushort RightWhipTargetBase = 16384;

    /// <summary>The longest each segment may extend while whipping, base through tip ($A6:CF5A-$D09E).</summary>
    internal static readonly ushort[] MaximumDistances = [0, 24 << 8, 24 << 8, 22 << 8, 22 << 8, 18 << 8, 5 << 8];
    /// <summary>$A6:CE08, descending stab extension request.</summary>
    internal const ushort StabDistance = 2560;
    /// <summary>$A6:B76E, bounce extension request.</summary>
    internal const ushort BounceDistance = 3072;
    /// <summary>$A6:CC12, SetRidleyTailAngleExtrema_minClockwiseAngle for left/front/right facing.</summary>
    internal static readonly Dictionary<RidleyFacing, RotationAngle> MinimumClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RotationAngle.FromRaw(16368),
        [RidleyFacing.Turning] = RotationAngle.FromRaw(16368),
        [RidleyFacing.Right] = RotationAngle.FromRaw(16320),
    };
    /// <summary>$A6:CC18, SetRidleyTailAngleExtrema_maxCounterClockwiseAngle for left/front/right facing.</summary>
    internal static readonly Dictionary<RidleyFacing, RotationAngle> MaximumCounterClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RotationAngle.FromRaw(16448),
        [RidleyFacing.Turning] = RotationAngle.FromRaw(16448),
        [RidleyFacing.Right] = RotationAngle.FromRaw(16400),
    };

    /// <summary>$A6:D38A begins InitializeTailParts.angles at $4000, one quarter of the 16-bit full turn.</summary>
    private const int InitialBaseAngle = 65536 / 4;
    /// <summary>$A6:D2FD loads $0010 into Ridley.idealInterSegmentTailAngle; the initial angles advance by that separation.</summary>
    internal const ushort IdealInterSegmentSpacing = 16;
    /// <summary>$A6:D37C initial base separation, in8.8 pixels. The two-pixel length is an authored scalar (see residualScalarInputsReview).</summary>
    private const ushort BaseRestDistance = 2 << 8;
    /// <summary>$A6:D37E..D387 initial shaft separation and $CF7F/$CFB5/$CFEB/$D021/$D057 shrink thresholds. The eight-pixel length is an authored scalar.</summary>
    private const ushort ShaftRestDistance = 8 << 8;
    /// <summary>$A6:D388 initial tip separation and $D08D shrink threshold. The five-pixel length is an authored scalar.</summary>
    private const ushort TipRestDistance = 5 << 8;

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static readonly ushort[] RestDistances =
        [BaseRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, TipRestDistance];

    /// <summary>$A6:D38A..D397 contains seven successive initial link angles, base through tip.</summary>
    internal static readonly ushort[] InitialAngles =
    [
        InitialBaseAngle, InitialBaseAngle + IdealInterSegmentSpacing, InitialBaseAngle + 2 * IdealInterSegmentSpacing,
        InitialBaseAngle + 3 * IdealInterSegmentSpacing, InitialBaseAngle + 4 * IdealInterSegmentSpacing,
        InitialBaseAngle + 5 * IdealInterSegmentSpacing, InitialBaseAngle + 6 * IdealInterSegmentSpacing,
    ];
}

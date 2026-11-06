namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>InitializeTailParts writes this bound independently of facing.</summary>
    internal static RotationAngle InitialMinimumClockwise { get; } = RotationAngle.FromRaw(16368);
    /// <summary>InitializeTailParts writes this bound before the live tail controller adjusts it.</summary>
    internal static RotationAngle InitialMaximumCounterClockwise { get; } = RotationAngle.FromRaw(16448);

    /// <summary>The carry's release sets the ideal tail spacing while releasing Samus.</summary>
    internal const int CarryReleaseInterSegmentSpacing = 8;
    /// <summary>The carry restores tail extension speed on carry phase expiry.</summary>
    internal const int CarryReleaseExtensionSpeed = 240;
    /// <summary>Straight-down target angle.</summary>
    internal static RotationAngle DownAngle { get; } = RotationAngle.QuarterTurn;
    /// <summary>Pogo clockwise whip target.</summary>
    internal static RotationAngle PogoWhipAngle { get; } = RotationAngle.FromRaw(16128);
    /// <summary>The whip's extension request while a segment chases a whip target.</summary>
    internal const int WhipDistance = 3072;
    /// <summary>The axis a facing change reflects tail angles around: half of the 16-bit turn.</summary>
    internal static RotationAngle MirrorAxisAngle { get; } = RotationAngle.HalfTurn;
    /// <summary>The first byte angle of the forward interval a whip may not aim through Ridley's body.</summary>
    internal const int ForbiddenForwardStart = 24;
    /// <summary>The byte angle past the forbidden forward interval; a left-facing target clamps here.</summary>
    internal const int ForbiddenForwardEnd = 232;
    /// <summary>The expansion of a left-facing whip target's byte angle.</summary>
    internal const int LeftWhipTargetBase = 16128;
    /// <summary>The expansion of a right-facing whip target's byte angle.</summary>
    internal const int RightWhipTargetBase = 16384;

    /// <summary>The longest each segment may extend while whipping, base through tip.</summary>
    internal static readonly int[] MaximumDistances = [0, 24 << 8, 24 << 8, 22 << 8, 22 << 8, 18 << 8, 5 << 8];
    /// <summary>Descending stab extension request.</summary>
    internal const int StabDistance = 2560;
    /// <summary>Bounce extension request.</summary>
    internal const int BounceDistance = 3072;
    /// <summary>SetRidleyTailAngleExtrema_minClockwiseAngle for left/front/right facing.</summary>
    internal static readonly Dictionary<RidleyFacing, RotationAngle> MinimumClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RotationAngle.FromRaw(16368),
        [RidleyFacing.Turning] = RotationAngle.FromRaw(16368),
        [RidleyFacing.Right] = RotationAngle.FromRaw(16320),
    };
    /// <summary>SetRidleyTailAngleExtrema_maxCounterClockwiseAngle for left/front/right facing.</summary>
    internal static readonly Dictionary<RidleyFacing, RotationAngle> MaximumCounterClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RotationAngle.FromRaw(16448),
        [RidleyFacing.Turning] = RotationAngle.FromRaw(16448),
        [RidleyFacing.Right] = RotationAngle.FromRaw(16400),
    };

    /// <summary>InitializeTailParts begins its angles at one quarter of the 16-bit full turn.</summary>
    private const int InitialBaseAngle = 65536 / 4;
    /// <summary>Tail initialization loads 16 into Ridley.idealInterSegmentTailAngle; the initial angles advance by that separation.</summary>
    internal const int IdealInterSegmentSpacing = 16;
    /// <summary>The initial base separation, in8.8 pixels. The two-pixel length is an authored scalar (see residualScalarInputsReview).</summary>
    private const int BaseRestDistance = 2 << 8;
    /// <summary>The initial shaft separation and its shrink thresholds. The eight-pixel length is an authored scalar.</summary>
    private const int ShaftRestDistance = 8 << 8;
    /// <summary>The initial tip separation and its shrink threshold. The five-pixel length is an authored scalar.</summary>
    private const int TipRestDistance = 5 << 8;

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static readonly int[] RestDistances =
        [BaseRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, ShaftRestDistance, TipRestDistance];

    /// <summary>The seven successive initial link angles, base through tip.</summary>
    internal static readonly int[] InitialAngles =
    [
        InitialBaseAngle, InitialBaseAngle + IdealInterSegmentSpacing, InitialBaseAngle + 2 * IdealInterSegmentSpacing,
        InitialBaseAngle + 3 * IdealInterSegmentSpacing, InitialBaseAngle + 4 * IdealInterSegmentSpacing,
        InitialBaseAngle + 5 * IdealInterSegmentSpacing, InitialBaseAngle + 6 * IdealInterSegmentSpacing,
    ];
}

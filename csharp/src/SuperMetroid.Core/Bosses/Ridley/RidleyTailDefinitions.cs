namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>InitializeTailParts writes this bound independently of facing.</summary>
    internal static RotationAngle InitialMinimumClockwise { get; } = RotationAngle.FromSteps(63, 240);
    /// <summary>InitializeTailParts writes this bound before the live tail controller adjusts it.</summary>
    internal static RotationAngle InitialMaximumCounterClockwise { get; } = RotationAngle.FromSteps(64, 64);

    /// <summary>The carry's release sets the ideal tail spacing while releasing Samus.</summary>
    internal const int CarryReleaseInterSegmentSpacing = 8;
    /// <summary>The carry restores tail extension speed on carry phase expiry.</summary>
    internal const int CarryReleaseExtensionSpeed = 240;
    /// <summary>Straight-down target angle.</summary>
    internal static RotationAngle DownAngle { get; } = RotationAngle.QuarterTurn;
    /// <summary>Pogo clockwise whip target.</summary>
    internal static RotationAngle PogoWhipAngle { get; } = RotationAngle.FromSteps(63);
    /// <summary>The whip's extension request while a segment chases a whip target, twelve pixels in 256ths.</summary>
    internal static readonly int WhipDistance = EightEight.FromWhole(12);
    /// <summary>The axis a facing change reflects tail angles around: half of the 16-bit turn.</summary>
    internal static RotationAngle MirrorAxisAngle { get; } = RotationAngle.HalfTurn;
    /// <summary>The first byte angle of the forward interval a whip may not aim through Ridley's body.</summary>
    internal const int ForbiddenForwardStart = 24;
    /// <summary>The byte angle past the forbidden forward interval; a left-facing target clamps here.</summary>
    internal const int ForbiddenForwardEnd = 232;
    /// <summary>The angle a left-facing whip target's byte angle is added to, in the angle's 65536ths of a turn.</summary>
    internal static RotationAngle LeftWhipTargetBase { get; } = RotationAngle.FromSteps(63);
    /// <summary>The angle a right-facing whip target's byte angle is added to, in the angle's 65536ths of a turn.</summary>
    internal static RotationAngle RightWhipTargetBase { get; } = RotationAngle.QuarterTurn;

    /// <summary>The longest each segment may extend while whipping, base through tip, in 256ths of a pixel.</summary>
    internal static readonly int[] MaximumDistances =
    [
        0, EightEight.FromWhole(24), EightEight.FromWhole(24), EightEight.FromWhole(22), EightEight.FromWhole(22),
        EightEight.FromWhole(18), EightEight.FromWhole(5),
    ];
    /// <summary>Descending stab extension request, ten pixels in 256ths.</summary>
    internal static readonly int StabDistance = EightEight.FromWhole(10);
    /// <summary>Bounce extension request, twelve pixels in 256ths.</summary>
    internal static readonly int BounceDistance = EightEight.FromWhole(12);
    /// <summary>The tail's minimum clockwise angle for left/front/right facing.</summary>
    internal static readonly Dictionary<RidleyFacing, RotationAngle> MinimumClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RotationAngle.FromSteps(63, 240),
        [RidleyFacing.Turning] = RotationAngle.FromSteps(63, 240),
        [RidleyFacing.Right] = RotationAngle.FromSteps(63, 192),
    };
    /// <summary>The tail's maximum counter-clockwise angle for left/front/right facing.</summary>
    internal static readonly Dictionary<RidleyFacing, RotationAngle> MaximumCounterClockwiseAngles = new()
    {
        [RidleyFacing.Left] = RotationAngle.FromSteps(64, 64),
        [RidleyFacing.Turning] = RotationAngle.FromSteps(64, 64),
        [RidleyFacing.Right] = RotationAngle.FromSteps(64, 16),
    };

    /// <summary>InitializeTailParts begins its angles at a quarter turn, in the angle's 65536ths of a turn.</summary>
    private static readonly int InitialBaseAngle = RotationAngle.QuarterTurn.RawValue;
    /// <summary>Tail initialization loads 16 into Ridley.idealInterSegmentTailAngle; the initial angles advance by that separation.</summary>
    internal const int IdealInterSegmentSpacing = 16;
    /// <summary>The initial base separation, two pixels in 256ths. The length is an authored scalar (see residualScalarInputsReview).</summary>
    private static readonly int BaseRestDistance = EightEight.FromWhole(2);
    /// <summary>The initial shaft separation and its shrink thresholds, eight pixels in 256ths. The length is an authored scalar.</summary>
    private static readonly int ShaftRestDistance = EightEight.FromWhole(8);
    /// <summary>The initial tip separation and its shrink threshold, five pixels in 256ths. The length is an authored scalar.</summary>
    private static readonly int TipRestDistance = EightEight.FromWhole(5);

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

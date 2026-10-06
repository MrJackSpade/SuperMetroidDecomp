namespace SuperMetroid.Core.Game;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts at $A6:D2D6.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>$A6:D2DD-D2E0: InitializeTailParts writes this bound independently of facing.</summary>
    internal const ushort InitialMinimumClockwise = 0x3ff0;
    /// <summary>$A6:D2E4-D2E7: InitializeTailParts writes this bound before the live tail controller adjusts it.</summary>
    internal const ushort InitialMaximumCounterClockwise = 0x4040;

    /// <summary>$A6:BC0A sets ideal tail spacing while releasing Samus.</summary>
    internal const ushort CarryReleaseInterSegmentAngle = 8;
    /// <summary>$A6:BC11/$BC54 restores tail extension speed on carry phase expiry.</summary>
    internal const ushort CarryReleaseExtensionSpeed = 240;
    /// <summary>$A6:CB23 -> CBC0, neutral tail.</summary>
    internal const ushort Neutral = 1;
    /// <summary>$A6:CB25 -> CB33, immediate pogo setup.</summary>
    internal const ushort PogoSetup = 2;
    /// <summary>$A6:CB27 -> CB45, point tail down.</summary>
    internal const ushort PointDown = 3;
    /// <summary>$A6:CB29 -> CBC7, normal pogo tail.</summary>
    internal const ushort Pogo = 4;
    /// <summary>$A6:CB2B -> CBCE, descending stab setup.</summary>
    internal const ushort StabSetup = 5;
    /// <summary>$A6:CB2D -> CB4E, descending stab.</summary>
    internal const ushort Stab = 6;
    /// <summary>$A6:CB78/CB88, straight-down target angle.</summary>
    internal const ushort DownAngle = 0x4000;
    /// <summary>$A6:CD52/CDD8, pogo clockwise whip target.</summary>
    internal const ushort PogoWhipAngle = 0x3f00;
    /// <summary>$A6:CE08, descending stab extension request.</summary>
    internal const ushort StabDistance = 0x0a00;
    /// <summary>$A6:B76E, bounce extension request.</summary>
    internal const ushort BounceDistance = 0x0c00;
    /// <summary>$A6:CC12, SetRidleyTailAngleExtrema_minClockwiseAngle for left/front/right facing.</summary>
    internal static ushort MinimumClockwise(ushort facing) => facing == 2 ? (ushort)0x3fc0 : (ushort)0x3ff0;
    /// <summary>$A6:CC18, SetRidleyTailAngleExtrema_maxCounterClockwiseAngle for left/front/right facing.</summary>
    internal static ushort MaximumCounterClockwise(ushort facing) => facing == 2 ? (ushort)0x4010 : (ushort)0x4040;

    /// <summary>$A6:D38A begins InitializeTailParts.angles at $4000, one quarter of the 16-bit full turn.</summary>
    private const int InitialBaseAngle = 0x10000 / 4;
    /// <summary>$A6:D2FD loads $0010 into Ridley.idealInterSegmentTailAngle; the initial angles advance by that separation.</summary>
    internal const ushort IdealInterSegmentAngle = 0x0010;
    /// <summary>$A6:D37C initial base separation, in8.8 pixels. Its chosen two-pixel length remains required issue-1165 input.</summary>
    private const ushort BaseRestDistance = 2 << 8;
    /// <summary>$A6:D37E..D387 initial shaft separation and $CF7F/$CFB5/$CFEB/$D021/$D057 shrink thresholds. The chosen eight-pixel length remains required input.</summary>
    private const ushort ShaftRestDistance = 8 << 8;
    /// <summary>$A6:D388 initial tip separation and $D08D shrink threshold. The chosen five-pixel length remains required input.</summary>
    private const ushort TipRestDistance = 5 << 8;

    /// <summary>The tail returns to its initialized rest geometry: base, five shaft links, then tip.</summary>
    internal static ushort RestDistance(int segmentIndex) => segmentIndex switch
    {
        0 => BaseRestDistance,
        >= 1 and <= 5 => ShaftRestDistance,
        6 => TipRestDistance,
        _ => throw new IndexOutOfRangeException(),
    };

    /// <summary>$A6:D38A..D397 contains seven successive initial link angles, base through tip.</summary>
    internal static ushort InitialAngle(int segmentIndex) => (uint)segmentIndex < 7
        ? (ushort)(InitialBaseAngle + segmentIndex * IdealInterSegmentAngle)
        : throw new ArgumentOutOfRangeException(nameof(segmentIndex));
}

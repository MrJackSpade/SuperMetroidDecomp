namespace SuperMetroid.Core.Game;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts at $A6:D2D6.</summary>
internal static class RidleyTailDefinitions
{
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

namespace SuperMetroid.Core.Game;

/// <summary>Initial articulated-tail angular geometry from InitializeTailParts at $A6:D2D6.</summary>
internal static class RidleyTailDefinitions
{
    /// <summary>$A6:D38A begins InitializeTailParts.angles at $4000, one quarter of the 16-bit full turn.</summary>
    private const int InitialBaseAngle = 0x10000 / 4;
    /// <summary>$A6:D2FD loads $0010 into Ridley.idealInterSegmentTailAngle; the initial angles advance by that separation.</summary>
    internal const ushort IdealInterSegmentAngle = 0x0010;
    /// <summary>$A6:D38A..D397 contains seven successive initial link angles, base through tip.</summary>
    internal static ushort InitialAngle(int segmentIndex) => (uint)segmentIndex < 7
        ? (ushort)(InitialBaseAngle + segmentIndex * IdealInterSegmentAngle)
        : throw new ArgumentOutOfRangeException(nameof(segmentIndex));
}
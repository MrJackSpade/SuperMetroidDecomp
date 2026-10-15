namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge's 8-bit enemy angles: 256 units per turn, wrapping in byte arithmetic. A half-turn bit set means the
/// angle lies in the turn's second half.
/// </summary>
internal static class EnemyByteAngleDefinitions
{
    /// <summary>An eighth of a turn.</summary>
    internal const int EighthTurn = 0x20;

    /// <summary>A quarter turn; adding it to an angle selects the cosine phase of a sine table.</summary>
    internal const int QuarterTurn = 0x40;

    /// <summary>A half turn, and the bit that marks an angle in the turn's second half.</summary>
    internal const int HalfTurn = 0x80;

    /// <summary>Folds an angle onto a 128-entry half-wave table.</summary>
    internal const int HalfWaveIndexMask = 0x7f;
}

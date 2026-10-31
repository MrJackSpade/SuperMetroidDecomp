namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The cartridge's 8-bit enemy angles: 256 units per turn, wrapping in byte arithmetic. Which half of the turn an angle
/// lies in, and its half-wave table index, are <see cref="ByteAngle"/> operations.
/// </summary>
internal static class EnemyByteAngleDefinitions
{
    /// <summary>An eighth of a turn.</summary>
    internal const int EighthTurn = 32;

    /// <summary>A quarter turn; adding it to an angle selects the cosine phase of a sine table.</summary>
    internal const int QuarterTurn = 64;

    /// <summary>A half turn.</summary>
    internal const int HalfTurn = 128;
}

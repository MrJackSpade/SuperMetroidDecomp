namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The game's enemy angles: 256 units per turn, wrapping at a full turn. Which half of the turn an angle
/// lies in, and its position within that half, are <see cref="TrigonometryAngle"/> operations.
/// </summary>
internal static class EnemyTrigonometryAngleDefinitions
{
    /// <summary>An eighth of a turn.</summary>
    internal const int EighthTurn = 32;

    /// <summary>A quarter turn; adding it to an angle selects the cosine phase of a sine table.</summary>
    internal const int QuarterTurn = 64;

    /// <summary>A half turn.</summary>
    internal const int HalfTurn = 128;
}

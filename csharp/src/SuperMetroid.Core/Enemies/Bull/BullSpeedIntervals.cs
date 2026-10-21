namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The intervals between a Bull's acceleration and deceleration steps.</summary>
/// <param name="Acceleration">The acceleration.</param>
/// <param name="Deceleration">The deceleration.</param>
public readonly record struct BullSpeedIntervals(int Acceleration, int Deceleration);

namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>The intervals between a Bull's acceleration and deceleration steps.</summary>
/// <param name="Acceleration">The updates between the Bull's acceleration steps.</param>
/// <param name="Deceleration">The updates between the Bull's deceleration steps.</param>
public readonly partial record struct BullSpeedIntervals(int Acceleration, int Deceleration);

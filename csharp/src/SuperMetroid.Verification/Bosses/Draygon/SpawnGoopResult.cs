namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The velocity of one spawned goop's flight.</summary>
/// <param name="Velocity">Its velocity along its heading.</param>
internal readonly record struct SpawnGoopResult(FixedVector<RoomPixels> Velocity);

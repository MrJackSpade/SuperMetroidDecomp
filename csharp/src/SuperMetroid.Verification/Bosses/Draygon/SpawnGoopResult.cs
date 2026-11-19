namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The heading and the velocity of one spawned goop's flight.</summary>
/// <param name="Heading">The direction the goop flies in.</param>
/// <param name="Velocity">Its velocity along the heading.</param>
internal readonly record struct SpawnGoopResult(TrigonometryAngle Heading, FixedVector<RoomPixels> Velocity);

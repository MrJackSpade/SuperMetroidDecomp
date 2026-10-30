namespace SuperMetroid.Verification.Bosses.Draygon;

/// <summary>The heading and the two speed parts of one spawned goop's flight.</summary>
/// <param name="Heading">The direction the goop flies in.</param>
/// <param name="HorizontalSpeed">The unsigned horizontal part of its speed.</param>
/// <param name="VerticalSpeed">The unsigned vertical part of its speed.</param>
internal readonly record struct SpawnGoopResult(ByteAngle Heading, UnsignedSixteenSixteen HorizontalSpeed, UnsignedSixteenSixteen VerticalSpeed);

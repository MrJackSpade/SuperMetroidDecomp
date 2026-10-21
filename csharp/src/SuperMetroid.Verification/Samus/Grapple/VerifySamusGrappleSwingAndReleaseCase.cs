namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The behavior, expected connection, expected cancellation and expected damage of one verify samus grapple swing and release case.</summary>
/// <param name="Behavior">The behavior.</param>
/// <param name="ExpectedConnection">The expected connection.</param>
/// <param name="ExpectedCancellation">The expected cancellation.</param>
/// <param name="ExpectedDamage">The expected damage.</param>
internal readonly record struct VerifySamusGrappleSwingAndReleaseCase(byte Behavior, bool ExpectedConnection, bool ExpectedCancellation, ushort ExpectedDamage);

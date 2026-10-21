using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>The counted, expected, expected behavior and expected movement of one verify yard surface loss case.</summary>
/// <param name="Counted">The counted.</param>
/// <param name="Expected">The expected.</param>
/// <param name="ExpectedBehavior">The expected behavior.</param>
/// <param name="ExpectedMovement">The expected movement.</param>
internal readonly record struct VerifyYardSurfaceLossCase(int Counted, int Expected, YardBehavior ExpectedBehavior, YardMovementFunction ExpectedMovement);

namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The behavior and expected damage of one samus grapple verify samus grapple swing and release case.</summary>
/// <param name="Behavior">The behavior.</param>
/// <param name="ExpectedDamage">The expected damage.</param>
internal readonly record struct SamusGrappleVerifySamusGrappleSwingAndReleaseCase(byte Behavior, ushort ExpectedDamage);

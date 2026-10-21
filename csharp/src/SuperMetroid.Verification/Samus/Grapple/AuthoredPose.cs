namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The running and direction of one authored pose.</summary>
/// <param name="Running">The running.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct AuthoredPose(bool Running, byte Direction);

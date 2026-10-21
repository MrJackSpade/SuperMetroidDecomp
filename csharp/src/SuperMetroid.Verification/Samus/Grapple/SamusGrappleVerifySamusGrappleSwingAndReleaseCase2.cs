namespace SuperMetroid.Verification.Samus.Grapple;

/// <summary>The collision word, expected cancellation and expected plms of one samus grapple verify samus grapple swing and release case2.</summary>
/// <param name="CollisionWord">The collision word.</param>
/// <param name="ExpectedCancellation">The expected cancellation.</param>
/// <param name="ExpectedPlms">The expected plms.</param>
internal readonly record struct SamusGrappleVerifySamusGrappleSwingAndReleaseCase2(ushort CollisionWord, bool ExpectedCancellation, int ExpectedPlms);

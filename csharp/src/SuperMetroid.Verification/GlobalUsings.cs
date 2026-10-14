// Assertions are verifier infrastructure, not members of the executable's partial Program
// type. A global static import keeps call sites compact across every domain-focused file.
global using static SuperMetroid.Verification.Harness.VerificationAssert;
global using SuperMetroid.Core.Audio;
// Typed spatial values (#628) appear throughout the verifier suites.
global using SuperMetroid.Core.Spatial;

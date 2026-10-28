using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Harness;

/// <summary>The first and second of one verify lookup stream2 kraid ramps case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyLookupStream2KraidRampsCase(string First, IReadOnlyList<RgbColor> Second);

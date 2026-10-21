namespace SuperMetroid.Verification.Bosses;

/// <summary>The origin y, offset and expected y of one verify boss display clipping case.</summary>
/// <param name="OriginY">The origin y.</param>
/// <param name="Offset">The offset.</param>
/// <param name="ExpectedY">The expected y.</param>
internal readonly record struct VerifyBossDisplayClippingCase(ushort OriginY, int Offset, byte ExpectedY);

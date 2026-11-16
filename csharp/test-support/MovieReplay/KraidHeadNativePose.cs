namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>One native Kraid head pose's mouth shapes, decoded from the cartridge for the mouth-read oracle (#2213).</summary>
/// <param name="Pointer">The pose's bank-$A7 address.</param>
/// <param name="OuterShape">The pose's outer (vulnerable) mouth-shape address.</param>
/// <param name="InnerShape">The pose's inner (invulnerable) mouth-shape address, $FFFF for none.</param>
internal readonly record struct KraidHeadNativePose(ushort Pointer, ushort OuterShape, ushort InnerShape);

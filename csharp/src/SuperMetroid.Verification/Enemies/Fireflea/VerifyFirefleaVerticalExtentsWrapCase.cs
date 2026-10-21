namespace SuperMetroid.Verification.Enemies.Fireflea;

/// <summary>The y and expected speed table index of one verify fireflea vertical extents wrap case.</summary>
/// <param name="Y">The y.</param>
/// <param name="ExpectedSpeedTableIndex">The expected speed table index.</param>
internal readonly record struct VerifyFirefleaVerticalExtentsWrapCase(ushort Y, int ExpectedSpeedTableIndex);

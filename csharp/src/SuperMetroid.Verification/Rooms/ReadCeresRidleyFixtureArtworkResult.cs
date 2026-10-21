namespace SuperMetroid.Verification.Rooms;

/// <summary>The palette words and planar of one read ceres ridley fixture artwork result.</summary>
/// <param name="PaletteWords">The palette words.</param>
/// <param name="Planar">The planar.</param>
internal readonly record struct ReadCeresRidleyFixtureArtworkResult(ushort[] PaletteWords, byte[] Planar);

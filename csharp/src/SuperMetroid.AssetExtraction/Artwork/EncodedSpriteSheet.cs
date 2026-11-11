namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One sheet's installed files, encoded and ready to write (#1490, #1633).</summary>
/// <param name="Image">The RGBA PNG of its pixels.</param>
/// <param name="Frames">Its frame manifest.</param>
/// <param name="RecolourRegions">Its recolour-region map (<see cref="RecolourRegionMapFile"/>), or null for a sheet that has none.</param>
/// <param name="BlendMap">Its blend map (<see cref="BlendMapFile"/>), or null for a sheet that has none.</param>
internal sealed record EncodedSpriteSheet(byte[] Image, byte[] Frames, byte[]? RecolourRegions, byte[]? BlendMap);

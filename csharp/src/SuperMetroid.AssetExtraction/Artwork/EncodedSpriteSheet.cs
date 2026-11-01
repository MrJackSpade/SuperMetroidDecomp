namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>One sheet's three installed files, encoded and ready to write (#1490).</summary>
/// <param name="Image">The RGBA PNG of its pixels.</param>
/// <param name="Palette">The PNG of its palette indices, priorities and part colour groups.</param>
/// <param name="Frames">Its frame manifest.</param>
internal sealed record EncodedSpriteSheet(byte[] Image, byte[] Palette, byte[] Frames);

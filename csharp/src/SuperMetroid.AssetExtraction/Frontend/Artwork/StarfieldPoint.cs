using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Artwork;

/// <summary>One star of a cinematic star field: where it is drawn and its tile.</summary>
/// <param name="Offset">The star's offset from the sprite origin: a signed X and the spritemap's unsigned Y byte.</param>
/// <param name="Tile">The star's tile.</param>
internal readonly record struct StarfieldPoint(Offset16<SpritePixels> Offset, int Tile);

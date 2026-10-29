using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>The tiles one HUD image is pictured from (#1466): its cells, row by row, across its size.</summary>
/// <param name="Image">The image.</param>
/// <param name="Size">Its size in HUD cells.</param>
/// <param name="Tiles">The tile each of its cells shows, row by row.</param>
internal sealed record HudImageTiles(HudImage Image, Extent16<TilemapCells> Size, ImmutableArray<BackgroundTile> Tiles);

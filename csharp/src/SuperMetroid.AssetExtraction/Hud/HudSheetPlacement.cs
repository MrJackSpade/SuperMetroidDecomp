using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>Where a HUD image is pictured on a sheet of the HUD's images (#1466).</summary>
/// <param name="Image">The image and the tiles it is pictured from.</param>
/// <param name="Shelf">The row of images it sits on.</param>
/// <param name="Cell">Its upper-left cell, from the left of its shelf's top row.</param>
internal sealed record HudSheetPlacement(HudImageTiles Image, int Shelf, Position16<TilemapCells> Cell);

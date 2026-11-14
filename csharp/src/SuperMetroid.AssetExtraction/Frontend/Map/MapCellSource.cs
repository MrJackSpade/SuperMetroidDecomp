using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// Where the cartridge's area maps draw a cell from (#1513): one of the map characters, mirrored as the map word lays it.
/// </summary>
/// <param name="Character">The map character the cell is drawn from.</param>
/// <param name="Mirroring">How the map word mirrors the character.</param>
internal readonly record struct MapCellSource(int Character, PictureFlip Mirroring);

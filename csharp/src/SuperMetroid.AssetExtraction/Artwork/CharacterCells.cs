using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Cells of a grid of eight-by-eight characters: the tilemaps the import decodes from the cartridge and the editable tile
/// documents it writes. The import turns a cell into the pixel the game places a picture at where it decodes it
/// (<see cref="CharacterCellConversions"/>); nothing the game reads holds a cell.
/// </summary>
public sealed class CharacterCells : ISpatialSpace;

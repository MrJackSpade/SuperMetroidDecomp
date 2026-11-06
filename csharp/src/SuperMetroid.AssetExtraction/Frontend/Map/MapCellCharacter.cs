using SuperMetroid.Core.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// One way the cartridge's area maps draw a cell (#1513): the map character and mirroring a map word names, and the cell
/// it pictures.
/// </summary>
/// <param name="Source">The character and mirroring the map word names.</param>
/// <param name="Cell">The cell the character, so mirrored, pictures.</param>
internal sealed record MapCellCharacter(MapCellSource Source, MapCell Cell);

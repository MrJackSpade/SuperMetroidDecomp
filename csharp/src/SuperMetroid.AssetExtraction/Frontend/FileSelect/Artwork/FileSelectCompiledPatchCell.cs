using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>A tilemap cell positioned relative to a dynamic file-select field.</summary>
/// <param name="Offset">Cell position relative to the field's anchor.</param>
/// <param name="Tile">The tile, colour variant, priority and mirroring drawn at that position.</param>
internal readonly record struct FileSelectCompiledPatchCell(Offset16<TilemapCells> Offset, BackgroundTile Tile);

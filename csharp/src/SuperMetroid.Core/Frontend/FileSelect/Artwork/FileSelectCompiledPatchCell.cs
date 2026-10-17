namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>A tilemap cell positioned relative to a dynamic file-select field.</summary>
/// <param name="Offset">Cell position relative to the field's anchor.</param>
/// <param name="Word">Tile, palette, priority and flips drawn at that position.</param>
internal readonly record struct FileSelectCompiledPatchCell(Offset16<TilemapCells> Offset, SnesBgTilemapWord Word);

namespace SuperMetroid.Core.Assets;

/// <summary>A native tilemap word positioned relative to a dynamic file-select field.</summary>
internal readonly record struct FileSelectCompiledPatchCell(Offset16<TilemapCells> Offset, ushort Word);

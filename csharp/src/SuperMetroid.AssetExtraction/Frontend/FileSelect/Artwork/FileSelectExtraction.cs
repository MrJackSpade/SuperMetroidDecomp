namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>
/// The two file-select documents the extraction writes (#1638): the layout the game binds, which is installed, and the
/// artwork the import composes the file-select pictures from, which is import data.
/// </summary>
/// <param name="Layout">UTF-8 JSON of the <see cref="FileSelectLayoutDocument"/>.</param>
/// <param name="Artwork">UTF-8 JSON of the <see cref="FileSelectArtworkDocument"/>.</param>
public sealed record FileSelectExtraction(byte[] Layout, byte[] Artwork);

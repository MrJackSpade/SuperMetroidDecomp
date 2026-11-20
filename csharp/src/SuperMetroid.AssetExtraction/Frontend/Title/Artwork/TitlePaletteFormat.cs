namespace SuperMetroid.AssetExtraction.Frontend.Title.Artwork;

/// <summary>File identity and schema revision of the import's title colours document: the title's paint and its selected ambient animation colours.</summary>
public static class TitlePaletteFormat
{
    /// <summary>JSON filename of the initial, ambient and skip-only colours the import reads to pre-colour the title sheets and record the backdrop.</summary>
    public const string FileName = "title-palette.json";
    /// <summary>Revision 3 of the schema, requiring all 256 initial colors, both ambient frame sets and both copyright replacements.</summary>
    public const int Version = 3;
}

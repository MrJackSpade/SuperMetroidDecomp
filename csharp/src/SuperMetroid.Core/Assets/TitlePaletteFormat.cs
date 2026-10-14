namespace SuperMetroid.Core.Assets;

/// <summary>Installed file identity and schema revision for title palette paint and its selected ambient animation colors.</summary>
public static class TitlePaletteFormat
{
    /// <summary>JSON filename loaded by the title presentation catalog for initial, ambient and skip-only colors.</summary>
    public const string FileName = "title-palette.json";
    /// <summary>Revision 3 of the schema, requiring all 256 initial colors, both ambient frame sets and both copyright replacements.</summary>
    public const int Version = 3;
}

namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Published filename and schema dimensions for the expanded title fixed-color gradient, independently editable without native HDMA pointers.</summary>
public static class TitleGradientFormat
{
    /// <summary>Installed JSON filename selected by the title presentation catalog.</summary>
    public const string FileName = "title-gradient.json";
    /// <summary>Revision 1 of the sixteen-variant, per-scanline RGB5/control-byte schema.</summary>
    public const int Version = 1;
    /// <summary>Sixteen variants covering every value of the native zoom word's bits 4 through 7.</summary>
    public const int VariantCount = 16;
}

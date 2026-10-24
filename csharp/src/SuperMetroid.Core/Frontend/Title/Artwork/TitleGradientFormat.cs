namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Published filename and schema dimensions for the expanded title fixed-color gradient, independently editable without native HDMA pointers.</summary>
public static class TitleGradientFormat
{
    /// <summary>Installed JSON filename selected by the title presentation catalog.</summary>
    public const string FileName = "title-gradient.json";
    /// <summary>
    /// Revision 2 of the sixteen-variant, per-scanline schema: each line's RGB5 colour plus the layers it blends with and
    /// whether it subtracts. Revision 1 stored the blend as a control byte.
    /// </summary>
    public const int Version = 2;
    /// <summary>Sixteen variants covering every value of the native zoom word's bits 4 through 7.</summary>
    public const int VariantCount = 16;
}

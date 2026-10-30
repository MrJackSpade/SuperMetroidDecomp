namespace SuperMetroid.Core.Frontend.Title.Artwork;

/// <summary>Published filename and schema dimensions for the title's gradient, editable as stops.</summary>
public static class TitleGradientFormat
{
    /// <summary>Installed JSON filename selected by the title presentation catalog.</summary>
    public const string FileName = "title-gradient.json";
    /// <summary>
    /// Revision 3 of the sixteen-variant schema (#1468): each variant's stops, each the line its band begins on, its RGB5
    /// colour, the layers it blends with and whether it subtracts. Revision 2 stored every screen line; revision 1 also
    /// stored the blend as a control byte.
    /// </summary>
    public const int Version = 3;
    /// <summary>Sixteen variants, one for each zoom bucket the title selects from.</summary>
    public const int VariantCount = 16;
}

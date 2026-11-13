namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>The installed escape-warning program document as the import reads and writes it (#1502).</summary>
public static class EscapeTypewriterDocumentDefinitions
{
    /// <summary>
    /// The supported editable escape-typewriter JSON schema version: revision 3 places each line by the pixel of its layer
    /// its first glyph shows at (#1601); revision 4 holds the warnings under <c>messages</c> (#1712).
    /// </summary>
    public const int Version = 4;

    /// <summary>The embedded editable escape-warning program asset file name.</summary>
    public const string FileName = "escape-typewriter.json";

    /// <summary>Maximum authored warning length, in characters.</summary>
    public const int MaximumLineLength = 32;

    /// <summary>The width of the layer a warning is typed over, in pixels: two screens side by side.</summary>
    public const int LayerWidth = 512;

    /// <summary>The height of the layer a warning is typed over, in pixels: one screen.</summary>
    public const int LayerHeight = 256;

    /// <summary>The height of a typed glyph, in pixels.</summary>
    public const int GlyphHeight = 8;
}

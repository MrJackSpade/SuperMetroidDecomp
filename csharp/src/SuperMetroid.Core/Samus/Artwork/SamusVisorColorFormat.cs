using SuperMetroid.Core.Samus.Palettes;


namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Installed-resource identity, schema revision, and native six-word extent for editable Samus visor colors.</summary>
public static class SamusVisorColorFormat
{
    /// <summary>JSON resource filename containing the six editable visor RGB5 colors.</summary>
    public const string FileName = "samus-visor-colors.json";
    /// <summary>Supported schema revision, one, requiring exactly the six authored visor words.</summary>
    public const int Version = 1;
    /// <summary>The six authored BGR555 words at $9B:A3C0, including X-ray widening and room-cycle colors.</summary>
    public const int SourceAddress = SamusPaletteRomData.Visor.Colors;
    /// <summary>Six selected color words, corresponding to valid native even byte offsets 0, 2, 4, 6, 8, and 10.</summary>
    public const int ColorCount = 6;
}

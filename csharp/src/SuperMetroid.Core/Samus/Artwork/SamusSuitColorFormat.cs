using SuperMetroid.Core.Samus.Palettes;


namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Defines the installed normal-suit color resource and its fixed palette size.</summary>
public static class SamusSuitColorFormat
{
    /// <summary>Canonical normal-suit color asset file name.</summary>
    public const string FileName = "samus-suit-colors.json";
    /// <summary>Supported normal-suit color schema version.</summary>
    public const int Version = 1;
    /// <summary>Number of OBJ palette colors stored for each suit, including the transparent slot.</summary>
    public const int ColorsPerSuit = SamusPaletteDefinitions.Common.ColorsPerObjPalette;
}

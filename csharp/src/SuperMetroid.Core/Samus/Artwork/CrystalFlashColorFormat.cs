using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Resource identity, supported JSON version, and fixed dimensions derived from the native Crystal Flash palette programs.</summary>
public static class CrystalFlashColorFormat
{
    /// <summary>Installed presentation resource name, <c>crystal-flash-colors.json</c>.</summary>
    public const string FileName = "crystal-flash-colors.json";
    /// <summary>Supported JSON schema revision one, checked before palette compilation.</summary>
    public const int Version = 1;
    /// <summary>Ten editable body frames, one per pointer/timer record at $91:DC00-$DC27 in native playback order.</summary>
    public const int BodyFrameCount = SamusPaletteDefinitions.CrystalFlash.BodyRecordCount;
    /// <summary>Six editable bubble frames selected independently by the pointer table at $91:DC28-$DC33.</summary>
    public const int BubbleFrameCount = SamusPaletteDefinitions.CrystalFlash.BubblePaletteCount;
    /// <summary>Ten colors in each body frame, occupying sprite palette six colors 0..9 at CGRAM $E0-$E9.</summary>
    public const int BodyColorCount = SamusPaletteDefinitions.CrystalFlash.BodyColorCount;
    /// <summary>Six colors in each bubble frame, occupying sprite palette six colors $A..$F at CGRAM $EA-$EF.</summary>
    public const int BubbleColorCount = SamusPaletteDefinitions.CrystalFlash.BubbleColorCount;
}

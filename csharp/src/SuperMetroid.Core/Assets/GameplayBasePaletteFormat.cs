using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Defines gameplay base-palette files, native sources, and fixed dimensions.</summary>
public static class GameplayBasePaletteFormat
{
    /// <summary>Supported gameplay base-palette schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing the initial and common-sprite colors.</summary>
    public const string ArtworkFileName = "gameplay-base-palettes.json";
    /// <summary>Manifest filename selecting the installed gameplay base-palette asset.</summary>
    public const string ManifestFileName = "gameplay-base-palettes-manifest.json";
    /// <summary>Number of colors in a complete SNES OBJ palette row.</summary>
    public const int SpriteColorCount = 16;
    /// <summary>Complete starting CGRAM image at $9A:8000.</summary>
    public const int InitialSourceAddress = GameplayBasePaletteRomData.InitialPalette;
    /// <summary>Shared gameplay OBJ palette at $9A:FC00.</summary>
    public const int CommonSpriteSourceAddress = GameplayBasePaletteRomData.CommonSpritePalette;
    /// <summary>Enemy-projectile OBJ colors begin at initial CGRAM index 208 ($9A:81A0).</summary>
    public const int EnemyProjectileInitialColor = 208;
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}

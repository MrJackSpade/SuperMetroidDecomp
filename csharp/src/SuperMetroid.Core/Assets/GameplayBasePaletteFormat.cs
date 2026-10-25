using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Defines the gameplay base-palette files and their fixed dimensions.</summary>
public static class GameplayBasePaletteFormat
{
    /// <summary>Supported gameplay base-palette schema revision.</summary>
    public const int Version = 1;
    /// <summary>JSON filename containing the initial and common-sprite colors.</summary>
    public const string ArtworkFileName = "gameplay-base-palettes.json";
    /// <summary>Manifest filename selecting the installed gameplay base-palette asset.</summary>
    public const string ManifestFileName = "gameplay-base-palettes-manifest.json";
    /// <summary>Number of colors in one complete sprite palette.</summary>
    public const int SpriteColorCount = 16;
    /// <summary>The enemy-projectile sprite colours begin at colour 208 of the starting image.</summary>
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

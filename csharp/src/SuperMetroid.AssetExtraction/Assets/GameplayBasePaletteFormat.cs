using System.Text.Json;

namespace SuperMetroid.AssetExtraction.Assets;

/// <summary>Defines the gameplay base-palette files and their fixed dimensions.</summary>
public static class GameplayBasePaletteFormat
{
    /// <summary>Supported gameplay base-palette schema revision.</summary>
    public const int Version = 1;
    /// <summary>Import-data JSON filename containing the initial and common-sprite colors (#1637).</summary>
    public const string ArtworkFileName = "gameplay-base-palettes.json";
    /// <summary>Number of colors in one complete sprite palette.</summary>
    public const int SpriteColorCount = 16;
    /// <summary>Shared camel-case, case-insensitive JSON options that reject unknown fields.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}

using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable starting CGRAM image and common sprite colors restored on room entry.</summary>
public sealed class GameplayBasePaletteCatalog
{
    /// <summary>Compiled 256-color initial CGRAM image used when a room is loaded.</summary>
    private readonly ushort[] initial;
    /// <summary>Compiled shared 16-color OBJ palette used by gameplay sprites.</summary>
    private readonly ushort[] commonSprites;

    /// <summary>Stores the validated, packed RGB5 values used by room and sprite palette loading.</summary>
    /// <param name="initial">Packed colors for the full starting CGRAM image.</param>
    /// <param name="commonSprites">Packed colors for the shared sprite palette.</param>
    private GameplayBasePaletteCatalog(ushort[] initial, ushort[] commonSprites)
    {
        this.initial = initial;
        this.commonSprites = commonSprites;
    }

    /// <summary>Identity of the decoded selected colors, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(GameplayBasePaletteCatalog), content =>
    {
        content.AppendWords("initial", initial);
        content.AppendWords("common sprites", commonSprites);
    });

    /// <summary>Loads the complete selected 256-color starting image into CGRAM.</summary>
    public void LoadInitial(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < initial.Length; color++)
            cgram.SetColor(color, initial[color]);
    }

    /// <summary>Loads the selected 16-color common-sprite palette at a CGRAM color index.</summary>
    public void LoadCommonSprites(SnesCgram cgram, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < commonSprites.Length; color++)
            cgram.SetColor(destination + color, commonSprites[color]);
    }

    /// <summary>Copies the 16 enemy-projectile colors from the initial image to a CGRAM color index.</summary>
    public void LoadEnemyProjectileSprites(SnesCgram cgram, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < GameplayBasePaletteFormat.SpriteColorCount; color++)
            cgram.SetColor(destination + color,
                initial[GameplayBasePaletteFormat.EnemyProjectileInitialColor + color]);
    }

    /// <summary>Loads and validates the full initial and common-sprite RGB5 palettes.</summary>
    public static GameplayBasePaletteCatalog Load(Stream json)
    {
        GameplayBasePaletteDocument document = JsonAssetDocument.Read<GameplayBasePaletteDocument>(
            json, GameplayBasePaletteFormat.JsonOptions, "gameplay base palette");
        if (document.Version != GameplayBasePaletteFormat.Version)
            throw new InvalidDataException("Gameplay base palette has an unsupported version.");
        return new GameplayBasePaletteCatalog(
            Compile(document.Initial, SnesCgram.ColorCount, "initial"),
            Compile(document.CommonSprites, GameplayBasePaletteFormat.SpriteColorCount,
                "commonSprites"));
    }

    /// <summary>Validates and serializes a gameplay base-palette document as JSON.</summary>
    public static byte[] Write(GameplayBasePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            GameplayBasePaletteFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Validates RGB5 components and packs a palette into SNES color words.</summary>
    /// <param name="source">JSON palette entries to validate and compile.</param>
    /// <param name="expected">Required number of entries.</param>
    /// <param name="name">Palette label used in validation errors.</param>
    /// <returns>Packed RGB5 color words in source order.</returns>
    private static ushort[] Compile(PaletteRgb5[]? source, int expected, string name)
    {
        if (source is null || source.Length != expected)
            throw new InvalidDataException($"Gameplay {name} palette requires {expected} RGB5 colors.");
        var result = new ushort[expected];
        for (int color = 0; color < expected; color++)
        {
            PaletteRgb5 entry = source[color] ??
                throw new InvalidDataException($"Gameplay {name} color {color} is null.");
            if ((uint)entry.Red > 31 || (uint)entry.Green > 31 || (uint)entry.Blue > 31)
                throw new InvalidDataException($"Gameplay {name} color {color} exceeds RGB5.");
            result[color] = (ushort)(entry.Red | entry.Green << 5 | entry.Blue << 10);
        }
        return result;
    }
}

/// <summary>Defines the editable gameplay starting and common-sprite RGB5 colors.</summary>
/// <param name="Version">Document schema revision.</param>
/// <param name="Initial">Complete 256-color initial CGRAM image.</param>
/// <param name="CommonSprites">Sixteen shared gameplay OBJ colors.</param>
public sealed record GameplayBasePaletteDocument(int Version, PaletteRgb5[] Initial,
    PaletteRgb5[] CommonSprites);

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
    public const int InitialSourceAddress = 0x9a8000;
    /// <summary>Shared gameplay OBJ palette at $9A:FC00.</summary>
    public const int CommonSpriteSourceAddress = 0x9afc00;
    /// <summary>Enemy-projectile OBJ colors begin at initial CGRAM index 208 ($9A:81A0).</summary>
    public const int EnemyProjectileInitialColor = 208;
    /// <summary>Gets the shared camel-case, case-insensitive, strict JSON serialization options.</summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow,
    };
}

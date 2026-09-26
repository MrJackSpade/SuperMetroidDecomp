using System.Text.Json;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable starting CGRAM image and common sprite colors restored on room entry.</summary>
public sealed class GameplayBasePaletteCatalog
{
    private readonly ushort[] initial;
    private readonly ushort[] commonSprites;

    private GameplayBasePaletteCatalog(ushort[] initial, ushort[] commonSprites)
    {
        this.initial = initial;
        this.commonSprites = commonSprites;
    }

    public ReadOnlySpan<ushort> Initial => initial;
    public ReadOnlySpan<ushort> CommonSprites => commonSprites;

    public void LoadInitial(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < initial.Length; color++)
            cgram.SetColor(color, initial[color]);
    }

    public void LoadCommonSprites(SnesCgram cgram, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < commonSprites.Length; color++)
            cgram.SetColor(destination + color, commonSprites[color]);
    }

    public void LoadEnemyProjectileSprites(SnesCgram cgram, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < GameplayBasePaletteFormat.SpriteColorCount; color++)
            cgram.SetColor(destination + color,
                initial[GameplayBasePaletteFormat.EnemyProjectileInitialColor + color]);
    }

    public static GameplayBasePaletteCatalog Load(Stream json)
    {
        GameplayBasePaletteDocument document;
        try
        {
            document = JsonSerializer.Deserialize<GameplayBasePaletteDocument>(json,
                GameplayBasePaletteFormat.JsonOptions) ??
                throw new InvalidDataException("Gameplay base palette JSON is empty.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid gameplay base palette JSON.", error);
        }
        if (document.Version != GameplayBasePaletteFormat.Version)
            throw new InvalidDataException("Gameplay base palette has an unsupported version.");
        return new GameplayBasePaletteCatalog(
            Compile(document.Initial, SnesCgram.ColorCount, "initial"),
            Compile(document.CommonSprites, GameplayBasePaletteFormat.SpriteColorCount,
                "commonSprites"));
    }

    public static byte[] Write(GameplayBasePaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            GameplayBasePaletteFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

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

public sealed record GameplayBasePaletteDocument(int Version, PaletteRgb5[] Initial,
    PaletteRgb5[] CommonSprites);

public static class GameplayBasePaletteFormat
{
    public const int Version = 1;
    public const string ArtworkFileName = "gameplay-base-palettes.json";
    public const string ManifestFileName = "gameplay-base-palettes-manifest.json";
    public const int SpriteColorCount = 16;
    /// <summary>Complete starting CGRAM image at $9A:8000.</summary>
    public const int InitialSourceAddress = 0x9a8000;
    /// <summary>Shared gameplay OBJ palette at $9A:FC00.</summary>
    public const int CommonSpriteSourceAddress = 0x9afc00;
    /// <summary>Enemy-projectile OBJ colors begin at initial CGRAM index 208 ($9A:81A0).</summary>
    public const int EnemyProjectileInitialColor = 208;
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}

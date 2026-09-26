using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors.</summary>
public sealed class CeresRidleyColorCatalog
{
    private readonly ushort[] start;
    private readonly ushort[][] eyeFade;
    private readonly ushort[][] bodyFade;
    private readonly ushort[][] health;
    private readonly ushort[] retreatBg;
    private readonly ushort[] retreatShared;
    private readonly ushort[][] baby;

    private CeresRidleyColorCatalog(ushort[] start, ushort[][] eyeFade,
        ushort[][] bodyFade, ushort[][] health, ushort[] retreatBg, ushort[] retreatShared,
        ushort[][] baby)
    {
        this.start = start;
        this.eyeFade = eyeFade;
        this.bodyFade = bodyFade;
        this.health = health;
        this.retreatBg = retreatBg;
        this.retreatShared = retreatShared;
        this.baby = baby;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    public ushort ResolveStart(int color) => Get(start, color);
    public ushort ResolveEyeFade(int row, int color) => Get(eyeFade, row, color);
    public ushort ResolveBodyFade(int row, int color) => Get(bodyFade, row, color);
    public ushort ResolveHealth(int row, int color) => Get(health, row, color);
    public ushort ResolveRetreatBg(int color) => Get(retreatBg, color);
    public ushort ResolveRetreatShared(int color) => Get(retreatShared, color);
    public ushort ResolveBaby(int row, int color) => Get(baby, row, color);

    public void ApplyStart(SnesCgram cgram) =>
        Apply(cgram, start, CeresRidleyPaletteRomData.StartCgramIndex);

    public void ApplyEyeFade(SnesCgram cgram, int row) =>
        Apply(cgram, Get(eyeFade, row), CeresRidleyPaletteRomData.EyeFadeCgramIndex);

    public void ApplyBodyFade(SnesCgram cgram, int row)
    {
        ushort[] colors = Get(bodyFade, row);
        Apply(cgram, colors, CeresRidleyPaletteRomData.BodyFadeBgCgramIndex);
        Apply(cgram, colors, CeresRidleyPaletteRomData.BodyFadeObjCgramIndex);
    }

    public void ApplyHealth(SnesCgram cgram, int row) =>
        Apply(cgram, Get(health, row), CeresRidleyPaletteRomData.HealthCgramIndex);

    public void ApplyRetreat(SnesCgram cgram)
    {
        Apply(cgram, retreatBg, CeresRidleyPaletteRomData.RetreatBgCgramIndex);
        Apply(cgram, retreatShared, CeresRidleyPaletteRomData.RetreatSharedBgCgramIndex);
        Apply(cgram, retreatShared, CeresRidleyPaletteRomData.RetreatSharedObjCgramIndex);
    }

    public void ApplyBaby(SnesCgram cgram, int row) =>
        Apply(cgram, Get(baby, row), CeresRidleyPaletteRomData.BabyCgramIndex);

    public static CeresRidleyColorCatalog Load(Stream json,
        CeresRidleyColorCatalog? stockForLegacyOverride = null)
    {
        ArgumentNullException.ThrowIfNull(json);
        CeresRidleyColorDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<CeresRidleyColorDocument>(JsonOptions)
                ?? throw new InvalidDataException("Ceres Ridley color JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Ceres Ridley color JSON.", error);
        }
        if (document.Version != CeresRidleyColorFormat.Version &&
            !(document.Version == CeresRidleyColorFormat.PreBabyVersion &&
              stockForLegacyOverride is not null))
            throw new InvalidDataException("Ceres Ridley colors require the supported version.");
        return new(Compile(document.Start, CeresRidleyPaletteRomData.StartColorCount, "start"),
            CompileRows(document.EyeFade, CeresRidleyPaletteRomData.EyeFadeRowCount,
                CeresRidleyPaletteRomData.EyeFadeColorCount, "eye fade"),
            CompileRows(document.BodyFade, CeresRidleyPaletteRomData.BodyFadeRowCount,
                CeresRidleyPaletteRomData.BodyFadeColorCount, "body fade"),
            CompileRows(document.Health, CeresRidleyPaletteRomData.HealthRowCount,
                CeresRidleyPaletteRomData.HealthColorCount, "health"),
            Compile(document.RetreatBg, CeresRidleyPaletteRomData.RetreatBgColorCount, "retreat BG"),
            Compile(document.RetreatShared, CeresRidleyPaletteRomData.RetreatSharedColorCount,
                "retreat shared"),
            document.Version == CeresRidleyColorFormat.PreBabyVersion
                ? stockForLegacyOverride!.baby
                : CompileRows(document.Baby, CeresRidleyPaletteRomData.BabyRowCount,
                    CeresRidleyPaletteRomData.BabyColorCount, "Baby"));
    }

    public static byte[] Write(CeresRidleyColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static ushort Get(ushort[] colors, int color)
    {
        if ((uint)color >= colors.Length) throw new ArgumentOutOfRangeException(nameof(color));
        return colors[color];
    }

    private static ushort[] Get(ushort[][] rows, int row)
    {
        if ((uint)row >= rows.Length) throw new ArgumentOutOfRangeException(nameof(row));
        return rows[row];
    }

    private static ushort Get(ushort[][] rows, int row, int color) =>
        Get(Get(rows, row), color);

    private static void Apply(SnesCgram cgram, ushort[] colors, int destination)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < colors.Length; color++)
            cgram.SetColor(destination + color, colors[color]);
    }

    private static ushort[][] CompileRows(PaletteRgb5[][]? rows, int count,
        int colorsPerRow, string name)
    {
        if (rows is null || rows.Length != count)
            throw new InvalidDataException($"Ceres Ridley {name} requires {count} rows.");
        var compiled = new ushort[count][];
        for (int row = 0; row < count; row++)
            compiled[row] = Compile(rows[row], colorsPerRow, $"{name} row {row}");
        return compiled;
    }

    private static ushort[] Compile(PaletteRgb5[]? colors, int count, string name)
    {
        if (colors is null || colors.Length != count)
            throw new InvalidDataException($"Ceres Ridley {name} requires {count} colors.");
        var compiled = new ushort[count];
        for (int color = 0; color < count; color++)
        {
            PaletteRgb5? rgb = colors[color];
            if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                (uint)rgb.Blue > 31)
                throw new InvalidDataException($"Ceres Ridley {name} color {color} requires RGB5 channels 0..31.");
            compiled[color] = (ushort)(rgb.Red | rgb.Green << 5 | rgb.Blue << 10);
        }
        return compiled;
    }

    private static void RejectDuplicates(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (JsonProperty property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                    throw new InvalidDataException($"Duplicate Ceres Ridley color property {property.Name}.");
                RejectDuplicates(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (JsonElement child in value.EnumerateArray()) RejectDuplicates(child);
    }
}

public sealed record CeresRidleyColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Start { get; init; }
    public required PaletteRgb5[][] EyeFade { get; init; }
    public required PaletteRgb5[][] BodyFade { get; init; }
    public required PaletteRgb5[][] Health { get; init; }
    public required PaletteRgb5[] RetreatBg { get; init; }
    public required PaletteRgb5[] RetreatShared { get; init; }
    public PaletteRgb5[][]? Baby { get; init; }
}

public static class CeresRidleyColorFormat
{
    public const string FileName = "ceres-ridley-colors.json";
    public const int Version = 2;
    public const int PreBabyVersion = 1;
}

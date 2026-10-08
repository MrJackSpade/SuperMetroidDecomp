using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors.</summary>
public sealed class CeresRidleyColorCatalog
{
    private readonly CeresRidleyStartColorDefinitions start;
    private readonly CeresRidleyFadeColorDefinitions eyeFade;
    private readonly CeresRidleyFadeColorDefinitions bodyFade;
    private readonly CeresRidleyHealthPaintDefinitions health;
    private readonly CeresRidleyAlarmColorDefinitions alarm;
    private readonly Dictionary<int, ushort> retreatBg = [];
    /// <summary>Only supplied differences from the first eight door paints: native $A6:AA01-AA10 repeats $A6:E171-E180.</summary>
    private readonly Dictionary<int, ushort> retreatShared = [];
    private readonly CeresBabyPaintDefinitions baby;

    private CeresRidleyColorCatalog(ushort[] start, CeresRidleyFadeColorDefinitions eyeFade,
        CeresRidleyFadeColorDefinitions bodyFade, ushort[][] health, CeresRidleyAlarmColorDefinitions alarm,
        ushort[] retreatBg, ushort[] retreatShared,
        CeresBabyPaintDefinitions baby)
    {
        this.start = new(start, baby);
        this.eyeFade = eyeFade;
        this.bodyFade = bodyFade;
        this.health = new(health, bodyFade, eyeFade);
        this.alarm = alarm;
        for (int color = 0; color < retreatBg.Length; color++)
            if (retreatBg[color] != CalculateRetreatBg(color)) this.retreatBg.Add(color, retreatBg[color]);
        // The retreat copies the normal door/container paints into both drawing domains.
        // Keep only independent supplied differences from the existing startup owner.
        for (int color = 0; color < retreatShared.Length; color++)
            if (retreatShared[color] != this.start.Resolve(color + 1))
                this.retreatShared.Add(color, retreatShared[color]);
        this.baby = baby;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    public ushort ResolveRetreatBg(int color)
    {
        if ((uint)color >= CeresRidleyPaletteRomData.RetreatBgColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatBg.TryGetValue(color, out ushort edited) ? edited : CalculateRetreatBg(color);
    }
    private ushort CalculateRetreatBg(int color) => color < 11 ? bodyFade.Resolve(1, color)
        : color < 14 ? eyeFade.Resolve(14, color - 11) : CeresRidleyMode7PaintDefinitions.RetreatNeutral;
    public ushort ResolveRetreatShared(int color)
    {
        if ((uint)color >= CeresRidleyPaletteRomData.RetreatSharedColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatShared.TryGetValue(color, out ushort edited) ? edited : start.Resolve(color + 1);
    }

    public void ApplyStart(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.StartColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.StartCgramIndex + color, start.Resolve(color));
    }

    public void ApplyEyeFade(SnesCgram cgram, int row)
    {
        eyeFade.ValidateRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.EyeFadeColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.EyeFadeCgramIndex + color, eyeFade.Resolve(row, color));
    }

    public void ApplyBodyFade(SnesCgram cgram, int row)
    {
        bodyFade.ValidateRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.BodyFadeColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.BodyFadeBgCgramIndex + color, bodyFade.Resolve(row, color));
        for (int color = 0; color < CeresRidleyPaletteRomData.BodyFadeColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.BodyFadeObjCgramIndex + color, bodyFade.Resolve(row, color));
    }
    public void ApplyHealth(SnesCgram cgram, int row)
    {
        _ = health.Resolve(row, 0);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.HealthColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.HealthCgramIndex + color, health.Resolve(row, color));
    }

    public void ApplyAlarm(SnesCgram cgram, int row)
    {
        _ = CeresRidleyAlarmColorDefinitions.SourceRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.AlarmColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.AlarmCgramIndex + color, alarm.Resolve(row, color));
    }

    public void ApplyRetreat(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.RetreatBgColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.RetreatBgCgramIndex + color, ResolveRetreatBg(color));
        for (int color = 0; color < CeresRidleyPaletteRomData.RetreatSharedColorCount; color++)
        {
            ushort selected = ResolveRetreatShared(color);
            cgram.SetColor(CeresRidleyPaletteRomData.RetreatSharedBgCgramIndex + color, selected);
            cgram.SetColor(CeresRidleyPaletteRomData.RetreatSharedObjCgramIndex + color, selected);
        }
    }

    public void ApplyBaby(SnesCgram cgram, int row)
    {
        _ = baby.Resolve(row, 0);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.BabyColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.BabyCgramIndex + color, baby.Resolve(row, color));
    }

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
            !(document.Version is CeresRidleyColorFormat.PreBabyVersion or
                CeresRidleyColorFormat.PreAlarmVersion &&
              stockForLegacyOverride is not null))
            throw new InvalidDataException("Ceres Ridley colors require the supported version.");
        var compiledBody = new CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind.Body, CompileRows(document.BodyFade, CeresRidleyPaletteRomData.BodyFadeRowCount,
                CeresRidleyPaletteRomData.BodyFadeColorCount, "body fade"));
        return new(Compile(document.Start, CeresRidleyPaletteRomData.StartColorCount, "start"),
            new CeresRidleyFadeColorDefinitions(CeresRidleyFadeKind.Eyes, CompileRows(document.EyeFade, CeresRidleyPaletteRomData.EyeFadeRowCount,
                CeresRidleyPaletteRomData.EyeFadeColorCount, "eye fade")),
            compiledBody,
            CompileRows(document.Health, CeresRidleyPaletteRomData.HealthRowCount,
                CeresRidleyPaletteRomData.HealthColorCount, "health"),
            document.Version < CeresRidleyColorFormat.Version
                ? stockForLegacyOverride!.alarm
                : new CeresRidleyAlarmColorDefinitions(CompileRows(document.Alarm, CeresRidleyPaletteRomData.AlarmRowCount,
                    CeresRidleyPaletteRomData.AlarmColorCount, "alarm")),
            Compile(document.RetreatBg, CeresRidleyPaletteRomData.RetreatBgColorCount, "retreat BG"),
            Compile(document.RetreatShared, CeresRidleyPaletteRomData.RetreatSharedColorCount,
                "retreat shared"),
            document.Version == CeresRidleyColorFormat.PreBabyVersion
                ? stockForLegacyOverride!.baby
                : new CeresBabyPaintDefinitions(CompileRows(document.Baby, CeresRidleyPaletteRomData.BabyRowCount,
                    CeresRidleyPaletteRomData.BabyColorCount, "Baby"), compiledBody));
    }

    public static byte[] Write(CeresRidleyColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
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

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Ceres Ridley color property {name}."));
}

public sealed record CeresRidleyColorDocument
{
    public required int Version { get; init; }
    public required PaletteRgb5[] Start { get; init; }
    public required PaletteRgb5[][] EyeFade { get; init; }
    public required PaletteRgb5[][] BodyFade { get; init; }
    public required PaletteRgb5[][] Health { get; init; }
    public PaletteRgb5[][]? Alarm { get; init; }
    public required PaletteRgb5[] RetreatBg { get; init; }
    public required PaletteRgb5[] RetreatShared { get; init; }
    public PaletteRgb5[][]? Baby { get; init; }
}

public static class CeresRidleyColorFormat
{
    public const string FileName = "ceres-ridley-colors.json";
    public const int Version = 3;
    public const int PreBabyVersion = 1;
    public const int PreAlarmVersion = 2;
}

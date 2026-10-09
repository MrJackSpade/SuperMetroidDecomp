using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable Ceres Ridley, shared Norfair Ridley health, and private Baby draw colors.</summary>
public sealed class CeresRidleyColorCatalog
{
    /// <summary>Validated startup palette words, including the shared door/container colors and Baby palette source.</summary>
    private readonly CeresRidleyStartColorDefinitions start;
    /// <summary>Validated three-color rows used while the eye fades from full paint to black.</summary>
    private readonly CeresRidleyFadeColorDefinitions eyeFade;
    /// <summary>Validated eleven-color body fade rows shared by BG and OBJ palette application.</summary>
    private readonly CeresRidleyFadeColorDefinitions bodyFade;
    /// <summary>Validated health palette rows shared by Ceres and Norfair Ridley health selection.</summary>
    private readonly CeresRidleyHealthPaintDefinitions health;
    /// <summary>Validated three-color phases for the self-destruct alarm text.</summary>
    private readonly CeresRidleyAlarmColorDefinitions alarm;
    /// <summary>Supplied retreat BG colors that differ from the body/eye/neutral composition.</summary>
    private readonly Dictionary<int, ushort> retreatBg = [];
    /// <summary>Only supplied differences from the first eight door paints: native $A6:AA01-AA10 repeats $A6:E171-E180.</summary>
    private readonly Dictionary<int, ushort> retreatShared = [];
    /// <summary>Validated private Baby/container draw palette rows.</summary>
    private readonly CeresBabyPaintDefinitions baby;

    /// <summary>Stores validated paint families and retains only retreat entries that need explicit overrides.</summary>
    /// <param name="start">Compiled startup palette words used for startup and shared retreat colors.</param>
    /// <param name="eyeFade">Compiled eye-fade rows.</param>
    /// <param name="bodyFade">Compiled body-fade rows.</param>
    /// <param name="health">Compiled shared health palette rows.</param>
    /// <param name="alarm">Compiled self-destruct alarm phases.</param>
    /// <param name="retreatBg">Supplied retreat BG colors, reduced to entries differing from their calculated composition.</param>
    /// <param name="retreatShared">Supplied retreat shared colors, reduced to entries differing from startup paint.</param>
    /// <param name="baby">Compiled Baby/container draw palette rows.</param>
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

    /// <summary>JSON policy shared by catalog loading and writing: camel-case names, strict unknown-property handling, and readable output.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };
    /// <summary>Returns one BGR555 retreat BG color, index 0..14, preserving supplied edits over the composition of body-fade row one, eye-fade row fourteen, and the neutral tail colors.</summary>
    public ushort ResolveRetreatBg(int color)
    {
        if ((uint)color >= CeresRidleyPaletteRomData.RetreatBgColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatBg.TryGetValue(color, out ushort edited) ? edited : CalculateRetreatBg(color);
    }
    /// <summary>Composes an unchanged retreat BG entry from body-fade paint, eye-fade paint, or the neutral Mode 7 tail.</summary>
    /// <param name="color">Zero-based index in the fifteen-color retreat BG sequence.</param>
    /// <returns>The calculated BGR555 color before any supplied edit is applied.</returns>
    private ushort CalculateRetreatBg(int color) => color < 11 ? bodyFade.Resolve(1, color)
        : color < 14 ? eyeFade.Resolve(14, color - 11) : CeresRidleyMode7PaintDefinitions.RetreatNeutral;
    /// <summary>Returns one BGR555 retreat shared color, index 0..7; unedited values reuse startup colors 1..8, matching native $A6:AA01 and $A6:E171.</summary>
    public ushort ResolveRetreatShared(int color)
    {
        if ((uint)color >= CeresRidleyPaletteRomData.RetreatSharedColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return retreatShared.TryGetValue(color, out ushort edited) ? edited : start.Resolve(color + 1);
    }

    /// <summary>Copies the 32 startup colors from native $A6:E16F into CGRAM entries 160..191, initializing the door/container and Baby OBJ palettes.</summary>
    public void ApplyStart(SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.StartColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.StartCgramIndex + color, start.Resolve(color));
    }

    /// <summary>Copies one eye-fade row, index 0..15, to CGRAM entries 252..254; native $A6:E2AA's three-color rows fade from full eye paint to black.</summary>
    public void ApplyEyeFade(SnesCgram cgram, int row)
    {
        eyeFade.ValidateRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.EyeFadeColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.EyeFadeCgramIndex + color, eyeFade.Resolve(row, color));
    }

    /// <summary>Copies one eleven-color body-fade row, index 0..15, to both BG CGRAM entries 145..155 and OBJ entries 241..251; native $A6:E30A progresses from black to full body paint.</summary>
    public void ApplyBodyFade(SnesCgram cgram, int row)
    {
        bodyFade.ValidateRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.BodyFadeColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.BodyFadeBgCgramIndex + color, bodyFade.Resolve(row, color));
        for (int color = 0; color < CeresRidleyPaletteRomData.BodyFadeColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.BodyFadeObjCgramIndex + color, bodyFade.Resolve(row, color));
    }
    /// <summary>Copies one fourteen-color health row, index 0..2, from the shared $A6:E46A palette family to CGRAM entries 241..254; encounter AI separately selects by Ceres shot count or Norfair health.</summary>
    public void ApplyHealth(SnesCgram cgram, int row)
    {
        _ = health.Resolve(row, 0);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.HealthColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.HealthCgramIndex + color, health.Resolve(row, color));
    }

    /// <summary>Copies one self-destruct EMERGENCY-text phase, index 0..15, to CGRAM entries 97..99, preserving the reflected three-color cycle at native $A6:C1DF.</summary>
    public void ApplyAlarm(SnesCgram cgram, int row)
    {
        _ = CeresRidleyAlarmColorDefinitions.SourceRow(row);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.AlarmColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.AlarmCgramIndex + color, alarm.Resolve(row, color));
    }

    /// <summary>Installs the retreat's fifteen BG colors at CGRAM 81..95 and eight shared colors at both BG 33..40 and OBJ 241..248, matching $A6:A9E3/$AA01.</summary>
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

    /// <summary>Copies one private Baby/container draw shade, index 0..3, from native $A6:E1F1 to OBJ palette-three entries 177..191; row selection and pose timing remain owned by the Baby draw callback.</summary>
    public void ApplyBaby(SnesCgram cgram, int row)
    {
        _ = baby.Resolve(row, 0);
        ArgumentNullException.ThrowIfNull(cgram);
        for (int color = 0; color < CeresRidleyPaletteRomData.BabyColorCount; color++)
            cgram.SetColor(CeresRidleyPaletteRomData.BabyCgramIndex + color, baby.Resolve(row, color));
    }

    /// <summary>Compiles RGB5 JSON into a validated immutable color catalog, rejecting unknown/duplicate properties, unsupported versions, wrong row dimensions, and channels outside 0..31.</summary>
    /// <param name="json">JSON input read from its current position; the caller retains ownership of the stream.</param>
    /// <param name="stockForLegacyOverride">Optional stock catalog required for versions one and two: supplies missing alarm colors and, for version one, missing Baby colors.</param>
    /// <returns>A catalog retaining independent supplied color edits while calculating unchanged shared paint and fade relationships.</returns>
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

    /// <summary>Serializes a complete current-version color document as indented camel-case UTF-8 JSON, then reloads it to enforce the same schema, dimension, and RGB5 validation as asset loading.</summary>
    public static byte[] Write(CeresRidleyColorDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Validates the required number of RGB5 rows and compiles each row to BGR555 words.</summary>
    /// <param name="rows">Optional source rows from the JSON document.</param>
    /// <param name="count">Required number of rows.</param>
    /// <param name="colorsPerRow">Required color count in each row.</param>
    /// <param name="name">Palette family label included in validation errors.</param>
    /// <returns>Compiled rows with each RGB5 color packed into a BGR555 word.</returns>
    /// <exception cref="InvalidDataException">The row array is missing or has the wrong row or color dimensions.</exception>
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

    /// <summary>Validates and packs one required RGB5 palette sequence into BGR555 words.</summary>
    /// <param name="colors">Optional source colors from the JSON document.</param>
    /// <param name="count">Required number of colors.</param>
    /// <param name="name">Palette family or row label included in validation errors.</param>
    /// <returns>Packed BGR555 colors in source order.</returns>
    /// <exception cref="InvalidDataException">The color array has the wrong length or contains a missing color or channel outside 0..31.</exception>
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

    /// <summary>Rejects repeated property names recursively using the asset's case-sensitive JSON naming policy.</summary>
    /// <param name="value">Parsed JSON root whose object properties are checked.</param>
    /// <exception cref="InvalidDataException">A duplicate property is found in the document.</exception>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate Ceres Ridley color property {name}."));
}

/// <summary>Editable JSON payload of native Ridley/Baby palette rows; colors use independent integer RGB5 channels, and row indices describe paint phases rather than animation durations.</summary>
public sealed record CeresRidleyColorDocument
{
    /// <summary>Schema selector: three requires all palette families; versions one and two are accepted by <see cref="CeresRidleyColorCatalog.Load"/> only with a stock fallback.</summary>
    public required int Version { get; init; }
    /// <summary>32 startup colors corresponding to $A6:E16F, including both transparent entries of the door/container and Baby OBJ palettes.</summary>
    public required PaletteRgb5[] Start { get; init; }
    /// <summary>Sixteen rows of three eye colors corresponding to $A6:E2AA, ordered from full brightness toward black.</summary>
    public required PaletteRgb5[][] EyeFade { get; init; }
    /// <summary>Sixteen rows of eleven body colors corresponding to $A6:E30A, ordered from black toward full brightness and used in both BG and OBJ domains.</summary>
    public required PaletteRgb5[][] BodyFade { get; init; }
    /// <summary>Three rows of fourteen health colors corresponding to $A6:E46A, shared by Ceres shot-count and Norfair HP palette selection.</summary>
    public required PaletteRgb5[][] Health { get; init; }
    /// <summary>Sixteen three-color self-destruct text phases corresponding to $A6:C1DF; required by version three, while older documents inherit the stock alarm.</summary>
    public PaletteRgb5[][]? Alarm { get; init; }
    /// <summary>Fifteen retreat BG colors corresponding to $A6:A9E3, excluding palette entry zero and copied to CGRAM entries 81..95.</summary>
    public required PaletteRgb5[] RetreatBg { get; init; }
    /// <summary>Eight retreat colors corresponding to $A6:AA01, copied identically into BG and OBJ palettes; edits remain independent of matching startup colors.</summary>
    public required PaletteRgb5[] RetreatShared { get; init; }
    /// <summary>Four rows of fifteen private Baby/container draw colors corresponding to $A6:E1F1; required from version two onward, excluding OBJ palette three's transparent entry.</summary>
    public PaletteRgb5[][]? Baby { get; init; }
}

/// <summary>Filename and schema-version identities for the installed Ridley/Baby RGB5 palette asset and its supported legacy override migrations.</summary>
public static class CeresRidleyColorFormat
{
    /// <summary>Resource-root JSON filename consumed by the presentation catalog's migrating-resource loader.</summary>
    public const string FileName = "ceres-ridley-colors.json";
    /// <summary>Current schema version three, requiring the complete startup, fade, health, retreat, Baby, and alarm palette families.</summary>
    public const int Version = 3;
    /// <summary>Legacy schema version one, lacking Baby and alarm rows; loading requires a stock catalog to provide both families.</summary>
    public const int PreBabyVersion = 1;
    /// <summary>Legacy schema version two, containing Baby rows but lacking alarm rows; loading requires a stock catalog for the alarm family.</summary>
    public const int PreAlarmVersion = 2;
}

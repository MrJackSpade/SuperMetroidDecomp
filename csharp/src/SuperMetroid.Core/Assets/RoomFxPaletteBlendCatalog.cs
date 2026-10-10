using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable three-color room-FX blends selected by native FX records.</summary>
public sealed class RoomFxPaletteBlendCatalog
{
    private readonly RoomFxBlendColors lava;
    private readonly RoomFxBlendColors landingSiteRain;
    private readonly RoomFxBlendColors maridiaWaterA;
    private readonly RoomFxBlendColors waterAndAcid;
    private readonly RoomFxBlendColors fog;
    private readonly RoomFxBlendColors maridiaWaterB;
    private readonly RoomFxBlendColors maridiaWaterC;
    private readonly RoomFxBlendColors maridiaWaterD;

    private RoomFxPaletteBlendCatalog(RoomFxBlendColors lava, RoomFxBlendColors landingSiteRain,
        RoomFxBlendColors maridiaWaterA, RoomFxBlendColors waterAndAcid, RoomFxBlendColors fog,
        RoomFxBlendColors maridiaWaterB, RoomFxBlendColors maridiaWaterC, RoomFxBlendColors maridiaWaterD,
        PaletteRgb5 ceresHazeBlue, PaletteRgb5 ceresHazeRed)
    {
        this.lava = lava;
        this.landingSiteRain = landingSiteRain;
        this.maridiaWaterA = maridiaWaterA;
        this.waterAndAcid = waterAndAcid;
        this.fog = fog;
        this.maridiaWaterB = maridiaWaterB;
        this.maridiaWaterC = maridiaWaterC;
        this.maridiaWaterD = maridiaWaterD;
        CeresHazeBlue = ceresHazeBlue;
        CeresHazeRed = ceresHazeRed;
    }

    /// <summary>Cosmetic fixed-color tint selected when Ceres Ridley is alive.</summary>
    public PaletteRgb5 CeresHazeBlue { get; }

    /// <summary>Cosmetic fixed-color tint selected after Ceres Ridley is defeated.</summary>
    public PaletteRgb5 CeresHazeRed { get; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads all eight three-color room-FX blends and optional Ceres haze tints, rejecting duplicate or unknown JSON properties, incomplete selections, and RGB components outside 0..31.</summary>
    /// <param name="json">Caller-owned stream containing the supported blend-palette JSON document.</param>
    /// <returns>Installed BG3 inks plus fixed-color haze tints; native selectors, CGRAM destinations, and fade timing remain compiled mechanics.</returns>
    /// <exception cref="InvalidDataException">The JSON, schema version, blend identities, color count, or RGB5 values are invalid.</exception>
    public static RoomFxPaletteBlendCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        RoomFxPaletteBlendDocument document;
        try
        {
            using JsonDocument parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<RoomFxPaletteBlendDocument>(JsonOptions)
                ?? throw new InvalidDataException("Room-FX blend palette JSON is null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid room-FX blend palette JSON.", error);
        }
        if (document.Version != RoomFxPaletteBlendDefinitions.Version ||
            document.Blends is null || document.Blends.Count != RoomFxPaletteBlendDefinitions.Ids.Count())
            throw new InvalidDataException("Room-FX blend palettes require the supported version and all eight selections.");

        return new(Compile(RoomFxPaletteBlendDefinitions.Lava),
            Compile(RoomFxPaletteBlendDefinitions.LandingSiteRain),
            Compile(RoomFxPaletteBlendDefinitions.MaridiaWaterA),
            Compile(RoomFxPaletteBlendDefinitions.WaterAndAcid),
            Compile(RoomFxPaletteBlendDefinitions.Fog),
            Compile(RoomFxPaletteBlendDefinitions.MaridiaWaterB),
            Compile(RoomFxPaletteBlendDefinitions.MaridiaWaterC),
            Compile(RoomFxPaletteBlendDefinitions.MaridiaWaterD),
            ValidateHazeTint(document.CeresHazeBlue ?? RoomFxPaletteBlendDefinitions.StockCeresHazeBlue,
                nameof(document.CeresHazeBlue)),
            ValidateHazeTint(document.CeresHazeRed ?? RoomFxPaletteBlendDefinitions.StockCeresHazeRed,
                nameof(document.CeresHazeRed)));

        RoomFxBlendColors Compile(byte id)
        {
            if (!document.Blends.TryGetValue(RoomFxPaletteBlendDefinitions.Key(id), out PaletteRgb5[]? colors) ||
                colors is null || colors.Length != RoomFxRomData.Layer3.PaletteBlendColorCount)
                throw new InvalidDataException($"Room-FX blend {id:X2} requires three colors.");
            var words = new Bgr555[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5? color = colors[index];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Room-FX blend {id:X2} color {index} requires RGB components from zero through 31.");
                words[index] = color.ToBgr555();
            }
            return new(id, words[0], words[1], words[2]);
        }
    }

    /// <summary>Serializes a complete blend-palette document as UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Eight named blend selections and optional haze tint overrides.</param>
    /// <returns>The validated UTF-8 JSON payload; this method does not write an external resource.</returns>
    public static byte[] Write(RoomFxPaletteBlendDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    /// <summary>Applies the native three-color write, or clears only color 27 for selection zero.</summary>
    public void Apply(SnesCgram cgram, byte selection)
    {
        ArgumentNullException.ThrowIfNull(cgram);
        if (selection == 0)
        {
            cgram.SetColor(RoomFxRomData.Layer3.EmptyPaletteColorIndex, Bgr555.Black);
            return;
        }
        SelectColors(selection).Apply(cgram);
    }

    private RoomFxBlendColors SelectColors(byte selection) => selection switch
    {
        RoomFxPaletteBlendDefinitions.Lava => lava,
        RoomFxPaletteBlendDefinitions.LandingSiteRain => landingSiteRain,
        RoomFxPaletteBlendDefinitions.MaridiaWaterA => maridiaWaterA,
        RoomFxPaletteBlendDefinitions.WaterAndAcid => waterAndAcid,
        RoomFxPaletteBlendDefinitions.Fog => fog,
        RoomFxPaletteBlendDefinitions.MaridiaWaterB => maridiaWaterB,
        RoomFxPaletteBlendDefinitions.MaridiaWaterC => maridiaWaterC,
        RoomFxPaletteBlendDefinitions.MaridiaWaterD => maridiaWaterD,
        _ => throw new InvalidDataException($"Room-FX palette blend ${selection:X2} is not an authored retail selection."),
    };
    private static PaletteRgb5 ValidateHazeTint(PaletteRgb5 color, string name)
    {
        if ((uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"{name} requires RGB components from zero through 31.");
        return color;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException($"Duplicate room-FX blend property {name}."));
}

/// <summary>Loaded artwork inks with calculated components separated from arbitrary edits.</summary>
/// <remarks>
/// Native89:AA02+selector supplies primary/secondary/background inks copied by89:AB5E
/// to BG3 palette25..27. The remaining24 independent channels specify each theme's
/// chosen hue/brightness, not samples of time or distance. Index interpolation or
/// fitted color transforms would merely recite those choices as code. Their specific
/// artistic-content/nonsense dispositions are in roomFxIndependentInkReview in the
/// issue1165 inventory;48 uniform/shared channels are calculated independently.
/// </remarks>
internal sealed class RoomFxBlendColors
{
    private readonly RoomFxPairColor primary;
    private readonly RoomFxPairColor secondary;
    private readonly RoomFxThirdColor? thirdOverride;

    public RoomFxBlendColors(byte selection, Bgr555 primary, Bgr555 secondary, Bgr555 third)
    {
        this.primary = new(selection, primary, true);
        this.secondary = new(selection, secondary, false);
        thirdOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdColor(selection) == third ? null : new(selection, third);
    }

    public void Apply(SnesCgram cgram)
    {
        cgram.SetColor(RoomFxRomData.Layer3.PaletteBlendDestinationIndex, primary.CreateColor());
        cgram.SetColor(RoomFxRomData.Layer3.PaletteBlendDestinationIndex + 1, secondary.CreateColor());
        cgram.SetColor(RoomFxRomData.Layer3.PaletteBlendDestinationIndex + 2, thirdOverride?.CreateColor() ?? Bgr555.Black);
    }
}
/// <summary>One of the first two blend colors, separating shared tint rules from edits.</summary>
internal sealed class RoomFxPairColor
{
    private readonly byte selection;
    private readonly bool isPrimary;
    private readonly int? redOverride;
    private readonly int? greenOverride;
    private readonly int? blueOverride;

    public RoomFxPairColor(byte selection, Bgr555 color, bool isPrimary)
    {
        this.selection = selection;
        this.isPrimary = isPrimary;
        int red = color.Red, green = color.Green, blue = color.Blue;
        redOverride = RoomFxPaletteBlendDefinitions.CalculatedPairRed(selection, isPrimary) == red ? null : red;
        greenOverride = RoomFxPaletteBlendDefinitions.CalculatedPairGreen(selection, red, isPrimary) == green ? null : green;
        blueOverride = RoomFxPaletteBlendDefinitions.CalculatedPairBlue(selection, red, green, isPrimary) == blue ? null : blue;
    }

    public Bgr555 CreateColor()
    {
        int red = redOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairRed(selection, isPrimary)!.Value;
        int green = greenOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairGreen(selection, red, isPrimary)!.Value;
        int blue = blueOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairBlue(selection, red, green, isPrimary)!.Value;
        return new Bgr555(red, green, blue);
    }
}
/// <summary>Independent third-color red and calculated weather green/blue, preserving edits.</summary>
internal sealed class RoomFxThirdColor
{
    private readonly byte selection;
    private readonly int red;
    private readonly int? greenOverride;
    private readonly int? blueOverride;

    public RoomFxThirdColor(byte selection, Bgr555 color)
    {
        this.selection = selection;
        red = color.Red;
        int green = color.Green;
        greenOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(selection) == green ? null : green;
        int blue = color.Blue;
        blueOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(selection, color.Red) == blue ? null : blue;
    }

    public Bgr555 CreateColor() => new(red,
        greenOverride ?? RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(selection)!.Value,
        blueOverride ?? RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(selection, red)!.Value);
}
/// <summary>Editable RGB5 inks for the eight native room-FX blend selections, with separate optional fixed-color tints for Ceres haze.</summary>
public sealed record RoomFxPaletteBlendDocument
{
    /// <summary>Schema revision, required to equal <see cref="RoomFxPaletteBlendDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>Exactly eight <c>blend-XX</c> keys generated by <see cref="RoomFxPaletteBlendDefinitions.Key"/>, each containing three non-null RGB5 colors in CGRAM 25, 26, 27 write order.</summary>
    public required Dictionary<string, PaletteRgb5[]> Blends { get; init; }
    /// <summary>Optional for older version-one overrides; missing means the cartridge blue tint.</summary>
    public PaletteRgb5? CeresHazeBlue { get; init; }
    /// <summary>Optional for older version-one overrides; missing means the cartridge red tint.</summary>
    public PaletteRgb5? CeresHazeRed { get; init; }
}

/// <summary>Native bank-$89 room-FX blend selectors, distinct from editable colors.</summary>
public static class RoomFxPaletteBlendDefinitions
{
    /// <summary>Editable JSON filename for room-FX blend inks and optional Ceres haze tints.</summary>
    public const string FileName = "room-fx-blend-palettes.json";
    /// <summary>Supported revision of the complete eight-selection blend schema; haze tint fields are optional within this revision.</summary>
    public const int Version = 1;

    /// <summary>Stock blue-channel fixed-color amplitude for Ceres haze.</summary>
    public static PaletteRgb5 StockCeresHazeBlue => StockCeresHaze(false);
    /// <summary>Stock red-channel fixed-color amplitude after Ceres Ridley.</summary>
    public static PaletteRgb5 StockCeresHazeRed => StockCeresHaze(true);

    /// <summary>
    /// Native $88:DE10/DE15 selects COLDATA blue/red; DE42 stops fade-in at counter16,
    /// after counter15 was the final written amplitude. Calculate that selected RGB
    /// axis directly instead of retaining two default tint records.
    /// </summary>
    public static PaletteRgb5 StockCeresHaze(bool ridleyIsDead)
    {
        var tint = StockCeresHazeComponents(ridleyIsDead);
        return new() { Red = tint.Red, Green = tint.Green, Blue = tint.Blue };
    }

    /// <summary>The same native tint as scalar components, without constructing a per-scanline record.</summary>
    internal static (int Red, int Green, int Blue) StockCeresHazeComponents(bool ridleyIsDead) =>
        (ridleyIsDead ? CeresHazeDefinitions.FadeSteps - 1 : 0, 0,
            ridleyIsDead ? 0 : CeresHazeDefinitions.FadeSteps - 1);
    /// <summary>FX-record selector $02, used primarily for lava/acid.</summary>
    public const byte Lava = 0x02;
    /// <summary>FX-record selector $22, used by Landing Site rain.</summary>
    public const byte LandingSiteRain = 0x22;
    /// <summary>FX-record selector $42, used by several Maridia water rooms.</summary>
    public const byte MaridiaWaterA = 0x42;
    /// <summary>FX-record selector $48, used by Ceres and other water/acid rooms.</summary>
    public const byte WaterAndAcid = 0x48;
    /// <summary>FX-record selector $62, used by fog and nonliquid room states.</summary>
    public const byte Fog = 0x62;
    /// <summary>FX-record selector $E2, used by western Maridia water rooms.</summary>
    public const byte MaridiaWaterB = 0xe2;
    /// <summary>FX-record selector $E8, used by central Maridia water rooms.</summary>
    public const byte MaridiaWaterC = 0xe8;
    /// <summary>FX-record selector $EE, used by eastern Maridia water rooms.</summary>
    public const byte MaridiaWaterD = 0xee;

    /// <summary>The eight named retail blend resources in document order, without stored identities.</summary>
    public static IEnumerable<byte> Ids
    {
        get
        {
            yield return Lava;
            yield return LandingSiteRain;
            yield return MaridiaWaterA;
            yield return WaterAndAcid;
            yield return Fog;
            yield return MaridiaWaterB;
            yield return MaridiaWaterC;
            yield return MaridiaWaterD;
        }
    }

    /// <summary>Creates the exact JSON blend key for a supported native byte-offset selector into $89:AA02, not a sequential palette ordinal.</summary>
    /// <param name="id">One of the eight selectors exposed by <see cref="Ids"/>; selection zero is the runtime clear operation and has no resource key.</param>
    /// <returns><c>blend-</c> followed by the selector's two uppercase hexadecimal digits.</returns>
    /// <exception cref="InvalidDataException">The selector is not one of the eight catalogued blends.</exception>
    public static string Key(byte id)
    {
        ValidateSelector(id);
        return $"blend-{id:X2}";
    }

    private static void ValidateSelector(byte id)
    {
        if (id is not (Lava or LandingSiteRain or MaridiaWaterA or WaterAndAcid or Fog or MaridiaWaterB or MaridiaWaterC or MaridiaWaterD))
            throw new InvalidDataException($"Room-FX palette blend ${id:X2} is not catalogued.");
    }
    /// <summary>
    /// The six liquid blends use black as their third color. Weather's independent
    /// third colors have no calculated value here. Unknown selectors still reject.
    /// </summary>
    public static Bgr555? CalculatedThirdColor(byte id) => id switch
    {
        Lava or MaridiaWaterA or WaterAndAcid or MaridiaWaterB or MaridiaWaterC or MaridiaWaterD => Bgr555.Black,
        LandingSiteRain or Fog => null,
        _ => throw new InvalidDataException($"Room-FX palette blend ${id:X2} is not catalogued."),
    };
    /// <summary>Lava shares full red; Maridia A/B/D share a zero-red dark-blue primary.</summary>
    public static int? CalculatedPairRed(byte id, bool isPrimary)
    {
        ValidateSelector(id);
        if (isPrimary && id is (MaridiaWaterA or MaridiaWaterB or MaridiaWaterD)) return 0;
        return id == Lava ? 31 : null;
    }

    /// <summary>Rain, fog and Maridia A share red/green intensity; Maridia C adds green1; A/B/D primary green is zero.</summary>
    public static int? CalculatedPairGreen(byte id, int red, bool isPrimary)
    {
        ValidateSelector(id);
        if ((uint)red > 31) throw new ArgumentOutOfRangeException(nameof(red));
        if (isPrimary && id is (MaridiaWaterA or MaridiaWaterB or MaridiaWaterD)) return 0;
        if (id == MaridiaWaterC) return Math.Min(31, red + 1);
        return id is LandingSiteRain or Fog or MaridiaWaterA ? red : null;
    }

    /// <summary>
    /// Maridia A/B/D share primary blue1; lava's first two colors share blue3.
    /// Sandy Maridia shares primary red/blue and secondary green/blue.
    /// Rain and fog tint their red/green intensity
    /// with blue+2/+3. Saturation defines the RGB5 extension for custom intensities;
    /// stock intensities do not saturate, and differing user components remain overrides.
    /// </summary>
    public static int? CalculatedPairBlue(byte id, int red, int green, bool isPrimary)
    {
        ValidateSelector(id);
        if ((uint)red > 31) throw new ArgumentOutOfRangeException(nameof(red));
        if ((uint)green > 31) throw new ArgumentOutOfRangeException(nameof(green));
        if (isPrimary && id is (MaridiaWaterA or MaridiaWaterB or MaridiaWaterD)) return 1;
        return id switch
        {
            MaridiaWaterC => isPrimary ? red : green,
            Lava => 3,
            LandingSiteRain => Math.Min(31, red + 2),
            Fog => Math.Min(31, red + 3),
            _ => null,
        };
    }
    /// <summary>Both weather third colors share green1; liquid third-color overrides remain independent.</summary>
    public static int? CalculatedThirdGreen(byte id)
    {
        ValidateSelector(id);
        return id is LandingSiteRain or Fog ? 1 : null;
    }
    /// <summary>Rain keeps the same blue tint in its third color; fog's third color shares red/blue.</summary>
    public static int? CalculatedThirdBlue(byte id, int red)
    {
        ValidateSelector(id);
        if ((uint)red > 31) throw new ArgumentOutOfRangeException(nameof(red));
        return id switch
        {
            LandingSiteRain => Math.Min(31, red + 2),
            Fog => red,
            _ => null,
        };
    }
    /// <summary>Native byte address for the first of three adjacent BGR555 colors.</summary>
    public static int SourceAddress(byte id)
    {
        ValidateSelector(id);
        return RoomFxRomData.Tables.PaletteBlendColors + id;
    }
}

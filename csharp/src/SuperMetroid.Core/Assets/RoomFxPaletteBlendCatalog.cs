using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Editable three-color room-FX blends selected by native FX records.</summary>
public sealed class RoomFxPaletteBlendCatalog
{
    /// <summary>Three installed CGRAM inks for the lava selector.</summary>
    private readonly RoomFxBlendColors lava;
    /// <summary>Three installed CGRAM inks for Landing Site rain.</summary>
    private readonly RoomFxBlendColors landingSiteRain;
    /// <summary>Three installed inks for Maridia's first water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterA;
    /// <summary>Three installed inks shared by the water and acid selector.</summary>
    private readonly RoomFxBlendColors waterAndAcid;
    /// <summary>Three installed inks for the fog selector.</summary>
    private readonly RoomFxBlendColors fog;
    /// <summary>Three installed inks for Maridia's second water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterB;
    /// <summary>Three installed inks for Maridia's third water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterC;
    /// <summary>Three installed inks for Maridia's fourth water selector.</summary>
    private readonly RoomFxBlendColors maridiaWaterD;

    /// <summary>Retains each validated room-FX ink and both optional Ceres haze tints.</summary>
    private RoomFxPaletteBlendCatalog(RoomFxPaletteBlendParts parts)
    {
        this.lava = parts.Lava;
        this.landingSiteRain = parts.LandingSiteRain;
        this.maridiaWaterA = parts.MaridiaWaterA;
        this.waterAndAcid = parts.WaterAndAcid;
        this.fog = parts.Fog;
        this.maridiaWaterB = parts.MaridiaWaterB;
        this.maridiaWaterC = parts.MaridiaWaterC;
        this.maridiaWaterD = parts.MaridiaWaterD;
        CeresHazeBlue = parts.CeresHazeBlue;
        CeresHazeRed = parts.CeresHazeRed;
    }

    /// <summary>Cosmetic fixed-color tint selected when Ceres Ridley is alive.</summary>
    public PaletteRgb5 CeresHazeBlue { get; }

    /// <summary>Cosmetic fixed-color tint selected after Ceres Ridley is defeated.</summary>
    public PaletteRgb5 CeresHazeRed { get; }

    /// <summary>Strict camel-case JSON options for the editable room-FX palette document.</summary>
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
        Ensure.NotNull(json);
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

        return new(new RoomFxPaletteBlendParts { Lava = Compile(RoomFxPaletteBlend.Lava), LandingSiteRain = Compile(RoomFxPaletteBlend.LandingSiteRain), MaridiaWaterA = Compile(RoomFxPaletteBlend.MaridiaWaterA), WaterAndAcid = Compile(RoomFxPaletteBlend.WaterAndAcid), Fog = Compile(RoomFxPaletteBlend.Fog), MaridiaWaterB = Compile(RoomFxPaletteBlend.MaridiaWaterB), MaridiaWaterC = Compile(RoomFxPaletteBlend.MaridiaWaterC), MaridiaWaterD = Compile(RoomFxPaletteBlend.MaridiaWaterD), CeresHazeBlue = ValidateHazeTint(document.CeresHazeBlue ?? RoomFxPaletteBlendDefinitions.StockCeresHazeBlue,
                nameof(document.CeresHazeBlue)), CeresHazeRed = ValidateHazeTint(document.CeresHazeRed ?? RoomFxPaletteBlendDefinitions.StockCeresHazeRed,
                nameof(document.CeresHazeRed)) });

        RoomFxBlendColors Compile(RoomFxPaletteBlend id)
        {
            if (!document.Blends.TryGetValue(RoomFxPaletteBlendDefinitions.Key(id), out PaletteRgb5[]? colors) ||
                colors is null || colors.Length != RoomFxRomData.Layer3.PaletteBlendColorCount)
                throw new InvalidDataException($"Room-FX blend {(int)id:X2} requires three colors.");
            var words = new Bgr555[colors.Length];
            for (int index = 0; index < colors.Length; index++)
            {
                PaletteRgb5? color = colors[index];
                if (color is null || (uint)color.Red > 31 ||
                    (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException($"Room-FX blend {(int)id:X2} color {index} requires RGB components from zero through 31.");
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
    public void Apply(SnesCgram cgram, RoomFxPaletteBlend selection)
    {
        Ensure.NotNull(cgram);
        if (selection == RoomFxPaletteBlend.None)
        {
            new RoomFxPalettes(cgram).Layer3Empty.Write(0, Bgr555.Black);
            return;
        }
        SelectColors(selection).Apply(cgram);
    }

    /// <summary>Resolves one authored blend selector to its installed three-color record.</summary>
    internal RoomFxBlendColors SelectColors(RoomFxPaletteBlend selection) => selection switch
    {
        RoomFxPaletteBlend.Lava => lava,
        RoomFxPaletteBlend.LandingSiteRain => landingSiteRain,
        RoomFxPaletteBlend.MaridiaWaterA => maridiaWaterA,
        RoomFxPaletteBlend.WaterAndAcid => waterAndAcid,
        RoomFxPaletteBlend.Fog => fog,
        RoomFxPaletteBlend.MaridiaWaterB => maridiaWaterB,
        RoomFxPaletteBlend.MaridiaWaterC => maridiaWaterC,
        RoomFxPaletteBlend.MaridiaWaterD => maridiaWaterD,
        _ => throw new InvalidDataException($"Room-FX palette blend ${(int)selection:X2} is not an authored retail selection."),
    };
    /// <summary>Rejects a Ceres fixed-color tint with a component outside RGB5 range.</summary>
    private static PaletteRgb5 ValidateHazeTint(PaletteRgb5 color, string name)
    {
        if ((uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
            throw new InvalidDataException($"{name} requires RGB components from zero through 31.");
        return color;
    }

    /// <summary>Rejects duplicate JSON properties before deserialization can overwrite them.</summary>
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
    /// <summary>First BG3 ink, with shared tint rules separated from user overrides.</summary>
    internal readonly RoomFxPairColor primary;
    /// <summary>Second BG3 ink, with shared tint rules separated from user overrides.</summary>
    internal readonly RoomFxPairColor secondary;
    /// <summary>Third ink only when it differs from the selector's calculated color.</summary>
    internal readonly RoomFxThirdColor? thirdOverride;

    /// <summary>Separates selected colors into calculated components and explicit edits.</summary>
    public RoomFxBlendColors(RoomFxPaletteBlend selection, Bgr555 primary, Bgr555 secondary, Bgr555 third)
    {
        this.primary = new(selection, primary, true);
        this.secondary = new(selection, secondary, false);
        thirdOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdColor(selection) == third ? null : new(selection, third);
    }

    /// <summary>Writes the three resulting blend inks to BG3 palette entries 25 through 27.</summary>
    public void Apply(SnesCgram cgram)
    {
        new RoomFxPalettes(cgram).Layer3Blend.Write(0, primary.CreateColor());
        new RoomFxPalettes(cgram).Layer3Blend.Write(1, secondary.CreateColor());
        new RoomFxPalettes(cgram).Layer3Blend.Write(2, thirdOverride?.CreateColor() ?? Bgr555.Black);
    }
}
/// <summary>One of the first two blend colors, separating shared tint rules from edits.</summary>
internal sealed class RoomFxPairColor
{
    /// <summary>Native blend selector used by the shared component calculations.</summary>
    private readonly RoomFxPaletteBlend selection;
    /// <summary>Distinguishes the first ink from the second ink's tint rules.</summary>
    private readonly bool isPrimary;
    /// <summary>User red component when it differs from the calculated value.</summary>
    internal readonly int? redOverride;
    /// <summary>User green component when it differs from the calculated value.</summary>
    internal readonly int? greenOverride;
    /// <summary>User blue component when it differs from the calculated value.</summary>
    internal readonly int? blueOverride;

    /// <summary>Stores only RGB components that depart from a selector's shared tint rules.</summary>
    public RoomFxPairColor(RoomFxPaletteBlend selection, Bgr555 color, bool isPrimary)
    {
        this.selection = selection;
        this.isPrimary = isPrimary;
        int red = color.Red, green = color.Green, blue = color.Blue;
        redOverride = RoomFxPaletteBlendDefinitions.CalculatedPairRed(selection, isPrimary) == red ? null : red;
        greenOverride = RoomFxPaletteBlendDefinitions.CalculatedPairGreen(selection, red, isPrimary) == green ? null : green;
        blueOverride = RoomFxPaletteBlendDefinitions.CalculatedPairBlue(selection, red, green, isPrimary) == blue ? null : blue;
    }

    /// <summary>Rebuilds the ink from retained edits and any still-applicable shared tint rules.</summary>
    public Bgr555 CreateColor()
    {
        int red = redOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairRed(selection, isPrimary) ??
            throw new InvalidOperationException("A blend color without a red edit has a calculated red.");
        int green = greenOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairGreen(selection, red, isPrimary) ??
            throw new InvalidOperationException("A blend color without a green edit has a calculated green.");
        int blue = blueOverride ?? RoomFxPaletteBlendDefinitions.CalculatedPairBlue(selection, red, green, isPrimary) ??
            throw new InvalidOperationException("A blend color without a blue edit has a calculated blue.");
        return new Bgr555(red, green, blue);
    }
}
/// <summary>Independent third-color red and calculated weather green/blue, preserving edits.</summary>
internal sealed class RoomFxThirdColor
{
    /// <summary>Native selector used to calculate the weather ink's shared channels.</summary>
    private readonly RoomFxPaletteBlend selection;
    /// <summary>Independent red channel of the third ink.</summary>
    private readonly int red;
    /// <summary>Edited green channel when the weather rule does not match.</summary>
    internal readonly int? greenOverride;
    /// <summary>Edited blue channel when the weather rule does not match.</summary>
    internal readonly int? blueOverride;

    /// <summary>Retains independent red and only the green/blue components that override weather rules.</summary>
    public RoomFxThirdColor(RoomFxPaletteBlend selection, Bgr555 color)
    {
        this.selection = selection;
        red = color.Red;
        int green = color.Green;
        greenOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(selection) == green ? null : green;
        int blue = color.Blue;
        blueOverride = RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(selection, color.Red) == blue ? null : blue;
    }

    /// <summary>Combines the retained red channel with edited or calculated weather channels.</summary>
    public Bgr555 CreateColor() => new(red,
        greenOverride ?? RoomFxPaletteBlendDefinitions.CalculatedThirdGreen(selection) ??
            throw new InvalidOperationException("A blend color without a green edit has a calculated green."),
        blueOverride ?? RoomFxPaletteBlendDefinitions.CalculatedThirdBlue(selection, red) ??
            throw new InvalidOperationException("A blend color without a blue edit has a calculated blue."));
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

    /// <summary>The eight named retail blend resources in document order, without stored identities.</summary>
    public static IEnumerable<RoomFxPaletteBlend> Ids
    {
        get
        {
            yield return RoomFxPaletteBlend.Lava;
            yield return RoomFxPaletteBlend.LandingSiteRain;
            yield return RoomFxPaletteBlend.MaridiaWaterA;
            yield return RoomFxPaletteBlend.WaterAndAcid;
            yield return RoomFxPaletteBlend.Fog;
            yield return RoomFxPaletteBlend.MaridiaWaterB;
            yield return RoomFxPaletteBlend.MaridiaWaterC;
            yield return RoomFxPaletteBlend.MaridiaWaterD;
        }
    }

    /// <summary>Creates the exact JSON blend key for a supported native byte-offset selector into $89:AA02, not a sequential palette ordinal.</summary>
    /// <param name="id">One of the eight selectors exposed by <see cref="Ids"/>; selection zero is the runtime clear operation and has no resource key.</param>
    /// <returns><c>blend-</c> followed by the selector's two uppercase hexadecimal digits.</returns>
    /// <exception cref="InvalidDataException">The selector is not one of the eight catalogued blends.</exception>
    public static string Key(RoomFxPaletteBlend id)
    {
        ValidateSelector(id);
        return $"blend-{(byte)id:X2}";
    }

    /// <summary>Requires one of the eight authored nonzero palette blend selectors.</summary>
    private static void ValidateSelector(RoomFxPaletteBlend id)
    {
        if (id is not (RoomFxPaletteBlend.Lava or RoomFxPaletteBlend.LandingSiteRain or RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.WaterAndAcid or RoomFxPaletteBlend.Fog or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterC or RoomFxPaletteBlend.MaridiaWaterD))
            throw new InvalidDataException($"Room-FX palette blend ${(byte)id:X2} is not catalogued.");
    }
    /// <summary>
    /// The six liquid blends use black as their third color. Weather's independent
    /// third colors have no calculated value here. Unknown selectors still reject.
    /// </summary>
    public static Bgr555? CalculatedThirdColor(RoomFxPaletteBlend id) => id switch
    {
        RoomFxPaletteBlend.Lava or RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.WaterAndAcid or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterC or RoomFxPaletteBlend.MaridiaWaterD => Bgr555.Black,
        RoomFxPaletteBlend.LandingSiteRain or RoomFxPaletteBlend.Fog => null,
        _ => throw new InvalidDataException($"Room-FX palette blend ${(byte)id:X2} is not catalogued."),
    };
    /// <summary>RoomFxPaletteBlend.Lava shares full red; Maridia A/B/D share a zero-red dark-blue primary.</summary>
    public static int? CalculatedPairRed(RoomFxPaletteBlend id, bool isPrimary)
    {
        ValidateSelector(id);
        if (isPrimary && id is (RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterD)) return 0;
        return id == RoomFxPaletteBlend.Lava ? 31 : null;
    }

    /// <summary>Rain, fog and Maridia A share red/green intensity; Maridia C adds green1; A/B/D primary green is zero.</summary>
    public static int? CalculatedPairGreen(RoomFxPaletteBlend id, int red, bool isPrimary)
    {
        ValidateSelector(id);
        if ((uint)red > 31) throw new ArgumentOutOfRangeException(nameof(red));
        if (isPrimary && id is (RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterD)) return 0;
        if (id == RoomFxPaletteBlend.MaridiaWaterC) return Math.Min(31, red + 1);
        return id is RoomFxPaletteBlend.LandingSiteRain or RoomFxPaletteBlend.Fog or RoomFxPaletteBlend.MaridiaWaterA ? red : null;
    }

    /// <summary>
    /// Maridia A/B/D share primary blue1; lava's first two colors share blue3.
    /// Sandy Maridia shares primary red/blue and secondary green/blue.
    /// Rain and fog tint their red/green intensity
    /// with blue+2/+3. Saturation defines the RGB5 extension for custom intensities;
    /// stock intensities do not saturate, and differing user components remain overrides.
    /// </summary>
    public static int? CalculatedPairBlue(RoomFxPaletteBlend id, int red, int green, bool isPrimary)
    {
        ValidateSelector(id);
        if ((uint)red > 31) throw new ArgumentOutOfRangeException(nameof(red));
        if ((uint)green > 31) throw new ArgumentOutOfRangeException(nameof(green));
        if (isPrimary && id is (RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterD)) return 1;
        return id switch
        {
            RoomFxPaletteBlend.MaridiaWaterC => isPrimary ? red : green,
            RoomFxPaletteBlend.Lava => 3,
            RoomFxPaletteBlend.LandingSiteRain => Math.Min(31, red + 2),
            RoomFxPaletteBlend.Fog => Math.Min(31, red + 3),
            RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.WaterAndAcid or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterD => null,
            _ => throw new InvalidOperationException($"Undefined RoomFxPaletteBlend {id}."),
        };
    }
    /// <summary>Both weather third colors share green1; liquid third-color overrides remain independent.</summary>
    public static int? CalculatedThirdGreen(RoomFxPaletteBlend id)
    {
        ValidateSelector(id);
        return id is RoomFxPaletteBlend.LandingSiteRain or RoomFxPaletteBlend.Fog ? 1 : null;
    }
    /// <summary>Rain keeps the same blue tint in its third color; fog's third color shares red/blue.</summary>
    public static int? CalculatedThirdBlue(RoomFxPaletteBlend id, int red)
    {
        ValidateSelector(id);
        if ((uint)red > 31) throw new ArgumentOutOfRangeException(nameof(red));
        return id switch
        {
            RoomFxPaletteBlend.LandingSiteRain => Math.Min(31, red + 2),
            RoomFxPaletteBlend.Fog => red,
            RoomFxPaletteBlend.Lava or RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.WaterAndAcid or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterC or RoomFxPaletteBlend.MaridiaWaterD => null,
            _ => throw new InvalidOperationException($"Undefined RoomFxPaletteBlend {id}."),
        };
    }
    /// <summary>Native byte address for the first of three adjacent BGR555 colors.</summary>
    public static int SourceAddress(RoomFxPaletteBlend id)
    {
        ValidateSelector(id);
        return RoomFxRomData.Tables.PaletteBlendColors + (int)id;
    }
}

/// <summary>Native bank-$89 room-FX blend selectors: byte offsets into $89:AA02, zero clearing the blend.</summary>
public enum RoomFxPaletteBlend : byte
{
    /// <summary>Selection zero clears the layer-3 blend color instead of loading a resource.</summary>
    None = 0,
    /// <summary>FX-record selector $02, used primarily for lava/acid.</summary>
    Lava = 0x02,
    /// <summary>FX-record selector $22, used by Landing Site rain.</summary>
    LandingSiteRain = 0x22,
    /// <summary>FX-record selector $42, used by several Maridia water rooms.</summary>
    MaridiaWaterA = 0x42,
    /// <summary>FX-record selector $48, used by Ceres and other water/acid rooms.</summary>
    WaterAndAcid = 0x48,
    /// <summary>FX-record selector $62, used by fog and nonliquid room states.</summary>
    Fog = 0x62,
    /// <summary>FX-record selector $E2, used by western Maridia water rooms.</summary>
    MaridiaWaterB = 0xe2,
    /// <summary>FX-record selector $E8, used by central Maridia water rooms.</summary>
    MaridiaWaterC = 0xe8,
    /// <summary>FX-record selector $EE, used by eastern Maridia water rooms.</summary>
    MaridiaWaterD = 0xee,
}

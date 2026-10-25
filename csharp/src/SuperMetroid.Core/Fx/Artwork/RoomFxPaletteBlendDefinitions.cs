
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Native bank-$89 room-FX blend selectors, distinct from editable colors.</summary>
public static class RoomFxPaletteBlendDefinitions
{
    /// <summary>The colours one palette blend writes.</summary>
    public const int ColorCount = 3;

    /// <summary>Editable JSON filename for room-FX blend inks and optional Ceres haze tints.</summary>
    public const string FileName = "room-fx-blend-palettes.json";
    /// <summary>Supported revision of the complete eight-selection blend schema; haze tint fields are optional within this revision.</summary>
    public const int Version = 1;

    /// <summary>Stock blue-channel fixed-color amplitude for Ceres haze.</summary>
    public static Bgr555 StockCeresHazeBlue => StockCeresHaze(CeresHazeColor.Blue);
    /// <summary>Stock red-channel fixed-color amplitude after Ceres Ridley.</summary>
    public static Bgr555 StockCeresHazeRed => StockCeresHaze(CeresHazeColor.Red);

    /// <summary>
    /// Native $88:DE10/DE15 selects COLDATA blue/red; DE42 stops fade-in at counter16,
    /// after counter15 was the final written amplitude. Calculate that selected RGB
    /// axis directly instead of retaining two default tint records.
    /// </summary>
    public static Bgr555 StockCeresHaze(CeresHazeColor color) => StockCeresHazeTint(color == CeresHazeColor.Red);

    /// <summary>The same native tint as a colour value, without constructing a per-scanline record.</summary>
    internal static Bgr555 StockCeresHazeTint(bool ridleyIsDead) =>
        new(ridleyIsDead ? CeresHazeDefinitions.FadeSteps - 1 : 0, 0,
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
        RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend ${(byte)id:X2} is not catalogued."),
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
            RoomFxPaletteBlend.None => throw new InvalidOperationException($"Undefined RoomFxPaletteBlend {id}."),
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
            RoomFxPaletteBlend.None => throw new InvalidOperationException($"Undefined RoomFxPaletteBlend {id}."),
            _ => throw new InvalidOperationException($"Undefined RoomFxPaletteBlend {id}."),
        };
    }
}

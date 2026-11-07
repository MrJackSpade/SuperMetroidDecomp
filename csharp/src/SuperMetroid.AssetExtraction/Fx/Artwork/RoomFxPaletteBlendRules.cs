using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// The room-FX palette blends' rules: the colour channels blends share or calculate, which the import reads to separate a
/// blend's edits from its calculated channels.
/// </summary>
public static class RoomFxPaletteBlendRules
{
    /// <summary>Requires one of the eight authored nonzero palette blend selectors.</summary>
    private static void ValidateSelector(RoomFxPaletteBlend id)
    {
        if (id is not (RoomFxPaletteBlend.Lava or RoomFxPaletteBlend.LandingSiteRain or RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.WaterAndAcid or RoomFxPaletteBlend.Fog or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterC or RoomFxPaletteBlend.MaridiaWaterD))
            throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued.");
    }
    /// <summary>
    /// The six liquid blends use black as their third color. Weather's independent
    /// third colors have no calculated value here. Unknown selectors still reject.
    /// </summary>
    public static RgbColor? CalculatedThirdColor(RoomFxPaletteBlend id) => id switch
    {
        RoomFxPaletteBlend.Lava or RoomFxPaletteBlend.MaridiaWaterA or RoomFxPaletteBlend.WaterAndAcid or RoomFxPaletteBlend.MaridiaWaterB or RoomFxPaletteBlend.MaridiaWaterC or RoomFxPaletteBlend.MaridiaWaterD => RgbColor.Black,
        RoomFxPaletteBlend.LandingSiteRain or RoomFxPaletteBlend.Fog => null,
        RoomFxPaletteBlend.None => throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued."),
        _ => throw new InvalidDataException($"Room-FX palette blend {id} is not catalogued."),
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

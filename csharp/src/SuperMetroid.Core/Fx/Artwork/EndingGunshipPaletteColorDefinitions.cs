using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Original $8D:D6BA two-stage gunship reveal. Frames0..7 interpolate
/// white to dim with truncation; frames8..15 interpolate dim to bright with nearest
/// rounding. Both have seven intervals. Dim channels are floor(bright*2/7), except
/// frame7's final color, an independent boundary input. All256 original words confirm
/// these rules. Supplied bright colors and independent edits remain presentation data.</summary>
/// <remarks>The ending map uses source bytes0..2FF from$96:FE69 and blank tile8C
/// elsewhere. Original$95:A82F character pixels selected by that map never reference
/// palette5 slots0 or15. Retain only their specific source payloads (bright0000/0000,
/// first-stage boundary0404) under #1165's nonsense exception: no visible color rule
/// determines them, and reciting them would disguise the same data. Their temporal
/// values remain calculated. Visible shared channels and shade arithmetic are calculated
/// by EndingGunshipPaletteInputView; its remaining independent artwork parameters have
/// a separate, narrowly documented nonsense disposition.</remarks>
internal static class EndingGunshipPaletteColorDefinitions
{

    /// <summary>Defaults are calculated only after explicit supplied edits have been
    /// checked. Endpoint resolution has one bounded level: white/dim depend at most on
    /// the supplied bright row. No generated palette row is stored.</summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
    {
        value = RgbColor.Black;
        if (!ZebesExplosionGunshipPaletteFxColorDefinitions.Rows.TryLocate(key, out int frame, out int color) || frame == 15 || (frame == 7 && color == 15))
            return false;
        if (frame == 0)
        {
            value = color == 0 ? RgbColor.Black : RgbColor.FromWord(0x7fff);
            return true;
        }
        if (frame is 7 or 8)
        {
            if (!colors.TryGetValue(ZebesExplosionGunshipPaletteFxColorDefinitions.Color(15, color), out RgbColor bright))
                return false;
            value = Pack((bright.Red) * 2 / 7, (bright.Green) * 2 / 7, (bright.Blue) * 2 / 7);
            return true;
        }
        int first = frame < 8 ? 0 : 8, last = frame < 8 ? 7 : 15;
        if (!Endpoint(first, color, colors, out RgbColor a) || !Endpoint(last, color, colors, out RgbColor b)) return false;
        int t = frame - first, bias = frame < 8 ? 0 : 3;
        value = Pack(((a.Red) * (7 - t) + (b.Red) * t + bias) / 7,
            ((a.Green) * (7 - t) + (b.Green) * t + bias) / 7,
            ((a.Blue) * (7 - t) + (b.Blue) * t + bias) / 7);
        return true;
    }

    /// <summary>Uses an explicit edited endpoint when present, otherwise calculates the frame's shared color rule.</summary>
    private static bool Endpoint(int frame, int color, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> colors, out RgbColor value)
    {
        PaletteFxColorKey key = ZebesExplosionGunshipPaletteFxColorDefinitions.Color(frame, color);
        return colors.TryGetValue(key, out value) || TryCalculatedColor(key, colors, out value);
    }
    /// <summary>Forms one RGB5 value from independently interpolated gunship color channels.</summary>
    private static RgbColor Pack(int red, int green, int blue) => new RgbColor(red, green, blue);
}

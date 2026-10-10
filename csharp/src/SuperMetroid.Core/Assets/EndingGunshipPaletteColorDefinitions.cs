using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Mutually exclusive original gunship palette slots with shared channel rules.</summary>
internal enum EndingGunshipPaletteInk
{
    /// <summary>$8D:D8DE, upper gold highlight.</summary>
    Highlight = 1,
    /// <summary>$8D:D8E2, deep gold shadow with the hull ramp's shared hue.</summary>
    DeepShadow = 3,
    /// <summary>$8D:D8E4, black outline/detail ink.</summary>
    BlackDetail = 4,
    /// <summary>$8D:D8E6, light gold hull ink.</summary>
    HullLight = 5,
    /// <summary>$8D:D8E8, light member of the three-shade gold ramp.</summary>
    HullShadeLight = 6,
    /// <summary>$8D:D8EA, middle member of the three-shade gold ramp.</summary>
    HullShadeMiddle = 7,
    /// <summary>$8D:D8EC, dark member of the three-shade gold ramp.</summary>
    HullShadeDark = 8,
    /// <summary>$8D:D8EE, green cockpit light.</summary>
    CockpitLight = 9,
    /// <summary>$8D:D8F0, middle cockpit shade.</summary>
    CockpitMiddle = 10,
    /// <summary>$8D:D8F2, dark cockpit shade.</summary>
    CockpitDark = 11,
    /// <summary>$8D:D8F4, light blue-grey underside.</summary>
    UndersideLight = 12,
    /// <summary>$8D:D8F6, middle blue-grey underside.</summary>
    UndersideMiddle = 13,
    /// <summary>$8D:D8F8, dark blue-grey underside.</summary>
    UndersideDark = 14,
}

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
    internal static bool TryCoordinates(ushort pointer, out int frame, out int color)
    {
        frame = color = 0;
        int offset = pointer - (ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FirstFramePointer + Bgr555.ByteCount);
        if ((uint)offset >= ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount *
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameByteCount) return false;
        int within = offset % ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameByteCount;
        if ((within & 1) != 0 || within >= 16 * Bgr555.ByteCount) return false;
        frame = offset / ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameByteCount;
        color = within / Bgr555.ByteCount;
        return true;
    }

    /// <summary>Defaults are calculated only after explicit supplied edits have been
    /// checked. Endpoint resolution has one bounded level: white/dim depend at most on
    /// the supplied bright row. No generated palette row is stored.</summary>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, Bgr555> colors, out Bgr555 value)
    {
        value = Bgr555.Black;
        if (!TryCoordinates(pointer, out int frame, out int color) || frame == 15 || (frame == 7 && color == 15))
            return false;
        if (frame == 0)
        {
            value = color == 0 ? Bgr555.Black : Bgr555.FromWord(0x7fff);
            return true;
        }
        if (frame is 7 or 8)
        {
            if (!colors.TryGetValue(ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(15, color), out Bgr555 bright))
                return false;
            value = Pack((bright.Red) * 2 / 7, (bright.Green) * 2 / 7, (bright.Blue) * 2 / 7);
            return true;
        }
        int first = frame < 8 ? 0 : 8, last = frame < 8 ? 7 : 15;
        if (!Endpoint(first, color, colors, out Bgr555 a) || !Endpoint(last, color, colors, out Bgr555 b)) return false;
        int t = frame - first, bias = frame < 8 ? 0 : 3;
        value = Pack(((a.Red) * (7 - t) + (b.Red) * t + bias) / 7,
            ((a.Green) * (7 - t) + (b.Green) * t + bias) / 7,
            ((a.Blue) * (7 - t) + (b.Blue) * t + bias) / 7);
        return true;
    }

    private static bool Endpoint(int frame, int color, IReadOnlyDictionary<ushort, Bgr555> colors, out Bgr555 value)
    {
        ushort pointer = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
        return colors.TryGetValue(pointer, out value) || TryCalculatedColor(pointer, colors, out value);
    }
    private static Bgr555 Pack(int red, int green, int blue) => new Bgr555(red, green, blue);
}

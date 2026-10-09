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
    /// <summary>Maps an aligned color-word pointer in the sixteen-record reveal to its frame and color index.</summary>
    /// <param name="pointer">Palette pointer to classify; record durations, waits, and odd byte addresses are not colors.</param>
    /// <param name="frame">Receives the zero-based reveal frame when the pointer identifies a color word.</param>
    /// <param name="color">Receives the zero-based color within that frame when the pointer identifies a color word.</param>
    /// <returns><see langword="true"/> for one of the sixteen color words in a timed record; otherwise <see langword="false"/>.</returns>
    internal static bool TryCoordinates(ushort pointer, out int frame, out int color)
    {
        frame = color = 0;
        int offset = pointer - (ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FirstFramePointer + sizeof(ushort));
        if ((uint)offset >= ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameCount *
            ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameByteCount) return false;
        int within = offset % ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameByteCount;
        if ((within & 1) != 0 || within >= 16 * sizeof(ushort)) return false;
        frame = offset / ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.FrameByteCount;
        color = within / sizeof(ushort);
        return true;
    }

    /// <summary>Defaults are calculated only after explicit supplied edits have been
    /// checked. Endpoint resolution has one bounded level: white/dim depend at most on
    /// the supplied bright row. No generated palette row is stored.</summary>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        if (!TryCoordinates(pointer, out int frame, out int color) || frame == 15 || (frame == 7 && color == 15))
            return false;
        if (frame == 0)
        {
            value = color == 0 ? (ushort)0 : (ushort)0x7fff;
            return true;
        }
        if (frame is 7 or 8)
        {
            if (!colors.TryGetValue(ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(15, color), out ushort bright))
                return false;
            value = Pack((bright & 31) * 2 / 7, (bright >> 5 & 31) * 2 / 7, (bright >> 10 & 31) * 2 / 7);
            return true;
        }
        int first = frame < 8 ? 0 : 8, last = frame < 8 ? 7 : 15;
        if (!Endpoint(first, color, colors, out ushort a) || !Endpoint(last, color, colors, out ushort b)) return false;
        int t = frame - first, bias = frame < 8 ? 0 : 3;
        value = Pack(((a & 31) * (7 - t) + (b & 31) * t + bias) / 7,
            ((a >> 5 & 31) * (7 - t) + (b >> 5 & 31) * t + bias) / 7,
            ((a >> 10 & 31) * (7 - t) + (b >> 10 & 31) * t + bias) / 7);
        return true;
    }

    /// <summary>Resolves an interpolation endpoint from supplied palette edits, falling back to the calculated color rules.</summary>
    /// <param name="frame">Reveal frame containing the endpoint.</param>
    /// <param name="color">Color index within the endpoint frame.</param>
    /// <param name="colors">Supplied native-pointer color values, which take precedence over calculation.</param>
    /// <param name="value">Receives the endpoint color when either source can resolve it.</param>
    /// <returns><see langword="true"/> when the endpoint is supplied or calculated; otherwise <see langword="false"/>.</returns>
    private static bool Endpoint(int frame, int color, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        ushort pointer = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
        return colors.TryGetValue(pointer, out value) || TryCalculatedColor(pointer, colors, out value);
    }
    /// <summary>Packs red, green, and blue five-bit channels into the SNES BGR555 palette-word layout.</summary>
    /// <param name="red">Five-bit red channel value.</param>
    /// <param name="green">Five-bit green channel value.</param>
    /// <param name="blue">Five-bit blue channel value.</param>
    /// <returns>The packed 15-bit palette word.</returns>
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}

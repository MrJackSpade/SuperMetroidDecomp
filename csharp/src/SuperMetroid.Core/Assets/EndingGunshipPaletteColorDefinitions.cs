using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

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
/// values remain calculated. Visible base colors require their separate review.</remarks>
internal static class EndingGunshipPaletteColorDefinitions
{
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

    private static bool Endpoint(int frame, int color, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        ushort pointer = ZebesExplosionGunshipPaletteFxProgramMechanicsDefinitions.ColorPointer(frame, color);
        return colors.TryGetValue(pointer, out value) || TryCalculatedColor(pointer, colors, out value);
    }
    private static ushort Pack(int red, int green, int blue) => (ushort)(red | green << 5 | blue << 10);
}

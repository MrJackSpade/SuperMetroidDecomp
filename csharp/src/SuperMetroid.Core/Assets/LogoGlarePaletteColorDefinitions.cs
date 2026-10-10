using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Assets;

/// <summary>Original $8D:DF94 logo glare colors: seven equal RGB5 intervals to
/// white followed by the mirrored return. For frame0..13, t is frame+1 up to6
/// and13-frame thereafter. Each channel is floor((base*(7-t)+31*t)/7).
/// All224 original words independently confirm this interpolation. The final
/// row remains supplied base artwork. Its fifteen opaque words equal the original
/// logo palette $8C:EFEB..F008: selected yellow, blue and orange drawing inks, whose
/// numerical recitation would only disguise the artwork. Slot0 instead holds$21A8;
/// OBJ index0 is transparent before CGRAM lookup, so this source payload has no
/// visible color-generation rule. Both are retained narrowly under #1165's nonsense
/// exception; no intermediate animation color is included in that disposition.</summary>
internal static class LogoGlarePaletteColorDefinitions
{
    /// <summary>Maps an in-range, word-aligned glare color pointer to its animation frame and color slot.</summary>
    internal static bool TryCoordinates(ushort pointer, out int frame, out int color)
    {
        frame = color = 0;
        int offset = pointer - (PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FirstFramePointer + sizeof(ushort));
        if ((uint)offset >= PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameCount *
            PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameByteCount) return false;
        int within = offset % PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameByteCount;
        if ((within & 1) != 0 || within >= PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorsPerFrame * sizeof(ushort))
            return false;
        frame = offset / PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.FrameByteCount;
        color = within / sizeof(ushort);
        return true;
    }

    /// <summary>Calculate only intermediate colors from the final supplied row.
    /// The caller resolves explicit per-frame edits first. No generated row is cached.</summary>
    internal static bool TryCalculatedColor(ushort pointer, IReadOnlyDictionary<ushort, ushort> colors, out ushort value)
    {
        value = 0;
        if (!TryCoordinates(pointer, out int frame, out int color) || frame == 13 ||
            !colors.TryGetValue(PostCreditsIconGlarePaletteFxProgramMechanicsDefinitions.ColorPointer(13, color), out ushort basis))
            return false;
        int t = frame < 7 ? frame + 1 : 13 - frame;
        int red = ((basis & 31) * (7 - t) + 31 * t) / 7;
        int green = (((basis >> 5) & 31) * (7 - t) + 31 * t) / 7;
        int blue = (((basis >> 10) & 31) * (7 - t) + 31 * t) / 7;
        value = (ushort)(red | green << 5 | blue << 10);
        return true;
    }
}

using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// Dachora's installed colours: the speed and shine frames calculated from the normal palette, how each colour resolves
/// from its edit or that calculation, and the identity of all of them.
/// </summary>
internal static class DachoraColors
{
    /// <summary>
    /// Frame k raises each color's blue channel toward min(blue + 16, 31),
    /// rounding to nearest over three steps; red and green keep the normal color. The first
    /// frame's transparent slot is the authored flash backdrop, later frames keep the normal one.
    /// </summary>
    internal static RgbColor SpeedColor(RgbColor[] normal, int frame, int color)
    {
        RgbColor basis = normal[color];
        if (color == 0) return frame == 0 ? DachoraColorCatalog.SpeedFlashBackdrop : basis;
        int blue = basis.Blue;
        int target = Math.Min(blue + 16, 31);
        int steps = DachoraColorDefinitions.AnimatedFrameCount - 1;
        int shifted = blue + ((target - blue) * frame + steps / 2) / steps;
        return basis.WithBlue(shifted);
    }

    /// <summary>
    /// Frame k fades every channel toward white by k/5, as
    /// channel + floor(k * (31 - channel) / 5); the transparent slot keeps the normal color.
    /// </summary>
    internal static RgbColor ShineColor(RgbColor[] normal, int frame, int color)
    {
        RgbColor basis = normal[color];
        if (color == 0) return basis;
        return basis.Map((_, channel) => channel + frame * (31 - channel) / 5);
    }

    /// <summary>Resolves one selected Dachora color, preserving every supplied speed/shine deviation.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase/frame combination is unsupported or the color index is outside the palette.</exception>
    internal static RgbColor Resolve(DachoraColorCatalog colors, DachoraColourPhase phase, int frame, int color)
    {
        bool animated = phase is DachoraColourPhase.Speed or DachoraColourPhase.Shine;
        if (!(phase == DachoraColourPhase.Default && frame == 0) &&
            !(animated && (uint)frame < DachoraColorDefinitions.AnimatedFrameCount))
            throw new ArgumentOutOfRangeException(nameof(frame), $"Dachora phase {phase} has no frame {frame}.");
        if ((uint)color >= DachoraColorFormat.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourStep(frame, color);
        return phase switch
        {
            DachoraColourPhase.Speed => colors.selection.SpeedEdits.TryGetValue(key, out RgbColor speed) ? speed : SpeedColor(colors.selection.Normal, frame, color),
            DachoraColourPhase.Shine => colors.selection.ShineEdits.TryGetValue(key, out RgbColor shine) ? shine : ShineColor(colors.selection.Normal, frame, color),
            DachoraColourPhase.Default => colors.selection.Normal[color],
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined Dachora palette phase."),
        };
    }
}

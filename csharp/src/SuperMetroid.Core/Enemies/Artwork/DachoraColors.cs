using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Animals;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Dachora's installed colours: the speed and shine frames calculated from the normal palette, how each colour resolves
/// from its edit or that calculation, and the identity of all of them.
/// </summary>
internal static class DachoraColors
{
    /// <summary>Hashes the normal palette, then the speed and shine frames.</summary>
    internal static string ContentIdentity(DachoraColorCatalog colors) =>
        SelectedPresentationHash.Create("DachoraColorCatalog-v1", content =>
        {
            content.AppendColors("normal", colors.selection.Normal);
            content.AppendColorFrames("speed", Frames(colors, DachoraPalettePhase.Speed));
            content.AppendColorFrames("shine", Frames(colors, DachoraPalettePhase.Shine));
        });

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

    /// <summary>Materializes four selected animated frames for stable content hashing.</summary>
    private static RgbColor[][] Frames(DachoraColorCatalog colors, DachoraPalettePhase phase) =>
        Enumerable.Range(0, DachoraColorDefinitions.AnimatedFrameCount)
            .Select(frame => Enumerable.Range(0, DachoraColorDefinitions.ColorsPerFrame)
                .Select(color => Resolve(colors, phase, frame, color)).ToArray()).ToArray();

    /// <summary>Resolves one selected Dachora color, preserving every supplied speed/shine deviation.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The phase/frame combination is unsupported or the color index is outside the palette.</exception>
    internal static RgbColor Resolve(DachoraColorCatalog colors, DachoraPalettePhase phase, int frame, int color)
    {
        bool animated = phase is DachoraPalettePhase.Speed or DachoraPalettePhase.Shine;
        if (!(phase == DachoraPalettePhase.Default && frame == 0) &&
            !(animated && (uint)frame < DachoraColorDefinitions.AnimatedFrameCount))
            throw new ArgumentOutOfRangeException(nameof(frame), $"Dachora phase {phase} has no frame {frame}.");
        if ((uint)color >= DachoraColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        var key = new ColourCell(frame, color);
        return phase switch
        {
            DachoraPalettePhase.Speed => colors.selection.SpeedEdits.TryGetValue(key, out RgbColor speed) ? speed : SpeedColor(colors.selection.Normal, frame, color),
            DachoraPalettePhase.Shine => colors.selection.ShineEdits.TryGetValue(key, out RgbColor shine) ? shine : ShineColor(colors.selection.Normal, frame, color),
            DachoraPalettePhase.Default => colors.selection.Normal[color],
            _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined Dachora palette phase."),
        };
    }
}

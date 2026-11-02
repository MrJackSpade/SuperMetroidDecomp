using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Recognizes the cutscene Baby's and the live Shitroid's supplied colours as the reviewed paint and its fades, keeping
/// supplied colours only where the calculation does not reproduce them.
/// </summary>
internal static class BabyAndShitroidColorRecognition
{
    /// <summary>Selects the cutscene Baby's initial image and death fade.</summary>
    internal static BabyMetroidCutsceneColorSelection Cutscene(RgbColor[] initial, RgbColor[][] fade) =>
        new(Initial(initial), Fade(fade));

    /// <summary>
    /// Selects the live Shitroid's pulse and targets, separating the transparent Shitroid palette word from the visible
    /// inks the pulse may share.
    /// </summary>
    internal static ShitroidColorSelection Shitroid(RgbColor[][] normal, RgbColor[] sidehopper, RgbColor[] shitroid,
        RgbColor[] deadSidehopper)
    {
        BabyMetroidInitialPalette target = Initial(shitroid.AsSpan(1).ToArray());
        return new(Pulse(normal, target), SidehopperPaletteRecognition.Initial(sidehopper), target, shitroid[0],
            SidehopperPaletteRecognition.Corpse(deadSidehopper));
    }

    /// <summary>Uses calculated Baby colors when all fifteen supplied entries agree, otherwise keeps this palette's edits.</summary>
    private static BabyMetroidInitialPalette Initial(RgbColor[] colors)
    {
        for (int color = 0; color < colors.Length; color++)
            if (BabyMetroidInitialPalette.Calculate(color) != colors[color])
                return new(colors.ToArray());
        return new(supplied: null);
    }

    /// <summary>Uses the composition for stock frames and retains any independently authored edits.</summary>
    private static BabyMetroidFade Fade(RgbColor[][] frames)
    {
        for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < frames[frame].Length; color++)
                if (BabyMetroidFade.Calculate(frame, color) != frames[frame][color])
                    return new(frames.Select(fadeFrame => fadeFrame.ToArray()).ToArray());
        return new(supplied: null);
    }

    /// <summary>
    /// Compares each supplied ink with the target-derived pulse before deciding whether the phases can be shared: sharing
    /// the target's innard colors is valid only when every supplied pulse sample agrees.
    /// </summary>
    private static ShitroidColorPulse Pulse(RgbColor[][] frames, BabyMetroidInitialPalette target)
    {
        var shared = new ShitroidColorPulse(target, supplied: null);
        for (int frame = 0; frame < frames.Length; frame++)
            for (int color = 0; color < ShitroidColorDefinitions.NormalColorsPerFrame; color++)
                if (shared.Resolve(frame, color) != frames[frame][color])
                    return new(target, frames);
        return shared;
    }
}

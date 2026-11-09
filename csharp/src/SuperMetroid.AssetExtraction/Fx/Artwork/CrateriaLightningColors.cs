using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Fx.Artwork.CrateriaLightningColorDefinitions;

using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.Artwork;

/// <summary>
/// Calculates the Crateria lightning palette effect's colours (<see cref="CrateriaLightningColorDefinitions"/>) that no supplied
/// edit replaces.
/// </summary>
internal static class CrateriaLightningColors
{
    /// <summary>
    /// Four equally spaced level brightness phases and a white crest.
    /// Record phases ascend zero through four, descend to zero, then descend four to one.
    /// Each nonwhite frame darkens by one level per color position.
    /// </summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, out RgbColor color)
    {
        color = RgbColor.Black;
        if (!SurfaceProgram.ColorFrames.TryLocate(key, out int frame, out int index))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : 13 - frame;
        // Colour words follow a base plus a per-phase step minus a per-position step: channels (12, 11, 11) rise
        // six per phase and fall one per position, never borrowing between channels.
        int level = 6 * phase - index;
        color = phase == 4 ? RgbColor.White : new FadeLevelColour(12 + level, 11 + level, 11 + level).ToColour();
        return true;
    }

    /// <summary>
    /// Each dark-lightning channel decreases five levels per phase,
    /// clamping at zero. Seven first-frame colors remain supplied inputs; other independent
    /// samples are retained whenever they differ from the supplied-base calculation.
    /// </summary>
    internal static bool TryCalculatedDarkColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> inputs, out RgbColor color)
    {
        color = RgbColor.Black;
        if (!DarkProgram.ColorFrames.TryLocate(key, out int frame, out int index) || frame == 0 ||
            !inputs.TryGetValue(DarkProgram.Color(0, index), out RgbColor first))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : frame == 9 ? 0 : 14 - frame;
        int red = Math.Max(0, (first.FadeLevels.Red) - 5 * phase);
        int green = Math.Max(0, (first.FadeLevels.Green) - 5 * phase);
        int blue = Math.Max(0, (first.FadeLevels.Blue) - 5 * phase);
        color = new FadeLevelColour(red, green, blue).ToColour();
        return true;
    }
}

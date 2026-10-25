using SuperMetroid.Core.Fx.PaletteFx;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Fx.Artwork;

/// <summary>Calculated surface-lightning colors belonging to the Crateria lightning object.</summary>
internal static class CrateriaLightningColorDefinitions
{
    /// <summary>Thirteen surface-lightning records, each containing eight colors.</summary>
    internal static CrateriaLightningPaletteFxColorDefinition SurfaceProgram =>
        CrateriaLightningPaletteFxColorDefinitions.All[0];

    /// <summary>
    /// Four equally spaced RGB5 brightness phases and a white crest.
    /// Record phases ascend zero through four, descend to zero, then descend four to one.
    /// Each nonwhite row darkens by one RGB5 unit per color position.
    /// </summary>
    internal static bool TryCalculatedColor(PaletteFxColorKey key, out RgbColor color)
    {
        color = RgbColor.Black;
        if (!SurfaceProgram.Rows.TryLocate(key, out int frame, out int index))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : 13 - frame;
        // Native words $2D6C + $18C6 * phase - $0421 * index: channels (12, 11, 11) rise
        // six per phase and fall one per position, never borrowing between channels.
        int level = 6 * phase - index;
        color = phase == 4 ? RgbColor.White : new RgbColor(12 + level, 11 + level, 11 + level);
        return true;
    }

    /// <summary>The unused dark-lightning program: fourteen seven-color records.</summary>
    internal static CrateriaLightningPaletteFxColorDefinition DarkProgram =>
        CrateriaLightningPaletteFxColorDefinitions.All[1];

    /// <summary>
    /// Each dark-lightning channel decreases five RGB5 units per phase,
    /// clamping at zero. Seven first-row colors remain supplied inputs; other independent
    /// samples are retained whenever they differ from the supplied-base calculation.
    /// </summary>
    internal static bool TryCalculatedDarkColor(PaletteFxColorKey key, IReadOnlyDictionary<PaletteFxColorKey, RgbColor> inputs, out RgbColor color)
    {
        color = RgbColor.Black;
        if (!DarkProgram.Rows.TryLocate(key, out int frame, out int index) || frame == 0 ||
            !inputs.TryGetValue(DarkProgram.Color(0, index), out RgbColor first))
            return false;
        int phase = frame < 9 ? 4 - Math.Abs(4 - frame) : frame == 9 ? 0 : 14 - frame;
        int red = Math.Max(0, (first.Red) - 5 * phase);
        int green = Math.Max(0, (first.Green) - 5 * phase);
        int blue = Math.Max(0, (first.Blue) - 5 * phase);
        color = new RgbColor(red, green, blue);
        return true;
    }
}

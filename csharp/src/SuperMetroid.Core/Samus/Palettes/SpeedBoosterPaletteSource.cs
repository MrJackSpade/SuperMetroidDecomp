using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>
/// What one active Speed Booster cycle phase copies into Samus's suit colors: a cycle palette, or, for Gravity's two
/// phases past its list, the instruction bytes that follow the lists.
/// </summary>
internal readonly record struct SpeedBoosterPaletteSource
{
    /// <summary>Holds exactly one of a cycle palette or an overrun.</summary>
    private SpeedBoosterPaletteSource(SamusFullBodyCyclePalette? palette, SpeedBoosterPaletteOverrun? overrun)
    {
        Palette = palette;
        Overrun = overrun;
    }

    /// <summary>The cycle palette the phase copies, or null when it reads an overrun.</summary>
    internal SamusFullBodyCyclePalette? Palette { get; }

    /// <summary>The overrun the phase reads, or null when it copies a cycle palette.</summary>
    internal SpeedBoosterPaletteOverrun? Overrun { get; }

    /// <summary>A phase that copies <paramref name="palette"/>.</summary>
    internal static SpeedBoosterPaletteSource Of(SamusFullBodyCyclePalette palette) => new(palette, null);

    /// <summary>A phase that reads <paramref name="overrun"/>.</summary>
    internal static SpeedBoosterPaletteSource Of(SpeedBoosterPaletteOverrun overrun) => new(null, overrun);
}

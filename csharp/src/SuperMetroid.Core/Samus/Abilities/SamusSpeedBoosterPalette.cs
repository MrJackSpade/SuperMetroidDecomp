using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>Applies installed Speed Booster cycle colors, or the colours Gravity reads past its cycle, to Samus's suit palette.</summary>
internal static class SamusSpeedBoosterPalette
{
    /// <summary>
    /// Copies the selected installed cycle into the suit colours. A step past Gravity's cycle instead copies the colours
    /// <paramref name="overrunColours"/> decoded for it.
    /// </summary>
    internal static void Apply(SamusColours colours, SpeedBoosterPaletteSource source,
        SamusFullBodyCycleColours? colors, SpeedBoosterOverrunColours<SpeedBoosterPaletteOverrun, RgbColor> overrunColours)
    {
        if (source.Overrun is { } overrun)
        {
            IReadOnlyList<RgbColor> copied = overrunColours.Colours(overrun);
            if (copied.Count != SamusPaletteDefinitions.Common.ColoursPerRow)
                throw new InvalidDataException($"The Speed Booster step {overrun} copies {copied.Count} colours, not one suit row.");
            for (int index = 0; index < copied.Count; index++)
                colours.Suit.Write(index, copied[index]);
            return;
        }
        (colors ?? throw new InvalidOperationException(
            "Speed Booster palette requires installed Samus full-body cycle colors.")).Apply(colours, source.Palette ??
                throw new InvalidOperationException("A Speed Booster palette source names either a cycle palette or an overrun."));
    }
}

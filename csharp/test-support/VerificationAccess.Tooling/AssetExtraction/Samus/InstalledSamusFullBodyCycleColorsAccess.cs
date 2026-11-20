using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="InstalledSamusFullBodyCycleColors"/> members production does not use.</summary>
internal static class InstalledSamusFullBodyCycleColorsAccess
{
    /// <summary>Exposes <see cref="InstalledSamusFullBodyCycleColors"/> state that only verification reads.</summary>
    extension(InstalledSamusFullBodyCycleColors self)
    {
        /// <summary>Returns one installed colour of a cycle palette.</summary>
        /// <param name="palette">The cycle palette.</param>
        /// <param name="colorIndex">The colour within the palette, 0 through 15.</param>
        /// <returns>The installed colour.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The palette is not catalogued or the colour is outside it.</exception>
        public RgbColor Resolve(SamusFullBodyCycleShade palette, int colorIndex)
        {
            RgbColor[] colours = self.palettes.TryGetValue(palette, out RgbColor[]? found) && found is not null ? found
                : throw new ArgumentOutOfRangeException(nameof(palette), palette, "Uncatalogued full-body palette.");
            if ((uint)colorIndex >= colours.Length)
                throw new ArgumentOutOfRangeException(nameof(colorIndex));
            return colours[colorIndex];
        }
    }
}

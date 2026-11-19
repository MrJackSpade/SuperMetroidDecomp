using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="TorizoBellyPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class TorizoBellyPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<TorizoBellyPaletteOwner, ColourEffectId> CrossChecks = new()
    {
        [TorizoBellyPaletteOwner.BombTorizo] = ColourEffectId.BombTorizoBelly,
        [TorizoBellyPaletteOwner.GoldenTorizo] = ColourEffectId.GoldenTorizoBelly,
    };

    /// <summary>Exposes <see cref="TorizoBellyPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(TorizoBellyPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner];
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}

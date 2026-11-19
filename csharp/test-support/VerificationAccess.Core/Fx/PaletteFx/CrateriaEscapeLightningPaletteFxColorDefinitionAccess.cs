using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="CrateriaEscapeLightningPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class CrateriaEscapeLightningPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<CrateriaEscapeLightningPaletteOwner, (ColourEffectId Object, int ColorsPerFrame)> CrossChecks = new()
    {
        [CrateriaEscapeLightningPaletteOwner.YellowLightning] = (ColourEffectId.CrateriaEscapeYellowLightning, 11),
        [CrateriaEscapeLightningPaletteOwner.ParlorSky] = (ColourEffectId.ParlorEscapeSkyFlicker, 5),
    };

    /// <summary>Exposes <see cref="CrateriaEscapeLightningPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(CrateriaEscapeLightningPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner].Object;
        /// <summary>The number of live BGR555 colors in every record.</summary>
        internal int ColorsPerFrame => CrossChecks[self.Owner].ColorsPerFrame;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}

using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class NorfairEnvironmentalPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<NorfairEnvironmentalPaletteOwner, (ColourEffectId Object, bool PublishesHeatPhase)> CrossChecks = new()
    {
        [NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase] = (ColourEffectId.NorfairForegroundAndHeatPhase, true),
        [NorfairEnvironmentalPaletteOwner.GreenForeground] = (ColourEffectId.NorfairGreenForegroundHeatGlow, false),
        [NorfairEnvironmentalPaletteOwner.PurpleForeground] = (ColourEffectId.NorfairPurpleForegroundHeatGlow, false),
        [NorfairEnvironmentalPaletteOwner.DarkForeground] = (ColourEffectId.NorfairDarkForegroundHeatGlow, false),
    };

    /// <summary>Exposes <see cref="NorfairEnvironmentalPaletteFxColorDefinition"/> state that only verification reads.</summary>
    extension(NorfairEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>The palette-FX object that installs this program.</summary>
        internal ColourEffectId Object => CrossChecks[self.Owner].Object;
        /// <summary>Whether each record begins with a byte-sized heat-phase publication.</summary>
        internal bool PublishesHeatPhase => CrossChecks[self.Owner].PublishesHeatPhase;
        /// <summary>Bytes of the heat-phase publication before each record's timer word; zero outside the heat program.</summary>
        public int HeatPublicationByteCount => self.PublishesHeatPhase ? 3 : 0;
        /// <summary>Bytes from one record's first command through its wait command.</summary>
        public int FrameByteCount => 16 + self.HeatPublicationByteCount;
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}

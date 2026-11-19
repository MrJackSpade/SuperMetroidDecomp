using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Verification access to <see cref="MaridiaEnvironmentalPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class MaridiaEnvironmentalPaletteFxColorDefinitionAccess
{
    /// <summary>
    /// What each program the cartridge holds that production does not read (#2570), by each program's owner: the checks compare
    /// each value against the cartridge.
    /// </summary>
    private static readonly Dictionary<MaridiaEnvironmentalPaletteOwner, (int FrameCount, int ColorsPerFrame)> CrossChecks = new()
    {
        [MaridiaEnvironmentalPaletteOwner.SandPits] = (MaridiaEnvironmentalPaletteFxColorDefinitions.SandRotationColors, MaridiaEnvironmentalPaletteFxColorDefinitions.SandRotationColors * MaridiaEnvironmentalPaletteFxColorDefinitions.SandPitBandCount),
        [MaridiaEnvironmentalPaletteOwner.SandFalls] = (MaridiaEnvironmentalPaletteFxColorDefinitions.SandRotationColors, MaridiaEnvironmentalPaletteFxColorDefinitions.SandRotationColors),
        [MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls] = (MaridiaEnvironmentalPaletteFxColorDefinitions.WaterfallRotationColors, MaridiaEnvironmentalPaletteFxColorDefinitions.WaterfallRotationColors),
    };

    /// <summary>Exposes Maridia sand-pit palette-FX identity and its frame-count-by-duration cycle.</summary>
    extension(MaridiaEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>A one-color cyclic rotation returns after the named group size.</summary>
        internal int FrameCount => CrossChecks[self.Owner].FrameCount;
        /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
        internal int ColorsPerFrame => CrossChecks[self.Owner].ColorsPerFrame;
        /// <summary>Native four-byte definition identity selected by environmental owner.</summary>
        internal ushort DefinitionPointer => (ushort)(MaridiaEnvironmentalPaletteFxColorDefinitions.SandPitDefinition + (int)self.Owner * 4);

        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames => self.FrameCount * self.Duration;
        /// <summary>The colour group the definition's object animates.</summary>
        internal ColourEffectColours AnimatedColours => PaletteFxObjectAccess.FromNative(self.DefinitionPointer).AnimatedColours;
        /// <summary>The authored cadence for sand or waterfall animation.</summary>
        public ushort Duration => self.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
            ? MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.WaterfallDuration
            : MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandDuration;
        /// <summary>Bytes from a duration word through its terminal wait command.</summary>
        public int FrameByteCount =>
            sizeof(ushort) + self.ColorsPerFrame * sizeof(ushort) + sizeof(ushort);
        /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
        public PaletteFxColorKey Color(int frame, int color) => self.ColorFrames.Color(frame, color);
    }
}

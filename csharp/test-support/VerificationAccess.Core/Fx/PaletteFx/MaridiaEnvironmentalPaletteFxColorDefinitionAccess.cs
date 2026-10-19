
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Verification access to <see cref="MaridiaEnvironmentalPaletteFxColorDefinition"/> members production does not use.</summary>
internal static class MaridiaEnvironmentalPaletteFxColorDefinitionAccess
{
    /// <summary>Exposes Maridia sand-pit palette-FX identity and its frame-count-by-duration cycle.</summary>
    extension(MaridiaEnvironmentalPaletteFxColorDefinition self)
    {
        /// <summary>Native four-byte definition identity selected by environmental owner.</summary>
        internal ushort DefinitionPointer => (ushort)(MaridiaEnvironmentalPaletteFxColorDefinitions.SandPitDefinition + (int)self.Owner * 4);

        /// <summary>Frames from the first record through the next first record.</summary>
        internal int CycleFrames => self.FrameCount * self.Duration;
        /// <summary>Destination byte address from the named palette slot and first color.</summary>
        public ushort ColorByteIndex => (ushort)(sizeof(ushort) * (16 *
            (self.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
                ? MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.WaterfallPalette
                : MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandPalette)
            + (self.Owner == MaridiaEnvironmentalPaletteOwner.SandPits
                ? MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandPitFirstColor
                : self.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
                    ? MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.WaterfallFirstColor
                    : MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.FallingFirstColor)));
        /// <summary>The authored cadence for sand or waterfall animation.</summary>
        public ushort Duration => self.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
            ? MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.WaterfallDuration
            : MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandDuration;
        /// <summary>Bytes from a duration word through its terminal wait command.</summary>
        public int FrameByteCount =>
            sizeof(ushort) + self.ColorsPerFrame * sizeof(ushort) + sizeof(ushort);
    }
}

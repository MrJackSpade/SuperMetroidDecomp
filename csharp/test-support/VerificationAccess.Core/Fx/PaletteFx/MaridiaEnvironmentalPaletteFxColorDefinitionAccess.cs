
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
        /// <summary>The colour group the definition's object animates.</summary>
        internal PaletteFxColours AnimatedColours => PaletteFxObjectAccess.FromNative(self.DefinitionPointer).AnimatedColours;
        /// <summary>The authored cadence for sand or waterfall animation.</summary>
        public ushort Duration => self.Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
            ? MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.WaterfallDuration
            : MaridiaEnvironmentalPaletteFxColorDefinitionsConstants.SandDuration;
        /// <summary>Bytes from a duration word through its terminal wait command.</summary>
        public int FrameByteCount =>
            sizeof(ushort) + self.ColorsPerFrame * sizeof(ushort) + sizeof(ushort);
    }
}

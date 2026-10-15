
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
    }
}

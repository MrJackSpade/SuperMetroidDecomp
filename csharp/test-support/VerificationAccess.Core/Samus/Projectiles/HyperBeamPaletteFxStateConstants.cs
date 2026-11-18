namespace SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.ColourEffects;

/// <summary>Cartridge values of <see cref="HyperBeamColourEffectState"/> that only verification reads.</summary>
internal static class HyperBeamPaletteFxStateConstants
{
    /// <summary>Number of colors copied by each Hyper Beam record.</summary>
    public const int ColorsPerFrame = HyperBeamPaletteFxColorDefinitionsConstants.ColorsPerFrame;
    /// <summary>Number of timed color records in the Hyper Beam loop.</summary>
    public const int FrameCount = HyperBeamPaletteFxColorDefinitionsConstants.FrameCount;

    /// <summary>Provides the expected Hyper Beam palette frame count and colors per frame for state comparison.</summary>
    extension(HyperBeamColourEffectState)
    {
        /// <inheritdoc cref="HyperBeamPaletteFxStateConstants.ColorsPerFrame"/>
        internal static int ColorsPerFrame => HyperBeamPaletteFxStateConstants.ColorsPerFrame;
        /// <inheritdoc cref="HyperBeamPaletteFxStateConstants.FrameCount"/>
        internal static int FrameCount => HyperBeamPaletteFxStateConstants.FrameCount;
    }

    /// <summary>Reads the updates the effect's shade has left (#2397, #2458).</summary>
    extension(HyperBeamColourEffectState effect)
    {
        /// <summary>The updates the current shade has left, counting the update about to run; one before the first shade.</summary>
        /// <exception cref="InvalidOperationException">The effect has not started.</exception>
        internal int FrameHold => effect.Cycle is { } cycle
            ? cycle.UpdatesLeft
            : throw new InvalidOperationException("The Hyper Beam's colour effect has not started.");
    }
}

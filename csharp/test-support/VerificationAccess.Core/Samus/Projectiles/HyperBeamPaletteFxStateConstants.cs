namespace SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="HyperBeamPaletteFxState"/> that only verification reads.</summary>
internal static class HyperBeamPaletteFxStateConstants
{
    /// <summary>Number of colors copied by each Hyper Beam record.</summary>
    public const int ColorsPerFrame = HyperBeamPaletteFxColorDefinitionsConstants.ColorsPerFrame;
    /// <summary>Number of timed color records in the Hyper Beam loop.</summary>
    public const int FrameCount = HyperBeamPaletteFxColorDefinitionsConstants.FrameCount;

    /// <summary>Provides the expected Hyper Beam palette frame count and colors per frame for state comparison.</summary>
    extension(HyperBeamPaletteFxState)
    {
        /// <inheritdoc cref="HyperBeamPaletteFxStateConstants.ColorsPerFrame"/>
        internal static int ColorsPerFrame => HyperBeamPaletteFxStateConstants.ColorsPerFrame;
        /// <inheritdoc cref="HyperBeamPaletteFxStateConstants.FrameCount"/>
        internal static int FrameCount => HyperBeamPaletteFxStateConstants.FrameCount;
    }
}

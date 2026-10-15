
namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="HyperBeamPaletteFxProgramDefinitions"/> that only verification reads.</summary>
internal static class HyperBeamPaletteFxProgramDefinitionsConstants
{
    /// <summary>Number of BGR555 colors in each presentation payload.</summary>
    public const int ColorsPerFrame = 8;
    /// <summary>Native address of the destination-selection command at <c>$8D:D900</c>.</summary>
    public const int NativeEntryControlAddress = 0x8dd900;
    /// <summary>Native address of the first frame timer at <c>$8D:D904</c>.</summary>
    public const int NativeFirstFrameTimerAddress = 0x8dd904;
    /// <summary>Native address of the terminal loop command at <c>$8D:D9CC</c>.</summary>
    public const int NativeLoopControlAddress = 0x8dd9cc;

    /// <summary>Provides Hyper Beam palette frame width, timers, and native entry and loop controls for effect checks.</summary>
    extension(HyperBeamPaletteFxProgramDefinitions)
    {
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.ColorsPerFrame"/>
        internal static int ColorsPerFrame => HyperBeamPaletteFxProgramDefinitionsConstants.ColorsPerFrame;
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.NativeEntryControlAddress"/>
        internal static int NativeEntryControlAddress => HyperBeamPaletteFxProgramDefinitionsConstants.NativeEntryControlAddress;
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.NativeFirstFrameTimerAddress"/>
        internal static int NativeFirstFrameTimerAddress => HyperBeamPaletteFxProgramDefinitionsConstants.NativeFirstFrameTimerAddress;
        /// <inheritdoc cref="HyperBeamPaletteFxProgramDefinitionsConstants.NativeLoopControlAddress"/>
        internal static int NativeLoopControlAddress => HyperBeamPaletteFxProgramDefinitionsConstants.NativeLoopControlAddress;
    }
}

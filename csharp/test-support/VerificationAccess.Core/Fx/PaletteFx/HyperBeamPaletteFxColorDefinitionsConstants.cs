using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Cartridge values of <see cref="HyperBeamPaletteFxColorDefinitions"/> that only verification reads.</summary>
internal static class HyperBeamPaletteFxColorDefinitionsConstants
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
    extension(HyperBeamPaletteFxColorDefinitions)
    {
        /// <inheritdoc cref="HyperBeamPaletteFxColorDefinitionsConstants.ColorsPerFrame"/>
        internal static int ColorsPerFrame => HyperBeamPaletteFxColorDefinitionsConstants.ColorsPerFrame;
        /// <inheritdoc cref="HyperBeamPaletteFxColorDefinitionsConstants.NativeEntryControlAddress"/>
        internal static int NativeEntryControlAddress => HyperBeamPaletteFxColorDefinitionsConstants.NativeEntryControlAddress;
        /// <inheritdoc cref="HyperBeamPaletteFxColorDefinitionsConstants.NativeFirstFrameTimerAddress"/>
        internal static int NativeFirstFrameTimerAddress => HyperBeamPaletteFxColorDefinitionsConstants.NativeFirstFrameTimerAddress;
        /// <inheritdoc cref="HyperBeamPaletteFxColorDefinitionsConstants.NativeLoopControlAddress"/>
        internal static int NativeLoopControlAddress => HyperBeamPaletteFxColorDefinitionsConstants.NativeLoopControlAddress;
    }
    /// <summary>Bytes occupied by one timer, eight colors, and the done command.</summary>
    public const int FrameByteCount = 20;
    /// <summary>Number of authored timed color records.</summary>
    public const int FrameCount = 10;
    /// <summary>Fixed duration loaded by every authored color record.</summary>
    public const ushort FrameDuration = 2;
}

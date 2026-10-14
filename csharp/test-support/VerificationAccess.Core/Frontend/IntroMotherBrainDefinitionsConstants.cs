using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroMotherBrainDefinitions"/> that only verification reads.</summary>
internal static class IntroMotherBrainDefinitionsConstants
{
    /// <summary><c>$8B:B9CA</c>, five large-explosion initial instruction timers.</summary>
    public const int BigTimerReferenceAddress = 0x8bb9ca;
    /// <summary><c>$8B:B9B6</c>, five signed large-explosion X offsets.</summary>
    public const int BigXOffsetReferenceAddress = 0x8bb9b6;
    /// <summary><c>$8B:B9C0</c>, five signed large-explosion Y offsets.</summary>
    public const int BigYOffsetReferenceAddress = 0x8bb9c0;
    /// <summary>Bank containing the native cinematic-object definitions and placement tables.</summary>
    public const int NativeBank = 0x8b0000;
    /// <summary><c>$8B:BA09</c>, three small-explosion initial instruction timers.</summary>
    public const int SmallTimerReferenceAddress = 0x8bba09;
    /// <summary><c>$8B:B9FD</c>, three signed small-explosion X offsets.</summary>
    public const int SmallXOffsetReferenceAddress = 0x8bb9fd;
    /// <summary><c>$8B:BA03</c>, three signed small-explosion Y offsets.</summary>
    public const int SmallYOffsetReferenceAddress = 0x8bba03;

    /// <summary>Provides intro Mother Brain explosion offsets, timers, and definition bank for actor comparisons.</summary>
    extension(IntroMotherBrainDefinitions)
    {
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.BigTimerReferenceAddress"/>
        internal static int BigTimerReferenceAddress => IntroMotherBrainDefinitionsConstants.BigTimerReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.BigXOffsetReferenceAddress"/>
        internal static int BigXOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.BigXOffsetReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.BigYOffsetReferenceAddress"/>
        internal static int BigYOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.BigYOffsetReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => IntroMotherBrainDefinitionsConstants.NativeBank;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.SmallTimerReferenceAddress"/>
        internal static int SmallTimerReferenceAddress => IntroMotherBrainDefinitionsConstants.SmallTimerReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.SmallXOffsetReferenceAddress"/>
        internal static int SmallXOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.SmallXOffsetReferenceAddress;
        /// <inheritdoc cref="IntroMotherBrainDefinitionsConstants.SmallYOffsetReferenceAddress"/>
        internal static int SmallYOffsetReferenceAddress => IntroMotherBrainDefinitionsConstants.SmallYOffsetReferenceAddress;
    }
}

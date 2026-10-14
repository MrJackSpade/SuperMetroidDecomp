using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroEggMotionDefinitions"/> that only verification reads.</summary>
internal static class IntroEggMotionDefinitionsConstants
{
    /// <summary>
    /// <c>$8B:A97C</c>, the six interleaved shell-fragment X/Y origins before the
    /// cartridge adds its fixed $10/$3B placement biases.
    /// </summary>
    public const int InitialPositionReferenceAddress = 0x8ba97c;

    /// <summary>Provides the native initial-position reference for intro egg movement checks.</summary>
    extension(IntroEggMotionDefinitions)
    {
        /// <inheritdoc cref="IntroEggMotionDefinitionsConstants.InitialPositionReferenceAddress"/>
        internal static int InitialPositionReferenceAddress => IntroEggMotionDefinitionsConstants.InitialPositionReferenceAddress;
    }
}

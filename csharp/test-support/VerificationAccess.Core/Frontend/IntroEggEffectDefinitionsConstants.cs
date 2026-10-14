using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="IntroEggEffectDefinitions"/> that only verification reads.</summary>
internal static class IntroEggEffectDefinitionsConstants
{
    /// <summary>Bank containing the seven native cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    /// <summary>Provides the native bank of intro egg effect definitions for extraction checks.</summary>
    extension(IntroEggEffectDefinitions)
    {
        /// <inheritdoc cref="IntroEggEffectDefinitionsConstants.NativeDefinitionBank"/>
        internal static int NativeDefinitionBank => IntroEggEffectDefinitionsConstants.NativeDefinitionBank;
    }
}

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Cartridge values of <see cref="IntroBabyActorDefinitions"/> that only verification reads.</summary>
internal static class IntroBabyActorDefinitionsConstants
{
    /// <summary>Bank containing the native actor definitions and initializer routines.</summary>
    public const int NativeBank = 0x8b0000;

    /// <summary>Provides the native bank of intro Baby actor definitions for extraction checks.</summary>
    extension(IntroBabyActorDefinitions)
    {
        /// <inheritdoc cref="IntroBabyActorDefinitionsConstants.NativeBank"/>
        internal static int NativeBank => IntroBabyActorDefinitionsConstants.NativeBank;
    }
}

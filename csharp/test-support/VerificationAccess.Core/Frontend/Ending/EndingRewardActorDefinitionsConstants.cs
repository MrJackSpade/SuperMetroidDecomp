namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge values of <see cref="EndingRewardActors"/> that only verification reads.</summary>
internal static class EndingRewardActorDefinitionsConstants
{
    /// <summary>Bank containing the native six-byte cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    /// <summary>Provides the native bank containing ending reward actor definitions for extraction checks.</summary>
    extension(EndingRewardActors)
    {
        /// <inheritdoc cref="EndingRewardActorDefinitionsConstants.NativeDefinitionBank"/>
        internal static int NativeDefinitionBank => EndingRewardActorDefinitionsConstants.NativeDefinitionBank;
    }
}

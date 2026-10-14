using SuperMetroid.Core.Frontend;

/// <summary>Cartridge values of <see cref="EndingLogoDefinitions"/> that only verification reads.</summary>
internal static class EndingLogoDefinitionsConstants
{
    /// <summary>$8B:E554..E569 spawns the upper/lower S and upper/lower circle in this order.</summary>
    public const int ActorCount = 4;
    /// <summary>Bank containing the native six-byte cinematic-object definitions.</summary>
    public const int NativeDefinitionBank = 0x8b0000;

    /// <summary>Provides the number of ending logo actors and their native definition bank for scene decoding.</summary>
    extension(EndingLogoDefinitions)
    {
        /// <inheritdoc cref="EndingLogoDefinitionsConstants.ActorCount"/>
        internal static int ActorCount => EndingLogoDefinitionsConstants.ActorCount;
        /// <inheritdoc cref="EndingLogoDefinitionsConstants.NativeDefinitionBank"/>
        internal static int NativeDefinitionBank => EndingLogoDefinitionsConstants.NativeDefinitionBank;
    }
}

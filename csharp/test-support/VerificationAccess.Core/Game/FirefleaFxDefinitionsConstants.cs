using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="FirefleaFxDefinitions"/> that only verification reads.</summary>
internal static class FirefleaFxDefinitionsConstants
{
    /// <summary>$88:B070 Fireflea_Darkness_Shades, six words plus one opcode alias.</summary>
    public const int DarknessReferenceAddress = 0x88b070;
    /// <summary>$88:B058 Fireflea_Flashing_Shades, twelve packed unsigned words.</summary>
    public const int FlashReferenceAddress = 0x88b058;

    /// <summary>Provides native Fireflea darkness and flash reference addresses for effect verification.</summary>
    extension(FirefleaFxDefinitions)
    {
        /// <inheritdoc cref="FirefleaFxDefinitionsConstants.DarknessReferenceAddress"/>
        internal static int DarknessReferenceAddress => FirefleaFxDefinitionsConstants.DarknessReferenceAddress;
        /// <inheritdoc cref="FirefleaFxDefinitionsConstants.FlashReferenceAddress"/>
        internal static int FlashReferenceAddress => FirefleaFxDefinitionsConstants.FlashReferenceAddress;
    }
}

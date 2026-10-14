using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SamusArmCannonDefinitions"/> that only verification reads.</summary>
internal static class SamusArmCannonDefinitionsConstants
{
    /// <summary>
    /// <c>$90:C7D9</c>, six bytes indexed by the selected HUD item. The native symbol is
    /// <c>ArmCannonOpenFlags</c>; one requests an open cover and zero a closed cover.
    /// </summary>
    public const int OpenFlagTable = 0x90c7d9;

    /// <summary>Provides Samus arm-cannon open flags for verifying pose-dependent rendering.</summary>
    extension(SamusArmCannonDefinitions)
    {
        /// <inheritdoc cref="SamusArmCannonDefinitionsConstants.OpenFlagTable"/>
        internal static int OpenFlagTable => SamusArmCannonDefinitionsConstants.OpenFlagTable;
    }
}

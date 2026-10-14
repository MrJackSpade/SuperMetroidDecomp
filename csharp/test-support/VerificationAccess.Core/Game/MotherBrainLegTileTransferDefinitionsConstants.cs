using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MotherBrainLegTileTransferDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainLegTileTransferDefinitionsConstants
{
    /// <summary>The full $A9:8F8F address used only by cartridge parity checks.</summary>
    public const int NativeListAddress = 0xa98f8f;

    /// <summary>Provides the native list address for Mother Brain leg tile transfer verification.</summary>
    extension(MotherBrainLegTileTransferDefinitions)
    {
        /// <inheritdoc cref="MotherBrainLegTileTransferDefinitionsConstants.NativeListAddress"/>
        internal static int NativeListAddress => MotherBrainLegTileTransferDefinitionsConstants.NativeListAddress;
    }
}

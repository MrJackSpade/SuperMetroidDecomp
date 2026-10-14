using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MotherBrainCorpseRottingState"/> that only verification reads.</summary>
internal static class MotherBrainCorpseRottingStateConstants
{
    /// <summary>Six tile rows times <c>$E0</c> bytes per row.</summary>
    public const int GraphicsBufferSize = 0x0540;
    /// <summary>$7E:9000, first byte of the six-row corpse graphics staging buffer.</summary>
    public const int GraphicsBufferAddress = 0x7e9000;

    /// <summary>Provides the Mother Brain corpse rotting graphics buffer address and size for state checks.</summary>
    extension(MotherBrainCorpseRottingState)
    {
        /// <inheritdoc cref="MotherBrainCorpseRottingStateConstants.GraphicsBufferSize"/>
        internal static int GraphicsBufferSize => MotherBrainCorpseRottingStateConstants.GraphicsBufferSize;
        /// <inheritdoc cref="MotherBrainCorpseRottingStateConstants.GraphicsBufferAddress"/>
        internal static int GraphicsBufferAddress => MotherBrainCorpseRottingStateConstants.GraphicsBufferAddress;
    }
}

using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MotherBrainTileTransferDefinitions"/> that only verification reads.</summary>
internal static class MotherBrainTileTransferDefinitionsConstants
{
    /// <summary>$A9:8FE5, four size/source/destination records loading Baby graphics +$400 onward.</summary>
    public const int BabyTileList = 0xa98fe5;
    /// <summary>Each native record contains a word size, long source, and word VRAM destination.</summary>
    public const int RecordSize = 7;

    /// <summary>Provides the Baby tile list and record width for Mother Brain tile transfer checks.</summary>
    extension(MotherBrainTileTransferDefinitions)
    {
        /// <inheritdoc cref="MotherBrainTileTransferDefinitionsConstants.BabyTileList"/>
        internal static int BabyTileList => MotherBrainTileTransferDefinitionsConstants.BabyTileList;
        /// <inheritdoc cref="MotherBrainTileTransferDefinitionsConstants.RecordSize"/>
        internal static int RecordSize => MotherBrainTileTransferDefinitionsConstants.RecordSize;
    }
}

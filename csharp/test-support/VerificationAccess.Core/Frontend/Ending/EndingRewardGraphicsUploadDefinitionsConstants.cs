using SuperMetroid.AssetExtraction.Frontend.Ending;
namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge values of <see cref="EndingRewardGraphicsUploadDefinitions"/> that only verification reads.</summary>
internal static class EndingRewardGraphicsUploadDefinitionsConstants
{
    /// <summary>$8B:F6D8: Func216 destination VRAM word-address table.</summary>
    public const int DestinationTable = 0x8bf6d8;
    /// <summary>$8B:F6B8: Func216 source-address table, relative to WRAM bank $7F.</summary>
    public const int SourceTable = 0x8bf6b8;

    /// <summary>Provides the ending reward graphics source and destination tables for upload verification.</summary>
    extension(EndingRewardGraphicsUploadDefinitions)
    {
        /// <inheritdoc cref="EndingRewardGraphicsUploadDefinitionsConstants.DestinationTable"/>
        internal static int DestinationTable => EndingRewardGraphicsUploadDefinitionsConstants.DestinationTable;
        /// <inheritdoc cref="EndingRewardGraphicsUploadDefinitionsConstants.SourceTable"/>
        internal static int SourceTable => EndingRewardGraphicsUploadDefinitionsConstants.SourceTable;
    }
}

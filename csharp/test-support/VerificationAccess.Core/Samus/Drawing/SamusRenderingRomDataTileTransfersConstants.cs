using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Cartridge values of <see cref="SamusRenderingRomData.TileTransfers"/> that only verification reads.</summary>
internal static class SamusRenderingRomDataTileTransfersConstants
{
    /// <summary>Bytes in one pose/frame selector: top set/position and bottom set/position.</summary>
    public const int AnimationRecordByteCount = 4;
    /// <summary>Number of bottom-half graphics sets before the animation pointer table.</summary>
    public const int BottomDefinitionSetCount = (SamusCartridgeTables.AnimationDefinitionListPointers - SamusCartridgeTables.BottomDefinitionListPointers) / sizeof(ushort);
    /// <summary>Number of top-half graphics sets before the bottom pointer table.</summary>
    public const int TopDefinitionSetCount = (SamusCartridgeTables.BottomDefinitionListPointers - SamusCartridgeTables.TopDefinitionListPointers) / sizeof(ushort);

    /// <summary>Provides Samus tile transfer animation width and top and bottom definition-set counts.</summary>
    extension(SamusRenderingRomData.TileTransfers)
    {
        /// <inheritdoc cref="SamusRenderingRomDataTileTransfersConstants.AnimationRecordByteCount"/>
        internal static int AnimationRecordByteCount => SamusRenderingRomDataTileTransfersConstants.AnimationRecordByteCount;
        /// <inheritdoc cref="SamusRenderingRomDataTileTransfersConstants.BottomDefinitionSetCount"/>
        internal static int BottomDefinitionSetCount => SamusRenderingRomDataTileTransfersConstants.BottomDefinitionSetCount;
        /// <inheritdoc cref="SamusRenderingRomDataTileTransfersConstants.TopDefinitionSetCount"/>
        internal static int TopDefinitionSetCount => SamusRenderingRomDataTileTransfersConstants.TopDefinitionSetCount;
    }
}

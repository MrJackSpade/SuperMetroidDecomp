using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Verification's native view of the arm cannon: descriptors by their bank-$90 address and cover tiles by their
/// bank-$9A source, as the cartridge's tables store them.
/// </summary>
internal static class SamusArmCannonNative
{
    /// <summary>Native views of the installed arm cannon.</summary>
    extension(SamusArmCannonArtworkCatalog cannon)
    {
        /// <summary>The bank-$90 address of pose <paramref name="pose"/>'s drawing descriptor.</summary>
        internal ushort PoseDrawingData(int pose) =>
            checked((ushort)(SamusArmCannonArtworkFormat.DrawingDataStart + cannon.PoseDescriptor(pose)));

        /// <summary>The installed drawing byte stored at bank-$90 <paramref name="address"/>.</summary>
        internal byte ReadDrawingByteAt(int address) => cannon.ReadDrawingByte(address - SamusArmCannonArtworkFormat.DrawingDataStart);

        /// <summary>The bank-$9A source of the cover tile <paramref name="direction"/> shows at <paramref name="frame"/>, or zero for the closed cover.</summary>
        internal ushort TileSource(int direction, int frame) => cannon.CoverTileIndex(direction, frame) is { } tile
            ? SamusArmCannonArtworkFormat.CoverTileDmaSources[tile]
            : (ushort)0;
    }

    /// <summary>Native views of the arm cannon's stock tables.</summary>
    extension(SamusArmCannonArtworkFormat)
    {
        /// <summary>The bank-$9A source of each installed cover tile.</summary>
        internal static SamusArmCannonArtworkFormat.TileSourceSequence TileSourcePointers => SamusArmCannonArtworkFormat.CoverTileDmaSources;

        /// <summary>The bank-$9A source of the stock cover tile <paramref name="direction"/> shows at <paramref name="frame"/>, or zero for the closed cover.</summary>
        internal static ushort StockTileSource(SamusProjectileDirection direction, int frame) => frame == 0
            ? (ushort)0
            : SamusArmCannonArtworkFormat.CoverTileDmaSources[SamusArmCannonArtworkFormat.StockCoverTile(direction, frame)];
    }
}

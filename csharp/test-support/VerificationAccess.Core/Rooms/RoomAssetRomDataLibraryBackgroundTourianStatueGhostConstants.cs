namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge values of <see cref="RoomAssetRomData.LibraryBackground.TourianStatueGhost"/> that only verification reads.</summary>
internal static class RoomAssetRomDataLibraryBackgroundTourianStatueGhostConstants
{
    /// <summary>Native destination VRAM word for the ghost character upload.</summary>
    public const ushort VramDestinationWord = 0x6d00;

    /// <summary>Provides the ghost Tourian statue background VRAM destination for library asset checks.</summary>
    extension(RoomAssetRomData.LibraryBackground.TourianStatueGhost)
    {
        /// <inheritdoc cref="RoomAssetRomDataLibraryBackgroundTourianStatueGhostConstants.VramDestinationWord"/>
        internal static ushort VramDestinationWord => RoomAssetRomDataLibraryBackgroundTourianStatueGhostConstants.VramDestinationWord;
    }
}

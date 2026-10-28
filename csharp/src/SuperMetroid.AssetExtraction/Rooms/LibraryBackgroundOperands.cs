using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Maps a library-background instruction's named sky pages and destinations to the cartridge's encoding of them (#1414):
/// extraction reads a sky page and writes a destination's words through this map.
/// </summary>
internal static class LibraryBackgroundOperands
{
    /// <summary>Where the cartridge stores scrolling-sky page <paramref name="page"/>, 0 through 6.</summary>
    internal static int SkyPageAddress(int page) =>
        RoomBackgroundSourceDefinitions.FirstSkyPage
        + Ensure.BetweenInclusive(0, RoomSkyTilemapFormat.PageCount - 1, page) * RoomBackgroundSourceDefinitions.SkyPageBytes;

    /// <summary>The VRAM word a transfer to <paramref name="destination"/> starts at.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="destination"/> is not a transfer's.</exception>
    internal static ushort DestinationWord(LibraryBackgroundDestination destination) =>
        RoomBackgroundSourceDefinitions.DestinationWords.ContainsKey(destination)
            ? RoomBackgroundSourceDefinitions.DestinationWords[destination]
            : throw new ArgumentOutOfRangeException(nameof(destination), destination, "Not a library-background transfer's destination.");
}

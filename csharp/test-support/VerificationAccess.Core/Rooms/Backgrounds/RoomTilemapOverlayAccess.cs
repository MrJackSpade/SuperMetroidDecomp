using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Reads an overlay's written tiles as the cartridge's packed tilemap words, so verification can compare them with the
/// words the cartridge writes.
/// </summary>
internal static class RoomTilemapOverlayAccess
{
    /// <summary>The value a position the overlay leaves to the level data reads as.</summary>
    private const int UnwrittenWord = -1;

    /// <summary>The packed words of an overlay's tiles.</summary>
    extension(RoomTilemapOverlay overlay)
    {
        /// <summary>Each position's packed tilemap word, or -1 where nothing was written.</summary>
        internal ImmutableArray<int> Words =>
            [.. overlay.Tiles.Select(tile => tile is { } written ? written.PackedWord : UnwrittenWord)];
    }

    /// <summary>The marker for a position an overlay leaves to the level data.</summary>
    extension(RoomTilemapOverlay)
    {
        /// <summary>Marks a tilemap word the overlay leaves to the level data.</summary>
        internal static int Unwritten => UnwrittenWord;
    }
}

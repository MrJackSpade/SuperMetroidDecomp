using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// A room background tilemap atlas as the cartridge's little-endian tilemap entries, encoded from its tiles through the
/// import pipeline's codec, so checks compare the installed tiles with the bytes the cartridge decompresses.
/// </summary>
internal static class RoomBackgroundTilemapAtlasBytes
{
    /// <summary>The cartridge's view of an atlas.</summary>
    extension(RoomBackgroundTilemapAtlas atlas)
    {
        /// <summary>Each placed tile's packed tilemap entry, as little-endian bytes in page order.</summary>
        internal ReadOnlyMemory<byte> Bytes => BackgroundTileWordBytes.Of(atlas.Tiles.Span);
    }
}

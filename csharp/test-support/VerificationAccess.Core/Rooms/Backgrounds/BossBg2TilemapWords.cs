using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>A boss body tilemap as the cartridge's tilemap entries, encoded from its tiles through the codec.</summary>
internal static class BossBg2TilemapWords
{
    /// <summary>The cartridge's view of a boss body tilemap.</summary>
    extension(BossBg2Tilemap tilemap)
    {
        /// <summary>Each tile's packed tilemap entry in screen order.</summary>
        internal ushort[] Words => BackgroundTileWordBytes.Words(tilemap.Tiles.AsSpan());
    }
}

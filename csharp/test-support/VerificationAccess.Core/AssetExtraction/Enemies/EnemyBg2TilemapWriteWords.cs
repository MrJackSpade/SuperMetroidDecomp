using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>A BG2 frame write as the cartridge's tilemap entries, encoded from its tiles through the codec.</summary>
internal static class EnemyBg2TilemapWriteWords
{
    /// <summary>The cartridge's view of one BG2 frame write.</summary>
    extension(EnemyBg2TilemapWrite write)
    {
        /// <summary>Each written tile's packed tilemap entry, in order.</summary>
        internal ushort[] Words => BackgroundTileWordBytes.Words(write.Tiles.Span);
    }
}

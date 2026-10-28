using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies;

/// <summary>The cartridge's view of the published enemy BG2 images: each tile's packed tilemap entry through the codec.</summary>
internal static class EnemyBg2Words
{
    /// <summary>A published enemy BG2 image as tilemap entries.</summary>
    extension(EnemyBg2ImageSnapshot image)
    {
        /// <summary>Each tile's packed tilemap entry, row by row.</summary>
        internal ushort[] Words => BackgroundTileWordBytes.Words(image.Tiles.AsSpan());
    }
}

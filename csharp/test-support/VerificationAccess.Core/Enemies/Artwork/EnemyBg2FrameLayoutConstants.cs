using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values and helpers of <see cref="EnemyBg2FrameLayout"/> that only verification reads.</summary>
internal static class EnemyBg2FrameLayoutConstants
{
    /// <summary>First VRAM word of the enemy BG2 tilemap.</summary>
    internal const ushort VramBase = 0x4800;

    /// <summary>The word that marks an extended spritemap component as a BG2 command stream, as the import decodes it.</summary>
    internal const ushort StreamMarker = EnemySourceDefinitions.Bg2StreamMarker;

    /// <summary>The work-RAM enemy BG2 tilemap a stream's destinations address, as the import decodes them.</summary>
    internal const ushort WorkingRamBase = EnemySourceDefinitions.Bg2WorkingTilemap;

    /// <summary>The enemy BG2 tilemap's size, which only verification reads once boss bodies are drawn as images (#1466).</summary>
    extension(EnemyBg2FrameLayout)
    {
        /// <summary>The cells of the enemy BG2 tilemap: 32 columns by 64 rows.</summary>
        internal static int TilemapCellCount => EnemyBg2FrameLayout.TilemapWidth * EnemyBg2FrameLayout.TilemapHeight;
    }
}

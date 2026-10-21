namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Shared dimensions of the enemy BG2 tilemap targeted by $A0:96CA.</summary>
internal static class EnemyBg2FrameLayout
{
    /// <summary>$FFFE marks an extended spritemap component as a BG2 command stream.</summary>
    internal const ushort StreamMarker = 0xfffe;
    /// <summary>First VRAM word of the enemy BG2 tilemap.</summary>
    internal const ushort VramBase = 0x4800;
    /// <summary>First byte of the native $7E working tilemap.</summary>
    internal const ushort WorkingRamBase = 0x2000;
    /// <summary>Number of tile words across one enemy BG2 tilemap row.</summary>
    internal const int TilemapWidth = 32;
    /// <summary>Number of rows available in the enemy BG2 tilemap.</summary>
    internal const int TilemapHeight = 64;

    /// <summary>The words of the enemy BG2 tilemap: 32 columns by 64 rows.</summary>
    internal const int TilemapWordCount = TilemapWidth * TilemapHeight;
    /// <summary>The most native components one BG2 frame draws: Mother Brain's body poses, the largest.</summary>
    internal const int MaximumComponents = 10;
    /// <summary>Defensive bound for one $FFFE stream's terminated command list.</summary>
    internal const int MaximumCommandsPerStream = 128;
}

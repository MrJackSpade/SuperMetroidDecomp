namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Shared dimensions of the enemy BG2 tilemap.</summary>
internal static class EnemyBg2FrameLayout
{
    /// <summary>Number of tiles across one enemy BG2 tilemap row.</summary>
    internal const int TilemapWidth = 32;
    /// <summary>Number of rows available in the enemy BG2 tilemap.</summary>
    internal const int TilemapHeight = 64;

    /// <summary>The most native components one BG2 frame draws: Mother Brain's body poses, the largest.</summary>
    internal const int MaximumComponents = 10;
    /// <summary>Defensive bound for one BG2 command stream's terminated command list.</summary>
    internal const int MaximumCommandsPerStream = 128;
}

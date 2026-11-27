namespace Engine.World;

/// <summary>What of a room a shake throws about. The player is never thrown.</summary>
public enum ShakeReach
{
    /// <summary>The room's tiles alone.</summary>
    Tiles,

    /// <summary>The room's tiles and its backdrops.</summary>
    TilesAndBackdrops,

    /// <summary>The room's tiles, its backdrops, and what its enemies have launched and left.</summary>
    Everything,

    /// <summary>The room's backdrops, and what its enemies have launched and left; its tiles stay still.</summary>
    BackdropsAndThings,
}

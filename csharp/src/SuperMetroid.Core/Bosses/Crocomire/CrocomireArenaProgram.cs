namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's five arena PLM programs (#1296), one per arena layout.</summary>
internal enum CrocomireArenaProgram
{
    /// <summary>Clears the ten-block bridge.</summary>
    ClearBridge,
    /// <summary>Crumbles one bridge block.</summary>
    CrumbleBridgeBlock,
    /// <summary>Clears one bridge block.</summary>
    ClearBridgeBlock,
    /// <summary>Clears the invisible wall.</summary>
    ClearInvisibleWall,
    /// <summary>Creates the invisible wall.</summary>
    CreateInvisibleWall,
}

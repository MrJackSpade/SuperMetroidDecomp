namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's five arena PLM programs (#1296), one per arena layout.</summary>
internal enum CrocomireArenaProgram
{
    /// <summary>Clears the ten-block bridge (header $B747).</summary>
    ClearBridge,
    /// <summary>Crumbles one bridge block (header $B74B).</summary>
    CrumbleBridgeBlock,
    /// <summary>Clears one bridge block (header $B74F).</summary>
    ClearBridgeBlock,
    /// <summary>Clears the invisible wall (header $B753).</summary>
    ClearInvisibleWall,
    /// <summary>Creates the invisible wall (header $B757).</summary>
    CreateInvisibleWall,
}

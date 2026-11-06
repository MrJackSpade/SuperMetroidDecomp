namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The bridge and invisible-wall changes Crocomire's fight makes to its arena: the only spawns Crocomire's arena spawner performs.</summary>
public enum CrocomireArenaMutation
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

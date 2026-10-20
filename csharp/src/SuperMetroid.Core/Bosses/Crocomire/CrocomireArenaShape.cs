namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>What a Crocomire arena draw list writes: a bridge run, or a wall column that is cleared or made solid.</summary>
internal enum CrocomireArenaShape
{
    /// <summary>One horizontal bridge run.</summary>
    Bridge,

    /// <summary>Three wall columns written as air.</summary>
    ClearWall,

    /// <summary>Three wall columns written as solid blocks.</summary>
    SolidWall,
}

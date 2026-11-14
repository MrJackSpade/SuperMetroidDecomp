namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>What the shot-block response does with a common shootable reaction it handles.</summary>
public enum ShotBlockReaction
{
    /// <summary>Any weapon breaks the block, and it restores itself later.</summary>
    RespawningBreak,

    /// <summary>Any weapon breaks the block for good.</summary>
    PermanentBreak,

    /// <summary>A Power Bomb breaks the block, and it restores itself later.</summary>
    RespawningPowerBombBreak,

    /// <summary>A Power Bomb breaks the block for good.</summary>
    PermanentPowerBombBreak,

    /// <summary>A Super Missile breaks the block, and it restores itself later.</summary>
    RespawningSuperMissileBreak,

    /// <summary>A Super Missile breaks the block for good.</summary>
    PermanentSuperMissileBreak,

    /// <summary>The empty reaction: a one-frame object that leaves the block alone.</summary>
    Nothing,

    /// <summary>
    /// The collision probe: it publishes the shot's collision result, then spawns a one-frame object that does nothing.
    /// </summary>
    CollisionProbe,
}

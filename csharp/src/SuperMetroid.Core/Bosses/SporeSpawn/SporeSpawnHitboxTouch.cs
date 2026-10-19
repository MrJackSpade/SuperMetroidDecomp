namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>How one of Spore Spawn's rectangles reacts when Samus touches it.</summary>
internal enum SporeSpawnHitboxTouch
{
    /// <summary>Spore Spawn's own contact reaction: every rectangle once the head has begun to open.</summary>
    Contact = 1,
    /// <summary>No reaction: the closed-head shell before the fight starts.</summary>
    Inert,
}

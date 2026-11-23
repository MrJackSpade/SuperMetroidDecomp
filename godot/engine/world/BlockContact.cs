namespace Engine.World;

/// <summary>What a block does to something touching it.</summary>
public enum BlockContact
{
    /// <summary>Nothing.</summary>
    Nothing,

    /// <summary>It does ongoing damage, as a spike does.</summary>
    Hurts,

    /// <summary>It carries what stands on it sideways, as a conveyor does.</summary>
    Carries,

    /// <summary>It pulls what is in it downward, as sand does.</summary>
    PullsDown,

    /// <summary>It can be latched onto.</summary>
    Latch,

    /// <summary>It can be latched onto once, and then breaks.</summary>
    LatchOnce,
}

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed body programs (#641), in the authored order of their lists.</summary>
internal enum SporeSpawnBodySegment
{
    /// <summary>A defeated room's hardened husk.</summary>
    InitialDead,
    /// <summary>The living room's wait before the descent.</summary>
    InitialAlive,
    /// <summary>The fight's first sway.</summary>
    FightStarted,
    /// <summary>Opening the head and stopping.</summary>
    OpenAndStop,
    /// <summary>The open head's pulsing loop.</summary>
    OpenPulse,
    /// <summary>Closing the head and resuming the sway.</summary>
    CloseAndMove,
    /// <summary>The death sequence's setup.</summary>
    Death,
    /// <summary>The dying explosions.</summary>
    DyingExplosions,
    /// <summary>Hardening into the husk.</summary>
    Hardening,
}

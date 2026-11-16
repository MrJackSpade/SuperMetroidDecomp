namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed body animations (#641), in authored order.</summary>
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
    /// <summary>A dying explosion and the wait after it, ten times.</summary>
    DyingExplosions,
    /// <summary>The dying head closes through its poses and starts to harden (#2235).</summary>
    DeathClosing,
    /// <summary>Hardening dust and the wait after it, ten times.</summary>
    Hardening,
    /// <summary>The husk's colours fade in, step by step with more dust, then the death's drops (#2235).</summary>
    DeathFade,
}

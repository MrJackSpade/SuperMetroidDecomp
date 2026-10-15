namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Spore Spawn's typed body programs (#641), in the authored order of their bank-$A5 lists.</summary>
internal enum SporeSpawnBodyProgram
{
    /// <summary>A defeated room's hardened husk (<c>InstList_SporeSpawn_Initial_Dead</c>, $A5:E6B9).</summary>
    InitialDead,
    /// <summary>The living room's wait before the descent (<c>InstList_SporeSpawn_Initial_Alive</c>, $A5:E6C7).</summary>
    InitialAlive,
    /// <summary>The fight's first sway (<c>InstList_SporeSpawn_FightStarted</c>, $A5:E6D5).</summary>
    FightStarted,
    /// <summary>Opening the head and stopping (<c>InstList_SporeSpawn_OpenAndStop_0</c>, $A5:E6E3).</summary>
    OpenAndStop,
    /// <summary>The open head's pulsing loop (<c>InstList_SporeSpawn_OpenAndStop_1</c>, $A5:E715).</summary>
    OpenPulse,
    /// <summary>Closing the head and resuming the sway (<c>InstList_SporeSpawn_CloseAndMove</c>, $A5:E729).</summary>
    CloseAndMove,
    /// <summary>The death sequence's setup (<c>InstList_SporeSpawn_DeathSequence_0</c>, $A5:E77D).</summary>
    Death,
    /// <summary>The dying explosions (<c>InstList_SporeSpawn_DeathSequence_1</c>, $A5:E78D).</summary>
    DyingExplosions,
    /// <summary>Hardening into the husk (<c>InstList_SporeSpawn_DeathSequence_2</c>, $A5:E7BD).</summary>
    Hardening,
}

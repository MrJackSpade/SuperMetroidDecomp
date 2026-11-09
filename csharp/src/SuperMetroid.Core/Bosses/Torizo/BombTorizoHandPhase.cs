namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Where the Bomb Torizo hand is in its course (<see cref="BombTorizoHandObject"/>).</summary>
internal enum BombTorizoHandPhase
{
    /// <summary>Spawned: its first update shows the intact hand.</summary>
    Starting,
    /// <summary>Its second update arms its wake for when Samus has Bombs.</summary>
    Arming,
    /// <summary>Resting until Samus has Bombs.</summary>
    Resting,
    /// <summary>Woken: stepping through its crumble.</summary>
    Crumbling,
}

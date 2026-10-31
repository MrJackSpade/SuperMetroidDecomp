namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Definitions owned by Spore Spawn's scrolling-finished hook.
/// </summary>
public static class SporeSpawnScrollingHooks
{
    /// <summary>
    /// <c>ScrollingFinishedHook_SporeSpawnFight</c>. The routine
    /// prevents layer one from moving above Y 464 while the live encounter owns
    /// the global scrolling-finished callback.
    /// </summary>
    public const int FightMinimumLayerOneY = 464;
}

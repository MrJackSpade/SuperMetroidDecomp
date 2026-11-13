namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Definitions owned by Spore Spawn's scrolling-finished hook.
/// </summary>
public static class SporeSpawnScrollingHooks
{
    /// <summary>
    /// The scrolling-finished hook of the Spore Spawn fight. The hook
    /// prevents the camera from moving above Y 464 while the live encounter owns
    /// the global scrolling-finished callback.
    /// </summary>
    public const int FightMinimumCameraY = 464;
}

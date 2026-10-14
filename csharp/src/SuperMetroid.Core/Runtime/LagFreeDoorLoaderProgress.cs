namespace SuperMetroid.Core.Runtime;

/// <summary>The port's normal-play policy: the loader completes within its first update.</summary>
public sealed class LagFreeDoorLoaderProgress : IDoorLoaderProgressSource
{
    /// <summary>Shared stateless progress source used by normal play.</summary>
    public static LagFreeDoorLoaderProgress Instance { get; } = new();

    /// <summary>Restricts normal-play progress reporting to the shared stateless instance.</summary>
    private LagFreeDoorLoaderProgress() { }

    /// <summary>Reports every enemy slot initialized in the first door-scroll update, intentionally eliminating only cartridge CPU-time lag.</summary>
    /// <param name="slot">Enemy slot ordinal; the lag-free policy does not inspect or bound it.</param>
    /// <returns>Always true.</returns>
    public bool HasInitializedEnemySlot(int slot) => true;
}

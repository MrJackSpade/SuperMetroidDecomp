namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Reports how far the cartridge's door loader (<c>$82:E4A9</c>) has progressed through
/// <c>Initialise_Enemies</c> by the end of the current door-scroll update.
/// </summary>
/// <remarks>
/// The loader's CPU work (enemy tile decompression, sprite clearing, per-enemy init) spans
/// several NMIs, during which the door IRQ keeps scrolling. Which update a loader-time write
/// lands in, such as an arriving elevator placing Samus, therefore depends on CPU time. The
/// port loads the room in one step and applies such writes when this source reports them.
/// </remarks>
public interface IDoorLoaderProgressSource
{
    /// <summary>True once the loader has run the init AI of enemy slot <paramref name="slot"/>.</summary>
    bool HasInitializedEnemySlot(int slot);
}

/// <summary>The port's normal-play policy: the loader completes within its first update.</summary>
public sealed class LagFreeDoorLoaderProgress : IDoorLoaderProgressSource
{
    /// <summary>Shared stateless progress source used by normal play.</summary>
    public static LagFreeDoorLoaderProgress Instance { get; } = new();

    private LagFreeDoorLoaderProgress() { }

    /// <summary>Reports every enemy slot initialized in the first door-scroll update, intentionally eliminating only cartridge CPU-time lag.</summary>
    /// <param name="slot">Enemy slot ordinal; the lag-free policy does not inspect or bound it.</param>
    /// <returns>Always true.</returns>
    public bool HasInitializedEnemySlot(int slot) => true;
}

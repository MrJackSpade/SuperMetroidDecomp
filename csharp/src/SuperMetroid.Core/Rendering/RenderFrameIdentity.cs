namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Host identity, independent of the cartridge counter's wrap and debugger-state rewind.
/// Sequence increases for every published host frame; generation increases on load/reset.
/// </summary>
/// <param name="Sequence">Positive host publication sequence, kept monotonic across simulation counter wraps and load/reset generations.</param>
/// <param name="Generation">Positive host load/reset generation used to reject stale queued display packets.</param>
/// <param name="SimulationFrame">Captured simulation frame number, 0 through <see cref="MaximumSimulationFrame"/>; it wraps or rewinds with the game's frame counter and is not a publication-order key.</param>
public readonly record struct RenderFrameIdentity(long Sequence, long Generation, int SimulationFrame)
{
    /// <summary>The largest frame number before the game's frame counter wraps to zero.</summary>
    public const int MaximumSimulationFrame = ushort.MaxValue;

    /// <summary>Captured simulation frame number, 0 through <see cref="MaximumSimulationFrame"/>.</summary>
    public int SimulationFrame
    {
        get => simulationFrame;
        init => simulationFrame = Ensure.BetweenInclusive(0, MaximumSimulationFrame, value);
    }

    /// <summary>Validated store of <see cref="SimulationFrame"/>.</summary>
    private readonly int simulationFrame = Ensure.BetweenInclusive(0, MaximumSimulationFrame, SimulationFrame);
}

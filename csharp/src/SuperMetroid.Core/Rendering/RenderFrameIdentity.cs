namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Host identity, independent of the cartridge counter's wrap and debugger-state rewind.
/// Sequence increases for every published host frame; generation increases on load/reset.
/// </summary>
/// <param name="Sequence">Positive host publication sequence, kept monotonic across simulation counter wraps and load/reset generations.</param>
/// <param name="Generation">Positive host load/reset generation used to reject stale queued display packets.</param>
/// <param name="SimulationFrame">Captured native-width simulation frame counter, which may wrap or rewind and is not a publication-order key.</param>
public readonly record struct RenderFrameIdentity(long Sequence, long Generation, ushort SimulationFrame);

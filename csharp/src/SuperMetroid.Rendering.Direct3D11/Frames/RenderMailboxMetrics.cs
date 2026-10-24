namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>Visual-only counters; none represents skipped emulation ticks or audio.</summary>
public readonly record struct RenderMailboxMetrics(long Replaced);

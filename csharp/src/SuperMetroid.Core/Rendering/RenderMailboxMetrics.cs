namespace SuperMetroid.Core.Rendering;

/// <summary>Visual-only counters; none represents skipped emulation ticks or audio.</summary>
public readonly record struct RenderMailboxMetrics(    long Replaced);

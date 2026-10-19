namespace SuperMetroid.Core.Enemies;

/// <summary>The accepted-frame counters enemy code reads for one pass.</summary>
/// <param name="Counter8">The short accepted-frame counter, 0 through 255.</param>
internal readonly record struct FrameCounters(int Counter8);

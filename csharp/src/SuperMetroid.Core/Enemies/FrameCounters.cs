namespace SuperMetroid.Core.Enemies;

/// <summary>The accepted-frame counters enemy code reads for one pass.</summary>
/// <param name="ShortCounter">The short frame counter, 0 through 255.</param>
internal readonly record struct FrameCounters(int ShortCounter);

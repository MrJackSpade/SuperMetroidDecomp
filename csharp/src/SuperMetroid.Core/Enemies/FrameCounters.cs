namespace SuperMetroid.Core.Enemies;

/// <summary>The update counts enemy code reads for one pass.</summary>
/// <param name="ShortCounter">The enemies' short update count, which paces their cadences.</param>
internal readonly record struct FrameCounters(int ShortCounter);

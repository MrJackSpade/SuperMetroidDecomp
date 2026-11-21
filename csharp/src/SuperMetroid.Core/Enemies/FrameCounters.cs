namespace SuperMetroid.Core.Enemies;

/// <summary>The update counts enemy code reads for one pass.</summary>
/// <param name="ShortCounter">The enemies' short update count, which paces their cadences (<see cref="Compatibility.ShortFrameCount"/>).</param>
internal readonly partial record struct FrameCounters(Compatibility.ShortFrameCount ShortCounter);

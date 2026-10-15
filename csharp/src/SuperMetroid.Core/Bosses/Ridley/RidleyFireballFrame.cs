namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One fireball or afterburn frame: its run and its pose within the run.</summary>
/// <param name="Kind">The run of poses.</param>
/// <param name="Phase">The pose within the run.</param>
internal readonly record struct RidleyFireballFrame(RidleyFireballFrameKind Kind, int Phase);

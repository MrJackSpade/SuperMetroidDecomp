namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>A run of occupied columns in one row of an intro silhouette, both ends included.</summary>
/// <param name="First">The run's leftmost column.</param>
/// <param name="Last">The run's rightmost column.</param>
internal readonly record struct IntroAtlasColumnRun(int First, int Last);

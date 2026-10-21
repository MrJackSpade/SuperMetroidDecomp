namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>A Chozo statue paint shade and whether it is in the second half.</summary>
/// <param name="Shade">The shade.</param>
/// <param name="Second">The second.</param>
internal readonly record struct ChozoStatueShade(int Shade, bool Second);

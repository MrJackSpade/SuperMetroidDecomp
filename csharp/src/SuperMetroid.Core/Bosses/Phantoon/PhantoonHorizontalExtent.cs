namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The left and right edges of a Phantoon hitbox, relative to its centre.</summary>
/// <param name="Left">The left.</param>
/// <param name="Right">The right.</param>
internal readonly record struct PhantoonHorizontalExtent(int Left, int Right);

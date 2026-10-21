namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The top and bottom edges of a Phantoon hitbox, relative to its centre.</summary>
/// <param name="Top">The top.</param>
/// <param name="Bottom">The bottom.</param>
internal readonly record struct PhantoonVerticalExtent(short Top, short Bottom);

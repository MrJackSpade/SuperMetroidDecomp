namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The segment a shooting star's flight plays (#2359), named for the pace its picture changes at: each picture after the
/// lead-in holds for <paramref name="Period"/> updates until the full star, or a still star keeps its first visible picture.
/// </summary>
/// <param name="Period">The updates each picture holds, or null for a still star.</param>
internal readonly record struct EndingShootingStarPace(int? Period);

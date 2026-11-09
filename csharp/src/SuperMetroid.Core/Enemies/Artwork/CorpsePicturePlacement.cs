namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Where a rotting corpse lies in one of its frame's pictures (#1539): the picture row its first pixel row lies at. The
/// corpse's rows run down the whole width of the picture from there, and every one of them rots as the corpse's own rot
/// moves it; the picture's rows above and below the corpse show themselves.
/// </summary>
/// <param name="Top">The picture row the corpse's first pixel row lies at, which may lie above the picture.</param>
public sealed record CorpsePicturePlacement(int Top);

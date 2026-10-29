namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// One region of what a graphics set's frames read that each of its encounters' looks covers whole or not at all (#1392,
/// #1406, #1463); a frame drawn over it has a picture for each look that can show there. The import pipeline decides the
/// regions and names them; Core only tells them apart.
/// </summary>
/// <param name="Ordinal">The region's place among its set's regions.</param>
/// <param name="Name">The name the set's pictures spell the region by.</param>
public sealed record LookRegion(int Ordinal, string Name);

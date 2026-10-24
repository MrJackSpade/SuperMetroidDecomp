namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// One region of a graphics set's characters that each of its encounters' uploads covers whole or not at all (#1392, #1406);
/// a frame drawn over it has a picture for each upload that can show there.
/// </summary>
/// <param name="Ordinal">The region's place among its set's regions.</param>
/// <param name="Name">The name the set's pictures spell the region by.</param>
public sealed record CharacterRegion(int Ordinal, string Name);

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The sides of a world map room tile that carry a border.</summary>
/// <param name="Top">The top.</param>
/// <param name="Bottom">The bottom.</param>
/// <param name="Left">The left.</param>
/// <param name="Right">The right.</param>
[IndependentFlag("the independent sides of a map tile that carry a border, read from the tile number")]
internal readonly record struct WorldMapRoomBorder(bool Top, bool Bottom, bool Left, bool Right);

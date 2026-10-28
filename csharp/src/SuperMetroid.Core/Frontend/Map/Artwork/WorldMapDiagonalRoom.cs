namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>How a diagonal world map room tile is drawn.</summary>
/// <param name="Mirror">The mirror.</param>
/// <param name="Offset">The offset.</param>
/// <param name="Below">The below.</param>
/// <param name="Left">The left.</param>
/// <param name="Right">The right.</param>
/// <param name="DashLeft">The dash left.</param>
/// <param name="DashBottom">The dash bottom.</param>
/// <param name="WallJoin">Whether the slope continues as a wall down the left side to the dotted guide.</param>
[IndependentFlag("independent drawing facts of a diagonal map tile, read from the tile number")]
internal readonly record struct WorldMapDiagonalRoom(bool Mirror, int Offset, bool Below, bool Left, bool Right, bool DashLeft, bool DashBottom, bool WallJoin);

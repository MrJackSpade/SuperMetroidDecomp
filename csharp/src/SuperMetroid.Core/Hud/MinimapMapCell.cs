using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>A revealed map cell on the minimap (#1466): its shape and mirroring, in the shade the minimap shows it in.</summary>
/// <param name="Shape">The map cell's shape.</param>
/// <param name="Mirroring">How the shape is mirrored.</param>
/// <param name="Shade">Whether the minimap shows the cell explored, unexplored or marking Samus's place.</param>
public sealed record MinimapMapCell(MapCellShape Shape, TileFlip Mirroring, MinimapShade Shade) : MinimapCell;

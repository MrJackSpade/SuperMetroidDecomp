using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>A revealed map cell on the minimap (#1513): the cell, in the shade the minimap shows it in.</summary>
/// <param name="Cell">How the map cell looks.</param>
/// <param name="Shade">Whether the minimap shows the cell explored, unexplored or marking Samus's place.</param>
public sealed record MinimapMapCell(MapCell Cell, MinimapShade Shade) : MinimapCell;

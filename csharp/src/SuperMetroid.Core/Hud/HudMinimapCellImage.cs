using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>A revealed map cell on the minimap (#1513): the picture of the cell in a minimap shade.</summary>
/// <param name="Cell">How the map cell looks.</param>
/// <param name="Shade">The shade the minimap shows the cell in.</param>
public sealed record HudMinimapCellImage(MapCell Cell, MinimapShade Shade) : HudImage
{
    /// <summary>The image of how the minimap shows <paramref name="cell"/>.</summary>
    public static HudMinimapCellImage Of(MinimapMapCell cell) => new(Ensure.NotNull(cell).Cell, cell.Shade);

    /// <inheritdoc/>
    public override string FrameName => $"Hud.Minimap.{Cell.Name}.{Shade}";
}

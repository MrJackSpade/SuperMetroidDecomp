using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Hud;

/// <summary>A revealed map cell on the minimap (#1466): its shape, mirrored as the map lays it, in a minimap shade.</summary>
/// <param name="Shape">The map cell's shape.</param>
/// <param name="Mirroring">How the shape is mirrored.</param>
/// <param name="Shade">The shade the minimap shows the cell in.</param>
public sealed record HudMinimapCellImage(MapCellShape Shape, TileFlip Mirroring, MinimapShade Shade) : HudImage
{
    /// <summary>The image of how the minimap shows <paramref name="cell"/>.</summary>
    public static HudMinimapCellImage Of(MinimapMapCell cell) =>
        new(Ensure.NotNull(cell).Shape, cell.Mirroring, cell.Shade);

    /// <inheritdoc/>
    public override string FrameName => $"Hud.Minimap.{Shape.Picture}.{MirroringName}.{Shade}";

    /// <summary>The mirroring as the frame name spells it.</summary>
    private string MirroringName => Mirroring switch
    {
        TileFlip.None => "AsDrawn",
        TileFlip.Horizontal => "Horizontal",
        TileFlip.Vertical => "Vertical",
        TileFlip.Horizontal | TileFlip.Vertical => "Both",
        _ => throw new InvalidOperationException($"Undefined mirroring {Mirroring}."),
    };
}

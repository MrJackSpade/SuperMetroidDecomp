namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Where Draygon's body shows on his body layer (#1354): the simulation body position and the layer placement that puts
/// the body image there, as <c>$A5:9342</c> derives it after enemies draw.
/// </summary>
/// <param name="BodyPosition">Draygon's body position.</param>
/// <param name="LayerPlacement">The layer position shown at the screen's top-left, which puts the body image at the body position.</param>
/// <param name="Deleted">Whether the body has been deleted, which hides it.</param>
public sealed record DraygonBodyPlacement(Position16<RoomPixels> BodyPosition, Position16<BackgroundPixels> LayerPlacement, bool Deleted)
{
    /// <summary>The physical scanlines [first, end) that show the body against <paramref name="camera"/>.</summary>
    public DraygonBodyScanlines VisibleLines(Position16<RoomPixels> camera) => DraygonMainScreenWindow.Select(BodyPosition, camera, Deleted);
}

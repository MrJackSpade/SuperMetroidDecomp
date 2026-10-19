namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Where Draygon's body shows on BG2 (#1354): the simulation body position and the BG2 scroll that places the body
/// image there, as <c>$A5:9342</c> derives it after enemies draw.
/// </summary>
/// <param name="BodyPosition">Draygon's body position.</param>
/// <param name="Scroll">The BG2 scroll that places the body image at the body position.</param>
/// <param name="Deleted">Whether the body has been deleted, which hides it.</param>
public sealed record DraygonBodyPlacement(Position16<RoomPixels> BodyPosition, Position16<Bg2Pixels> Scroll, bool Deleted)
{
    /// <summary>The physical scanlines [first, end) that show the body against <paramref name="camera"/>.</summary>
    public (int First, int End) VisibleLines(Position16<RoomPixels> camera) => DraygonMainScreenWindow.Select(BodyPosition, camera, Deleted);
}

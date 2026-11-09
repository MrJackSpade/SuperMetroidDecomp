namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The pictures fixed in a room around its turning floor (#1545), such as the getaway chamber's walls: a pictured plane
/// scrolled with the room's camera, drawn below the HUD behind the floor, the actors and the bottom band's background.
/// </summary>
/// <param name="Plane">The plane holding the pictures at their room points.</param>
/// <param name="Scroll">The plane pixel the screen's top-left corner shows: the camera the frame's actors were drawn against.</param>
public sealed record TurningRoomScenery(PicturedPlane Plane, Offset16<PlanePixels> Scroll)
{
    /// <summary>The plane holding the pictures at their room points.</summary>
    public PicturedPlane Plane
    {
        get;
        init => field = Ensure.NotNull(value);
    } = Ensure.NotNull(Plane);
}

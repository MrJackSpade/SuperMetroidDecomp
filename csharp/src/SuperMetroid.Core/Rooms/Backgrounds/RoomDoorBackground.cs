namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The source room's BG1 during a door's opening scroll: it keeps the screen it showed when the door began, now moving
/// with the destination's scroll, and the destination shows everywhere else. The window is in BG1 scroll space, so it
/// covers exactly the scrolled positions the source screen occupied.
/// </summary>
/// <param name="SourceRoom">The source room's BG1 with its own scroll offset.</param>
/// <param name="WindowOrigin">The scrolled position of the source screen's top-left gameplay pixel.</param>
public sealed record RoomDoorBackground(RoomBackgroundLayer SourceRoom, Position16<BgPlanePixels> WindowOrigin)
{
    /// <summary>The window of a door that began with BG1 scrolled to <paramref name="sourceScroll"/>.</summary>
    /// <param name="sourceRoom">The source room's BG1.</param>
    /// <param name="sourceScroll">The source room's BG1 scroll registers when the door began.</param>
    public static RoomDoorBackground Begin(RoomBackgroundLayer sourceRoom, Position16<BgPlanePixels> sourceScroll) =>
        new(sourceRoom, sourceScroll + new Offset16<BgPlanePixels>(0,
            ScreenGeometry.BackgroundRowOffset + ScreenGeometry.GameplayHudHeightPixels));
}

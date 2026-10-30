using SuperMetroid.Core.Bosses.Draygon;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// The band of gameplay lines Draygon's body layer shows on (#1468): the whole gameplay area while his body is near the
/// screen's middle, the top band while it is high, the bottom band while it is low, and none once it is far off the
/// screen or deleted.
/// </summary>
public static class DraygonBodyBand
{
    /// <summary>The lines Draygon's body layer shows on against <paramref name="camera"/>.</summary>
    public static ScreenLineBand ShownLines(DraygonBodyPlacement body, Position16<RoomPixels> camera)
    {
        Ensure.NotNull(body);
        Offset16<ScreenPixels> screen = body.BodyPosition.RelativeToCamera(camera);
        int x = screen.X;
        int y = screen.Y;
        if (body.Deleted || x < DraygonBackgroundData.MinimumScreenX || x >= DraygonBackgroundData.MaximumScreenX ||
            y < DraygonBackgroundData.MinimumScreenY || y >= DraygonBackgroundData.MaximumScreenY)
            return new(DisplayScreen.HudHeight, DisplayScreen.HudHeight);
        if (y < DraygonBackgroundData.MiddleScreenY) return new(DisplayScreen.HudHeight, DraygonBackgroundData.TopBandEnd);
        if (y < DraygonBackgroundData.BottomScreenY) return ScreenLineBand.Gameplay;
        return new(DraygonBackgroundData.BottomBandStart, DisplayScreen.Height);
    }
}

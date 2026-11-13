using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.World;

/// <summary>
/// The background scroll phase: scroll offsets from the camera and the camera position adoption. Backgrounds render from
/// level data through these offsets, so nothing streams into the backgrounds (#1346).
/// </summary>
internal sealed class BackgroundScrollPhase
{
    /// <summary>Layer scroll owner driven by the camera.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Active room camera and door mode.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the background phase to shared scroll state and room ownership.</summary>
    internal BackgroundScrollPhase(BackgroundScrollState backgroundScroll, ActiveRoomState room)
    {
        _backgroundScroll = backgroundScroll;
        _room = room;
    }

    /// <summary>Copies the camera into the background scroll bookkeeping and steps the scroll offsets.</summary>
    public void UpdateBackgroundScrollingFromCamera(bool timeIsFrozen = false)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before the backgrounds scroll.");

        _backgroundScroll.ForegroundPosition = _room.Camera.PixelPosition;
        _backgroundScroll.StepScrolling(timeIsFrozen);
    }

    /// <summary>Adopts the camera position whose fractions survived the previous runtime.</summary>
    internal void AdoptCameraPosition(FixedPosition<RoomPixels> retained) => _room.InheritedCameraPosition = retained;
}

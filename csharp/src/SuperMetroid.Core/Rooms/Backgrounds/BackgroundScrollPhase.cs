using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// The background scroll phase: scroll registers from the camera and layer-1 position adoption. Backgrounds render from
/// level data through these registers, so nothing streams into VRAM (#1346).
/// </summary>
internal sealed class BackgroundScrollPhase
{
    /// <summary>Scroll-register owner driven by the camera.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Active room camera and door mode.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Binds the background phase to shared scroll state and room ownership.</summary>
    internal BackgroundScrollPhase(BackgroundScrollState backgroundScroll, ActiveRoomState room)
    {
        _backgroundScroll = backgroundScroll;
        _room = room;
    }

    /// <summary>Copies the translated layer-1 camera into <c>$80:A3AB</c>'s scroll bookkeeping and steps the registers.</summary>
    public void UpdateBackgroundScrollingFromCamera(bool timeIsFrozen = false)
    {
        if (_room.Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before BG scrolling.");

        _backgroundScroll.Layer1Position = _room.Camera.PixelPosition;
        _backgroundScroll.StepScrolling(timeIsFrozen);
    }

    /// <summary>Adopts the layer-one position whose fraction words survived the previous runtime.</summary>
    internal void AdoptLayer1Position(FixedPosition<RoomPixels> retained) => _room.InheritedLayer1Position = retained;
}

using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// The background scroll phase: scroll offsets from the camera. Backgrounds render from
/// level data through these offsets, so nothing streams into the backgrounds (#1346).
/// </summary>
internal sealed class BackgroundScrollPhase
{
    /// <summary>Layer scroll owner driven by the camera.</summary>
    private readonly BackgroundScrollState _backgroundScroll;

    /// <summary>Binds the background phase to shared scroll state.</summary>
    internal BackgroundScrollPhase(BackgroundScrollState backgroundScroll) => _backgroundScroll = backgroundScroll;

    /// <summary>Gives the background scroll the loaded room's camera and steps the scroll offsets.</summary>
    public void UpdateBackgroundScrollingFromCamera(ScrollBoundaryCamera camera, bool timeIsFrozen)
    {
        _backgroundScroll.ForegroundPosition = camera.PixelPosition;
        _backgroundScroll.StepScrolling(timeIsFrozen);
    }
}

using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The camera's layer scrolls and the room shake one accepted gameplay frame shows together.
/// </summary>
/// <param name="ForegroundPosition">The camera position the frame's actors were drawn against.</param>
/// <param name="ForegroundScroll">The foreground's scroll.</param>
/// <param name="BackgroundScroll">The background's scroll.</param>
/// <param name="RoomShake">The room shake displayed with this frame.</param>
public readonly record struct GameplayRenderSnapshot(
    Position16<RoomPixels> ForegroundPosition,
    Position16<RoomPixels> ForegroundScroll,
    Position16<BackgroundPixels> BackgroundScroll,
    RoomShakeFrameResult RoomShake);

using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The camera, the layers' views and the room shake one accepted gameplay frame shows together (#1555).
/// </summary>
/// <param name="ForegroundPosition">The camera position the frame's actors were drawn against.</param>
/// <param name="ForegroundScroll">The room pixel the foreground's view starts at.</param>
/// <param name="BackgroundScroll">The background pixel the background's view starts at.</param>
/// <param name="RoomShake">The room shake displayed with this frame.</param>
public readonly record struct GameplayRenderSnapshot(
    Position16<RoomPixels> ForegroundPosition,
    Position16<RoomPixels> ForegroundScroll,
    Position16<BackgroundPixels> BackgroundScroll,
    RoomShakeFrameResult RoomShake);

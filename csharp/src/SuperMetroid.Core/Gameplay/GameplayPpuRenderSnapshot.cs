using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Gameplay;

/// <summary>
/// The camera's layer scrolls and the room shake one accepted gameplay frame shows together.
/// </summary>
/// <param name="Layer1Position">The camera/layer-one position the frame's actors were drawn against.</param>
/// <param name="Bg1Scroll">Layer 1's scroll.</param>
/// <param name="Bg2Scroll">Layer 2's scroll.</param>
/// <param name="RoomShake">The room shake displayed with this frame.</param>
public readonly record struct GameplayPpuRenderSnapshot(
    Position16<RoomPixels> Layer1Position,
    Position16<RoomPixels> Bg1Scroll,
    Position16<Bg2Pixels> Bg2Scroll,
    RoomShakeFrameResult RoomShake);

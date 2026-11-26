using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// One published gameplay display: the latest frame-boundary latch, with the colours it latched, and the display state
/// read while the frame is shown (sampled after the update). Immutable; capturing it any number of times yields the same
/// picture and never advances simulation.
/// </summary>
/// <param name="Latched">Values the most recent frame boundary made visible.</param>
/// <param name="FrameEnd">The door layers and screen-effect inputs sampled at publication.</param>
public sealed partial record GameplayDisplayFrame(
    GameplayDisplayLatch Latched,
    GameplayFrameEndDisplayInputs FrameEnd)
{
    /// <summary>
    /// The colours and displayed sprites the frame boundary latched with the room's frame (#1403, #3036).
    /// </summary>
    /// <exception cref="InvalidOperationException">No frame boundary has latched a room's frame yet.</exception>
    public FramePaint Paint => Latched is RoomFrameLatch room ? room.Colours
        : throw new InvalidOperationException("A gameplay frame shows colours once a frame boundary has latched a room's frame.");
}

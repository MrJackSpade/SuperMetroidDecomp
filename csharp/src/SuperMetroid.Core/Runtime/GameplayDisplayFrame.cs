using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// One published gameplay display: the latest frame-boundary latch, the display state read while
/// the frame is shown (sampled after the update), and the frame's colours. Immutable; capturing it
/// any number of times yields the same picture and never advances simulation.
/// </summary>
/// <param name="Latched">Values the most recent frame boundary made visible.</param>
/// <param name="FrameEnd">The door layers and per-line effect inputs sampled at publication.</param>
/// <param name="Paint">The colours and displayed sprites at publication (#1403).</param>
public sealed record GameplayDisplayFrame(
    GameplayDisplayLatch Latched,
    GameplayFrameEndDisplayInputs FrameEnd,
    FramePaint Paint);

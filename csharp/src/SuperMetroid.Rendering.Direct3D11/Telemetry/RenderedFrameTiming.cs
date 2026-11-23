using SuperMetroid.Rendering.Direct3D11.Presentation;

namespace SuperMetroid.Rendering.Direct3D11.Telemetry;

/// <summary>
/// How long the render worker spent on one packet (#2653): the CPU time to submit its composition, and the time to display
/// and present it, which includes any wait for the swap chain, as the frame log a measurement starts records them.
/// </summary>
/// <param name="Sequence">The packet's publication sequence, which names the host frame it shows.</param>
/// <param name="CompositionMilliseconds">The CPU time submitting the packet's composition.</param>
/// <param name="PresentMilliseconds">The time displaying and presenting it.</param>
/// <param name="Result">How the presentation ended.</param>
public readonly record struct RenderedFrameTiming(long Sequence, double CompositionMilliseconds, double PresentMilliseconds, D3D11PresentationResult Result);

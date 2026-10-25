namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What a host asks of one runtime frame beyond controller input (#633): the gameplay-state policy,
/// the cartridge audio calls the frame makes at their native positions, and host work immediately
/// after the frame boundary. Nothing here is retained after the frame or serialized with the runtime.
/// </summary>
/// <remarks>A default request is an ordinary state-eight frame: departure allowed, game time advancing, deferred audio, no boundary work.</remarks>
public sealed record RuntimeFrameRequest
{
    /// <summary>The outer dispatcher's state policy for this frame.</summary>
    public RuntimeFramePolicy Policy { get; init; } = RuntimeFramePolicy.Gameplay;

    /// <summary>How the frame's echo and low-health audio calls reach the cartridge audio queue.</summary>
    public RuntimeFrameAudio Audio { get; init; } = RuntimeFrameAudio.Deferred;

    /// <summary>
    /// Host work immediately after the ordinary frame boundary and before the per-line effects and main-loop owners;
    /// the suspended message-box frame path does not run it.
    /// </summary>
    public Action AfterFrameBoundary { get; init; } = static () => { };
}

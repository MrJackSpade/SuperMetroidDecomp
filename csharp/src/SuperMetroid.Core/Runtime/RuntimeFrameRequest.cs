namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What a host asks of one runtime frame beyond controller input (#633): the gameplay-state policy,
/// the audio calls the frame makes at their usual positions, and host work immediately
/// after the frame boundary. Nothing here is retained after the frame.
/// </summary>
/// <remarks>A default request is an ordinary main-gameplay frame: departure allowed, game time advancing, no low-health check, no boundary work.</remarks>
public sealed record RuntimeFrameRequest
{
    /// <summary>The outer game-state loop's state policy for this frame.</summary>
    public RuntimeFramePolicy Policy { get; init; } = RuntimeFramePolicy.Gameplay;

    /// <summary>Whether the frame runs Samus's low-health check.</summary>
    public RuntimeFrameAudio Audio { get; init; } = RuntimeFrameAudio.WithoutLowHealthCheck;

    /// <summary>
    /// Host work immediately after the ordinary frame boundary and before the liquid wave, statue descent and main-loop owners;
    /// the suspended message-box frame path does not run it.
    /// </summary>
    public Action AfterFrameBoundary { get; init; } = static () => { };
}

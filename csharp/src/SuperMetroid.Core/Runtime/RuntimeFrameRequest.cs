namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What a host asks of one runtime frame beyond controller input (#633): the gameplay-state policy, including whether
/// Samus's low-health check runs, and host work immediately after the frame boundary. Nothing here is retained after the
/// frame.
/// </summary>
/// <remarks>A default request is an ordinary main-gameplay frame: departure allowed, game time advancing, no low-health check, no boundary work.</remarks>
public sealed record RuntimeFrameRequest
{
    /// <summary>The outer game-state loop's state policy for this frame.</summary>
    public RuntimeFramePolicy Policy { get; init; } = RuntimeFramePolicy.Gameplay;

    /// <summary>
    /// Host work immediately after the ordinary frame boundary and before the liquid wave, statue descent and the update's
    /// owners, given <see cref="Policy"/> and returning the policy the rest of the update runs under: the work can move the
    /// game to another state before the update's gameplay runs, as the reserve refill's completion returns it to main
    /// gameplay (#2526). The suspended message-box frame path does not run it.
    /// </summary>
    public Func<RuntimeFramePolicy, RuntimeFramePolicy> AfterFrameBoundary { get; init; } = static policy => policy;
}

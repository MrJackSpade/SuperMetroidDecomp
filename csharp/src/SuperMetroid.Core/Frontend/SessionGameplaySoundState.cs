using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The gameplay states a session's sound queues test as each request is made (#1620): its live gameplay runtime's attract
/// demo and its loaded room's power bomb, read at the moment of the request; none while no runtime is loaded.
/// </summary>
/// <param name="session">The session whose live runtime is read.</param>
internal sealed class SessionGameplaySoundState(FrontendSession session) : IGameplaySoundState
{
    /// <summary>The session whose live runtime is read.</summary>
    private readonly FrontendSession session = Ensure.NotNull(session);

    /// <inheritdoc/>
    public bool AttractDemoPlaying => session.Runtime is SuperMetroidRuntime runtime && runtime.AttractSession.IsAttractDemo;

    /// <inheritdoc/>
    public bool PowerBombExploding => session.Runtime is SuperMetroidRuntime runtime && runtime.PowerBombExplosionActive;
}

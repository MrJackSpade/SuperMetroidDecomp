using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The gameplay states a session's sound queues test as each request is made (#1620): its running door transition, its live
/// gameplay runtime's attract demo and its loaded room's power bomb, read at the moment of the request; no demo or power bomb
/// while no runtime is loaded.
/// </summary>
/// <param name="session">The session whose live runtime is read.</param>
internal sealed partial class SessionGameplaySoundState(FrontendSession session) : IGameplaySoundState
{
    /// <summary>The session whose live runtime is read.</summary>
    private readonly FrontendSession session = Ensure.NotNull(session);

    /// <inheritdoc/>
    /// <remarks>The door transition's phase is the one owner of the switch (#2499): its transitioning step disables sounds.</remarks>
    public bool DoorTransitionDisablesSounds => session.TransitioningThroughDoor;

    /// <inheritdoc/>
    public bool AttractDemoPlaying => session.Runtime is SuperMetroidRuntime runtime && runtime.AttractSession.IsAttractDemo;

    /// <inheritdoc/>
    public bool PowerBombExploding => session.Runtime is SuperMetroidRuntime runtime && runtime.PowerBombExplosionActive;
}

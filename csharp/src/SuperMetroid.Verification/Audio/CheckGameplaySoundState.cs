namespace SuperMetroid.Core.Audio;

/// <summary>
/// The gameplay states a check's standalone sound queues test as each request is made (#1620): none holds until the
/// check sets it, as a session with no runtime loaded and no door transition reports.
/// </summary>
internal sealed class CheckGameplaySoundState : IGameplaySoundState
{
    /// <inheritdoc/>
    public bool DoorTransitionDisablesSounds { get; set; }

    /// <inheritdoc/>
    public bool AttractDemoPlaying { get; set; }

    /// <inheritdoc/>
    public bool PowerBombExploding { get; set; }
}

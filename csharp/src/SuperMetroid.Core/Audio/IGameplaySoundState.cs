namespace SuperMetroid.Core.Audio;

/// <summary>
/// The gameplay states the sound queues read, as each request is made, to decide whether an admitted request is turned away
/// (<see cref="SoundSuppression"/>, #1620): whether a door's transition has disabled sound effects, whether an attract demo
/// drives the game and whether a power bomb is exploding. A session reports its running door transition and its live
/// gameplay runtime's demo and power bomb; with no runtime loaded neither of those holds.
/// </summary>
internal interface IGameplaySoundState
{
    /// <summary>
    /// Whether a door's transition has disabled sound effects (#2499): from the entry's stop requests, once the transition has
    /// begun, until the next room has faded in. Queued sounds and music still advance.
    /// </summary>
    bool DoorTransitionDisablesSounds { get; }

    /// <summary>Whether an attract demo drives the game.</summary>
    bool AttractDemoPlaying { get; }

    /// <summary>Whether a power bomb's explosion, or Crystal Flash's, is under way in the loaded room.</summary>
    bool PowerBombExploding { get; }
}

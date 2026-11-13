namespace SuperMetroid.Core.Audio;

/// <summary>
/// The gameplay states the sound queues read, as each request is made, to decide whether an admitted request is turned away
/// (<see cref="SoundSuppression"/>, #1620): whether an attract demo drives the game and whether a power bomb is exploding.
/// A session reports its live gameplay runtime's; with no runtime loaded neither holds.
/// </summary>
internal interface IGameplaySoundState
{
    /// <summary>Whether an attract demo drives the game.</summary>
    bool AttractDemoPlaying { get; }

    /// <summary>Whether a power bomb's explosion, or Crystal Flash's, is under way in the loaded room.</summary>
    bool PowerBombExploding { get; }
}

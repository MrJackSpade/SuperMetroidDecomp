namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Whether a Waver glides, spins or has just finished a spin (#2450): the animation it shows, and the update that ends a spin.
/// Its values are arbitrary.
/// </summary>
/// <remarks>
/// SpinFinished is a state of its course, not a request another owner stores for it (#2562): a cue, a touch or a shot ends
/// the state before, and the game acts on it one update later, the state naming where it stands until then; the update
/// that acts on it moves it on, as every other state's does. The update between is the game's.
/// </remarks>
public enum WaverAction
{
    /// <summary>Flies its wave in its steady pose.</summary>
    Gliding,

    /// <summary>Flies its wave while its four-frame spin plays.</summary>
    Spinning,

    /// <summary>
    /// The spin has finished, as its last cue says: the next update, wherever its flight takes it, ends by showing the
    /// steady pose for its facing again. The update between the cue and the steady pose is the game's.
    /// </summary>
    SpinFinished,
}

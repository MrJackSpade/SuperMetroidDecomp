namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>
/// Whether a Waver glides, spins or has just finished a spin (#2450): the animation it shows, and the update that ends a spin.
/// Its values are arbitrary.
/// </summary>
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

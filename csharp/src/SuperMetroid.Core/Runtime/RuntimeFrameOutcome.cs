using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What one runtime frame's update started that the outer game-state loop acts on after it (#2499, #2509, #2514, #2517,
/// #2518): the game state its steps move the game to, where its out-of-health check sends Samus, and the save a message
/// box closing in the update completed. The loop reads the frame's own outcome; no outcome outlives the update that
/// produced it.
/// </summary>
/// <param name="Transition">
/// The game state the update's steps move the game to, which the game-state loop enters unless the out-of-health check
/// sends Samus to the reserves' refill or her death.
/// </param>
/// <param name="OutOfHealth">Where the update's out-of-health check sends Samus.</param>
public readonly partial record struct RuntimeFrameOutcome(GameplayUpdateTransition Transition, SamusOutOfHealthRoute OutOfHealth)
{
    /// <summary>
    /// Whether the update started the time-up: the escape countdown ran out during it, whitening the target colours and
    /// stopping the colour effects, or, once the Zebes time bomb is set, the countdown had run out and the time-up did not
    /// own the update, as after the AUTO reserves' refill took an update from it (#2518). Every other transition the
    /// update started gives way to it.
    /// </summary>
    public bool StartsTimeUp => Transition == GameplayUpdateTransition.TimeUp;

    /// <summary>
    /// The save the update's closing message box completed, at a save station or the gunship, which the game-state loop
    /// writes to the selected slot after the update; null when the update completed none.
    /// </summary>
    public SaveStationPersistenceRequest? CompletedSave { get; init; }
}

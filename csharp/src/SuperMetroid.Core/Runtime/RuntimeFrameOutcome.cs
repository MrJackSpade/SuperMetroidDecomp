using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Saves;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What one runtime frame's update started that the outer game-state loop acts on after it (#2499, #2509, #2514): the
/// time-up the escape countdown starts from inside gameplay as it runs out, where the update's out-of-health check sends
/// Samus, and the save a message box closing in the update completed. The loop reads the frame's own outcome; no outcome
/// outlives the update that produced it.
/// </summary>
/// <param name="StartsTimeUp">
/// Whether the escape countdown ran out during the update: it whitened the target colours and stopped the colour effects,
/// and the game-state loop starts the time-up unless the same update's out-of-health check sends Samus to the reserves'
/// refill.
/// </param>
/// <param name="OutOfHealth">Where the update's out-of-health check sends Samus.</param>
public readonly record struct RuntimeFrameOutcome(bool StartsTimeUp, SamusOutOfHealthRoute OutOfHealth)
{
    /// <summary>
    /// The save the update's closing message box completed, at a save station or the gunship, which the game-state loop
    /// writes to the selected slot after the update; null when the update completed none.
    /// </summary>
    public SaveStationPersistenceRequest? CompletedSave { get; init; }
}

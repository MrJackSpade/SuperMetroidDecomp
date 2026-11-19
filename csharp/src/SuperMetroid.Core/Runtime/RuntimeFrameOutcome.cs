using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What one runtime frame's update started that the outer game-state loop acts on after it (#2499, #2509, #2518): the
/// time-up the escape countdown starts from inside gameplay as it runs out, or that the update returns to once it has, and
/// where the update's out-of-health check sends Samus. The loop reads the frame's own outcome; no outcome outlives the
/// update that produced it.
/// </summary>
/// <param name="StartsTimeUp">
/// Whether the update started the time-up: the escape countdown ran out during it, whitening the target colours and
/// stopping the colour effects, or, once the Zebes time bomb is set, the countdown had run out and the time-up did not own
/// the update, as after the AUTO reserves' refill took an update from it. The game-state loop starts the time-up unless
/// the same update's out-of-health check sends Samus to the reserves' refill.
/// </param>
/// <param name="OutOfHealth">Where the update's out-of-health check sends Samus.</param>
public readonly record struct RuntimeFrameOutcome(bool StartsTimeUp, SamusOutOfHealthRoute OutOfHealth);

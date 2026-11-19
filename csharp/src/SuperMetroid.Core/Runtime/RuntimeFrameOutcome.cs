using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What one runtime frame's update started that the outer game-state loop acts on after it (#2499, #2509, #2517): the game
/// state its steps move the game to, and where its out-of-health check sends Samus. The loop reads the frame's own
/// outcome; no outcome outlives the update that produced it.
/// </summary>
/// <param name="Transition">
/// The game state the update's steps move the game to, which the game-state loop enters unless the out-of-health check
/// sends Samus to the reserves' refill or her death.
/// </param>
/// <param name="OutOfHealth">Where the update's out-of-health check sends Samus.</param>
public readonly record struct RuntimeFrameOutcome(GameplayUpdateTransition Transition, SamusOutOfHealthRoute OutOfHealth)
{
    /// <summary>
    /// Whether the escape countdown ran out during the update: it whitened the target colours and stopped the colour
    /// effects. Every other transition the update started gives way to it.
    /// </summary>
    public bool StartsTimeUp => Transition == GameplayUpdateTransition.TimeUp;
}

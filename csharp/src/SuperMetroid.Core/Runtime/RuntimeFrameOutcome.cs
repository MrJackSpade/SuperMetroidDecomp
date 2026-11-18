namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What one runtime frame's update started that the outer game-state loop acts on after it (#2499): nothing, or the
/// time-up the escape countdown starts from inside gameplay as it runs out. The loop reads the frame's own outcome; no
/// outcome outlives the update that produced it.
/// </summary>
public enum RuntimeFrameOutcome
{
    /// <summary>The update started nothing for the game-state loop.</summary>
    Ordinary,

    /// <summary>
    /// The escape countdown ran out during the update: it whitened the target colours and stopped the colour effects, and
    /// the game-state loop starts the time-up unless the same update's death check takes the frame first.
    /// </summary>
    TimeUp,
}

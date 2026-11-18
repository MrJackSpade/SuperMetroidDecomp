namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The behaviour a Choot's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum ChootAction
{
    /// <summary>Waits for Samus within 80 horizontal pixels, then loads the population's jump delay.</summary>
    WaitingForSamus,
    /// <summary>Waits out the jump delay (<see cref="ChootEnemyState.JumpDelayTimer"/>), then selects the jump animation and ascent state.</summary>
    PreparingJump,
    /// <summary>Applies the quadratic speed curve's negative Y displacement until the ascent's place passes the curve's start, then snaps whole coordinates to the precalculated falling origin.</summary>
    Jumping,
    /// <summary>Plays and repeats the selected run of X/Y offsets, advancing its Y origin between loops, then restores the spawn position and rearms proximity activation.</summary>
    Falling,
}

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The behaviour a Choot's main AI runs each frame.
/// Its values are arbitrary.
/// </summary>
public enum ChootEnemyFunction
{
    /// <summary>Waits for Samus within 80 horizontal pixels, then loads the population's jump delay.</summary>
    WaitingForSamus = 1,
    /// <summary>Decrements the jump delay through signed underflow before selecting the jump animation and ascent state.</summary>
    PreparingJump,
    /// <summary>Applies negative quadratic-table Y displacement until the ascent cursor underflows, then snaps whole coordinates to the precalculated falling origin.</summary>
    Jumping,
    /// <summary>Plays and repeats the selected X/Y offset stream, advancing its Y origin between loops, then restores the spawn position and rearms proximity activation.</summary>
    Falling,
}

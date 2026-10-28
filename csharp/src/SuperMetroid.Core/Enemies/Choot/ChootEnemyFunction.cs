namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The routine a Choot's main AI runs each frame.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum ChootEnemyFunction
{
    /// <summary><c>$A2:E035</c>, native <c>Function_Choot_WaitForSamusToGetNear</c>: waits for Samus within 80 horizontal pixels, then loads the population's jump delay.</summary>
    WaitingForSamus = 1,
    /// <summary><c>$A2:E04F</c>, native <c>Function_Choot_PrepareToJump</c>: decrements the jump delay through signed underflow before selecting the jump animation and ascent state.</summary>
    PreparingJump,
    /// <summary><c>$A2:E06A</c>, native <c>Function_Choot_Jumping</c>: applies negative quadratic-table Y displacement until the ascent cursor underflows, then snaps whole coordinates to the precalculated falling origin.</summary>
    Jumping,
    /// <summary><c>$A2:E0CD</c>, native <c>Function_Choot_Falling</c>: plays and repeats the selected X/Y offset stream, advancing its Y origin between loops, then restores the spawn position and rearms proximity activation.</summary>
    Falling,
}

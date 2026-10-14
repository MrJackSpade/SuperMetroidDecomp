namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A2 function pointer stored in a Choot's variable A.</summary>
public enum ChootEnemyFunction : ushort
{
    /// <summary><c>$A2:E035</c>, native <c>Function_Choot_WaitForSamusToGetNear</c>: waits for Samus within 80 horizontal pixels, then loads the population's jump delay.</summary>
    WaitingForSamus = 0xe035,
    /// <summary><c>$A2:E04F</c>, native <c>Function_Choot_PrepareToJump</c>: decrements the jump delay through signed underflow before selecting the jump animation and ascent state.</summary>
    PreparingJump = 0xe04f,
    /// <summary><c>$A2:E06A</c>, native <c>Function_Choot_Jumping</c>: applies negative quadratic-table Y displacement until the ascent cursor underflows, then snaps whole coordinates to the precalculated falling origin.</summary>
    Jumping = 0xe06a,
    /// <summary><c>$A2:E0CD</c>, native <c>Function_Choot_Falling</c>: plays and repeats the selected X/Y offset stream, advancing its Y origin between loops, then restores the spawn position and rearms proximity activation.</summary>
    Falling = 0xe0cd,
}

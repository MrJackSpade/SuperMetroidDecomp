namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A6 function pointer stored in Kzan top variable A. These are kept as ROM
/// addresses so debugger watches agree directly with <c>$0FA8,x</c> and corrupt/custom
/// population state fails at the exact indirect target instead of becoming a plausible enum.
/// </summary>
public enum KzanEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Kzan_WaitingToFall</c> at <c>$A6:8BB4</c>; counts down the population-defined top wait before resetting fall acceleration.</summary>
    WaitingToFall = 0x8bb4,
    /// <summary>Native <c>Function_Kzan_Falling</c> at <c>$A6:8BDC</c>; accelerates downward, carries a touching rider, and clamps to the lower target on landing.</summary>
    Falling = 0x8bdc,
    /// <summary>Native <c>Function_Kzan_WaitingToRise</c> at <c>$A6:8C4A</c>; waits at the lower target until the post-landing countdown expires.</summary>
    WaitingToRise = 0x8c4a,
    /// <summary>Native <c>Function_Kzan_Rising</c> at <c>$A6:8C5D</c>; returns upward at one-half pixel per NTSC actor update and resumes the top wait.</summary>
    Rising = 0x8c5d,
}

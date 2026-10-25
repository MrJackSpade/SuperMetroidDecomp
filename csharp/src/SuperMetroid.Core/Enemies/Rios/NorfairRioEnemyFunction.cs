namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The routines Norfair Rio's main AI dispatches.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum NorfairRioEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Geruta_Flames</c> at <c>$A2:C281</c>; follows the preceding parent using its signed animation-authored Y offset, hides for parent freeze/idle, and deletes the follower when the parent dies.</summary>
    FollowParent = 1,
    /// <summary>Native <c>Function_Geruta_Idle</c> at <c>$A2:C2E7</c>; admits an attack when RNG mask <c>$0101</c> is nonzero and Samus is strictly within 192 horizontal room pixels, sampling a launch velocity from that RNG word.</summary>
    WaitForAttackOpportunity,
    /// <summary>Native <c>Function_Geruta_StartSwoop</c> at <c>$A2:C33F</c>; waits for the animation handshake, then installs descending artwork, enters the dive, and requests library-two sound <c>$65</c>.</summary>
    WaitForTakeoffAnimation,
    /// <summary>Native <c>Function_Geruta_Swoop_Descending</c> at <c>$A2:C361</c>; moves down and horizontally with wall reversal, transitioning to return on vertical collision or when vertical velocity becomes negative.</summary>
    Dive,
    /// <summary>Native <c>Function_Geruta_Swoop_Ascending</c> at <c>$A2:C3B1</c>; accelerates upward while moving horizontally and responding to animation signals, ending on vertical collision rather than a saved perch coordinate.</summary>
    ReturnToPerch,
    /// <summary>Native <c>Function_Geruta_FinishSwoop</c> at <c>$A2:C406</c>; one-update state that restores only the idle dispatch address, leaving the existing looping animation list and follower visibility intact.</summary>
    FinishLanding,
}

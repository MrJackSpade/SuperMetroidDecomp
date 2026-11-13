namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours Norfair Rio's main AI dispatches.
/// Its values are arbitrary.
/// </summary>
public enum NorfairRioEnemyFunction
{
    /// <summary>Follows the preceding parent using its signed animation-authored Y offset, hides for parent freeze/idle, and deletes the follower when the parent dies.</summary>
    FollowParent = 1,
    /// <summary>Admits an attack when either half of the random sample is odd and Samus is strictly within 192 horizontal room pixels, sampling a launch velocity from that sample.</summary>
    WaitForAttackOpportunity,
    /// <summary>Waits for the animation handshake, then installs descending artwork, enters the dive, and requests the enemy-library dive sound.</summary>
    WaitForTakeoffAnimation,
    /// <summary>Moves down and horizontally with wall reversal, transitioning to return on vertical collision or when vertical velocity becomes negative.</summary>
    Dive,
    /// <summary>Accelerates upward while moving horizontally and responding to animation signals, ending on vertical collision rather than a saved perch coordinate.</summary>
    ReturnToPerch,
    /// <summary>One-update state that restores only the idle dispatch, leaving the existing looping animation list and follower visibility intact.</summary>
    FinishLanding,
}

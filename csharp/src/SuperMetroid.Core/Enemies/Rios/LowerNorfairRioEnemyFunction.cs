namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Exact bank-$A2 function words dispatched by Lower Norfair Rio main AI.</summary>
public enum LowerNorfairRioEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Holtz_Flames</c> at <c>$A2:C72E</c>; follows the immediately preceding parent twelve pixels below, mirrors its freeze state and flame visibility, and deletes the follower when the parent dies.</summary>
    FollowParent = 0xc72e,
    /// <summary>Native <c>Function_Holtz_Idle</c> at <c>$A2:C771</c>; admits an attack when the RNG mask <c>$0101</c> is nonzero and Samus is strictly within 112 horizontal room pixels.</summary>
    WaitForAttackOpportunity = 0xc771,
    /// <summary>Native <c>Function_Holtz_PrepareToSwoop</c> at <c>$A2:C7BB</c>; waits for the animation instruction's completion signal before clearing it and starting downward motion.</summary>
    WaitForTakeoffAnimation = 0xc7bb,
    /// <summary>Native <c>Function_Holtz_Swoop_Descending</c> at <c>$A2:C7D6</c>; moves horizontally and downward, reverses X velocity on walls, and transitions to return on vertical collision or a negative vertical velocity.</summary>
    Dive = 0xc7d6,
    /// <summary>Native <c>Function_Holtz_Swoop_Ascending</c> at <c>$A2:C82D</c>; accelerates upward while retaining horizontal movement, switches ascent artwork on the animation signal, and enters landing wait on vertical collision rather than targeting a saved Y.</summary>
    ReturnToPerch = 0xc82d,
    /// <summary>Native <c>Function_Holtz_SwoopCooldown</c> at <c>$A2:C888</c>; consumes the landing animation's signal, installs cooldown artwork, and resumes attack-opportunity waiting.</summary>
    WaitForLandingAnimation = 0xc888,
}

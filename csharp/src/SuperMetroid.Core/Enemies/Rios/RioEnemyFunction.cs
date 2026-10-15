namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>Exact bank-$A2 function words dispatched by Rio main AI at $A2:BBE3.</summary>
public enum RioEnemyFunction : ushort
{
    /// <summary>$A2:BBED Function_Rio_WaitForSamusToGetNear: starts a dive when Samus is within a strict 160-world-pixel horizontal distance, selecting launch direction toward her.</summary>
    WaitingForSamus = 0xbbed,
    /// <summary>$A2:BC32 Function_Rio_SwoopCooldown: waits for the $A2:BBC3 animation handshake, then installs the post-swoop idle list and resumes proximity waiting.</summary>
    WaitingForLandingAnimation = 0xbc32,
    /// <summary>$A2:BC48 Function_Rio_Swoop_Descending: moves with signed 8.8 velocities, subtracting $0018 from Y velocity per AI update until homing begins or collision starts the bounce.</summary>
    Diving = 0xbc48,
    /// <summary>$A2:BCB7 Function_Rio_Swoop_Ascending: follows the upward bounce with signed 8.8 acceleration; horizontal collision reverses X, and vertical collision starts the cooldown animation.</summary>
    BouncingBackToPerch = 0xbcb7,
    /// <summary>$A2:BCFF Function_Rio_Homing: uses angle-table samples as 8.8 velocities while above Samus, then restores saved horizontal speed and begins ascent at Y velocity $FFFF.</summary>
    HoveringTowardSamus = 0xbcff,
}

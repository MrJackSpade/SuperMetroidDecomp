namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours Rio's main AI dispatches.
/// Its values are arbitrary.
/// </summary>
public enum RioEnemyFunction
{
    /// <summary>Starts a dive when Samus is within a strict 160-world-pixel horizontal distance, selecting launch direction toward her.</summary>
    WaitingForSamus = 1,
    /// <summary>Waits for the swoop animation's handshake, then installs the post-swoop idle animation and resumes proximity waiting.</summary>
    WaitingForLandingAnimation,
    /// <summary>Moves with signed 8.8 velocities, subtracting 24 from Y velocity per AI update until homing begins or collision starts the bounce.</summary>
    Diving,
    /// <summary>Follows the upward bounce with signed 8.8 acceleration; horizontal collision reverses X, and vertical collision starts the cooldown animation.</summary>
    BouncingBackToPerch,
    /// <summary>Uses angle-table samples as 8.8 velocities while above Samus, then restores saved horizontal speed and begins ascent at Y velocity -1.</summary>
    HoveringTowardSamus,
}

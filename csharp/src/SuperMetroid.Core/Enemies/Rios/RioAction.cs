namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// The behaviours Rio's main AI runs.
/// Its values are arbitrary.
/// </summary>
public enum RioAction
{
    /// <summary>Starts a dive when Samus is within a strict 160-world-pixel horizontal distance, selecting launch direction toward her.</summary>
    WaitingForSamus,
    /// <summary>Waits for its swoop's animation to finish, then plays the idle animation that follows and waits for Samus again.</summary>
    WaitingForLandingAnimation,
    /// <summary>Moves at its velocity, its vertical speed falling by its gravity each update, until homing begins or a collision starts the bounce.</summary>
    Diving,
    /// <summary>Follows the upward bounce under its gravity; a horizontal collision reverses its sideways speed, and a vertical collision starts the cooldown animation.</summary>
    BouncingBackToPerch,
    /// <summary>Moves along its bearing toward Samus while above her, then takes back its saved sideways speed and starts climbing at a 256th of a pixel an update.</summary>
    HoveringTowardSamus,
}

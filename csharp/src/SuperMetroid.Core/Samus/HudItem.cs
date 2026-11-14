namespace SuperMetroid.Core.Samus;

/// <summary>
/// The item selected on the HUD, which decides what Samus fires (#1323). Select steps through them in the order
/// <see cref="HudItems"/> keeps; the arm cannon's cover and every weapon's firing read this choice.
/// </summary>
public enum HudItem
{
    /// <summary>No HUD item: Samus fires her beam.</summary>
    Beam,
    /// <summary>Missiles.</summary>
    Missile,
    /// <summary>Super Missiles.</summary>
    SuperMissile,
    /// <summary>Power Bombs.</summary>
    PowerBomb,
    /// <summary>The Grapple Beam.</summary>
    Grapple,
    /// <summary>The X-ray Scope.</summary>
    XRay,
}

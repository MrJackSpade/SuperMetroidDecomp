namespace SuperMetroid.Core.Samus;

/// <summary>
/// The selected HUD item: the six items of the HUD selection (#1323). The selection ring,
/// the arm-cannon cover policy and every weapon handler index exactly this domain.
/// </summary>
public enum HudItem
{
    /// <summary>No HUD weapon; beam fire.</summary>
    Beam = 0,
    /// <summary>Missiles.</summary>
    Missile = 1,
    /// <summary>Super missiles.</summary>
    SuperMissile = 2,
    /// <summary>Power Bombs.</summary>
    PowerBomb = 3,
    /// <summary>Grapple Beam.</summary>
    Grapple = 4,
    /// <summary>X-ray Scope.</summary>
    XRay = 5,
}


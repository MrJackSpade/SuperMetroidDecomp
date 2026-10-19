namespace SuperMetroid.Core.Samus;

/// <summary>
/// WRAM <c>SelectedHUDItem</c>: the six entries of the bank-$90 HUD dispatchers (#1323). The selection ring at
/// <c>$90:C51D</c>, the arm-cannon cover policy and every weapon handler index exactly this domain.
/// </summary>
public enum HudItem : ushort
{
    /// <summary>SelectedHUDItem $0000: no HUD weapon; beam fire.</summary>
    Beam = 0,
    /// <summary>SelectedHUDItem $0001: missiles.</summary>
    Missile = 1,
    /// <summary>SelectedHUDItem $0002: super missiles.</summary>
    SuperMissile = 2,
    /// <summary>SelectedHUDItem $0003: Power Bombs.</summary>
    PowerBomb = 3,
    /// <summary>SelectedHUDItem $0004: Grapple Beam.</summary>
    Grapple = 4,
    /// <summary>SelectedHUDItem $0005: X-ray Scope.</summary>
    XRay = 5,
}


namespace SuperMetroid.Core.Game;

/// <summary>Fixed cartridge policy controlling the arm-cannon cover for each HUD item.</summary>
internal static class SamusArmCannonDefinitions
{

    /// <summary>Number of HUD selections accepted by the native arm-cannon dispatcher.</summary>
    public const int HudItemCount = 6;

    /// <summary>Native SelectedHUDItem values used to choose the arm-cannon cover policy.</summary>
    private enum HudSelection : ushort
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

    /// <summary>Returns the native desired cover state for a validated HUD-item index.</summary>
    public static byte DesiredOpenFlag(ushort selectedHudItem)
    {
        if (selectedHudItem >= HudItemCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedHudItem),
                selectedHudItem,
                $"HUD item must be in the native range 0..{HudItemCount - 1}.");
        }

        return (HudSelection)selectedHudItem switch
        {
            HudSelection.Missile or HudSelection.SuperMissile or HudSelection.Grapple => 1,
            HudSelection.Beam or HudSelection.PowerBomb or HudSelection.XRay => 0,
            _ => throw new InvalidOperationException("Validated HUD selection has no cover policy."),
        };
    }
}

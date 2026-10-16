namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Fixed cartridge policy controlling the arm-cannon cover for each HUD item.</summary>
internal static class SamusArmCannonDefinitions
{
    /// <summary>Returns the native desired cover state for a HUD item.</summary>
    public static byte DesiredOpenFlag(HudItem selectedHudItem) => selectedHudItem switch
    {
        HudItem.Missile or HudItem.SuperMissile or HudItem.Grapple => 1,
        HudItem.Beam or HudItem.PowerBomb or HudItem.XRay => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(selectedHudItem), selectedHudItem,
            "HUD item is outside the six-entry dispatcher."),
    };
}

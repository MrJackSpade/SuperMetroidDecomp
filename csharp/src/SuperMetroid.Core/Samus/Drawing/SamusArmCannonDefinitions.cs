namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Fixed cartridge policy controlling the arm-cannon cover for each HUD item.</summary>
internal static class SamusArmCannonDefinitions
{
    /// <summary>The value each key selects; DesiredOpenFlag looks it up.</summary>
    public static readonly IReadOnlyDictionary<HudItem, byte> DesiredOpenFlags = new Dictionary<HudItem, byte>
    {
        [HudItem.Missile] = 1,
        [HudItem.SuperMissile] = 1,
        [HudItem.Grapple] = 1,
        [HudItem.Beam] = 0,
        [HudItem.PowerBomb] = 0,
        [HudItem.XRay] = 0,
    };
}

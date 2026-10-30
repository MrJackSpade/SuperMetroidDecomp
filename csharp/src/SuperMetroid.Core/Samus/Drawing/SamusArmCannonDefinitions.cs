namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Fixed cartridge policy controlling the arm-cannon cover for each HUD item.</summary>
internal static class SamusArmCannonDefinitions
{
    /// <summary>Whether each HUD item opens the cover; Opens looks it up.</summary>
    public static readonly IReadOnlyDictionary<HudItem, bool> OpensCover = new Dictionary<HudItem, bool>
    {
        [HudItem.Missile] = true,
        [HudItem.SuperMissile] = true,
        [HudItem.Grapple] = true,
        [HudItem.Beam] = false,
        [HudItem.PowerBomb] = false,
        [HudItem.XRay] = false,
    };
}

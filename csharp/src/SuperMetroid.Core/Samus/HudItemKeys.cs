namespace SuperMetroid.Core.Samus;

/// <summary>
/// The installed key of each <see cref="HudItem"/> (#2520), written out so renaming a member never changes a name the
/// installation, an installed document or a save spells it by.
/// </summary>
internal static class HudItemKeys
{
    /// <summary>Each HudItem member's installed key.</summary>
    internal static InstalledKeys<HudItem> Installed { get; } = new(new Dictionary<HudItem, string>
    {
        [HudItem.Beam] = "Beam",
        [HudItem.Missile] = "Missile",
        [HudItem.SuperMissile] = "SuperMissile",
        [HudItem.PowerBomb] = "PowerBomb",
        [HudItem.Grapple] = "Grapple",
        [HudItem.XRay] = "XRay",
    });
}

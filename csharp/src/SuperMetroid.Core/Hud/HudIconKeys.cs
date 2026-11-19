namespace SuperMetroid.Core.Hud;

/// <summary>
/// The installed key of each <see cref="HudIcon"/> (#2520), written out so renaming a member never changes a name the
/// installation, an installed document or a save spells it by.
/// </summary>
internal static class HudIconKeys
{
    /// <summary>Each HudIcon member's installed key.</summary>
    internal static InstalledKeys<HudIcon> Installed { get; } = new(new Dictionary<HudIcon, string>
    {
        [HudIcon.Missile] = "Missile",
        [HudIcon.SuperMissile] = "SuperMissile",
        [HudIcon.PowerBomb] = "PowerBomb",
        [HudIcon.Grapple] = "Grapple",
        [HudIcon.XRay] = "XRay",
    });
}

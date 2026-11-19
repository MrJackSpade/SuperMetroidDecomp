namespace SuperMetroid.Core.Hud;

/// <summary>
/// The installed key of each <see cref="HudIconShade"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class HudIconShadeKeys
{
    /// <summary>Each HudIconShade member's installed key.</summary>
    internal static InstalledKeys<HudIconShade> Installed { get; } = new(new Dictionary<HudIconShade, string>
    {
        [HudIconShade.Selected] = "Selected",
        [HudIconShade.Deselected] = "Deselected",
    });
}

namespace SuperMetroid.Core.Hud;

/// <summary>
/// The installed key of each <see cref="HudIconCover"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class HudIconCoverKeys
{
    /// <summary>Each HudIconCover member's installed key.</summary>
    internal static InstalledKeys<HudIconCover> Installed { get; } = new(new Dictionary<HudIconCover, string>
    {
        [HudIconCover.Template] = "Template",
        [HudIconCover.Icon] = "Icon",
    });
}

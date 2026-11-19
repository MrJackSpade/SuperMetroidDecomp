namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The installed key of each <see cref="ZebesRevealActorRole"/> (#2520), written out so renaming a member never changes
/// a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class ZebesRevealActorRoleKeys
{
    /// <summary>Each ZebesRevealActorRole member's installed key.</summary>
    internal static InstalledKeys<ZebesRevealActorRole> Installed { get; } = new(new Dictionary<ZebesRevealActorRole, string>
    {
        [ZebesRevealActorRole.Planet] = "Planet",
        [ZebesRevealActorRole.StarsUpperLeft] = "StarsUpperLeft",
        [ZebesRevealActorRole.StarsUpperRight] = "StarsUpperRight",
        [ZebesRevealActorRole.StarsLowerLeft] = "StarsLowerLeft",
        [ZebesRevealActorRole.StarsLowerRight] = "StarsLowerRight",
        [ZebesRevealActorRole.Title] = "Title",
        [ZebesRevealActorRole.JapaneseSubtitle] = "JapaneseSubtitle",
    });
}

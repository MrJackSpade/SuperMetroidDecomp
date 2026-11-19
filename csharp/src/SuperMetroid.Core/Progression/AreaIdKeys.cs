namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="AreaId"/> (#2520), written out so renaming a member never changes a name the
/// installation, an installed document or a save spells it by.
/// </summary>
internal static class AreaIdKeys
{
    /// <summary>Each AreaId member's installed key.</summary>
    internal static InstalledKeys<AreaId> Installed { get; } = new(new Dictionary<AreaId, string>
    {
        [AreaId.Crateria] = "Crateria",
        [AreaId.Brinstar] = "Brinstar",
        [AreaId.Norfair] = "Norfair",
        [AreaId.WreckedShip] = "WreckedShip",
        [AreaId.Maridia] = "Maridia",
        [AreaId.Tourian] = "Tourian",
        [AreaId.Ceres] = "Ceres",
    });
}

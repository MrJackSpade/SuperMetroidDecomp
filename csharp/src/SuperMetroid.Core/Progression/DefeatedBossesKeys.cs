namespace SuperMetroid.Core.Progression;

/// <summary>
/// The installed key of each <see cref="DefeatedBosses"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class DefeatedBossesKeys
{
    /// <summary>Each DefeatedBosses member's installed key.</summary>
    internal static InstalledKeys<DefeatedBosses> Installed { get; } = new(new Dictionary<DefeatedBosses, string>
    {
        [DefeatedBosses.None] = "None",
        [DefeatedBosses.AreaBoss] = "AreaBoss",
        [DefeatedBosses.AreaMiniBoss] = "AreaMiniBoss",
        [DefeatedBosses.AreaTorizo] = "AreaTorizo",
    });
}

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>
/// The installed key of each <see cref="BossBodyOwner"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class BossBodyOwnerKeys
{
    /// <summary>Each BossBodyOwner member's installed key.</summary>
    internal static InstalledKeys<BossBodyOwner> Installed { get; } = new(new Dictionary<BossBodyOwner, string>
    {
        [BossBodyOwner.Kraid] = "Kraid",
        [BossBodyOwner.Phantoon] = "Phantoon",
        [BossBodyOwner.Draygon] = "Draygon",
        [BossBodyOwner.Crocomire] = "Crocomire",
        [BossBodyOwner.MotherBrain] = "MotherBrain",
    });
}

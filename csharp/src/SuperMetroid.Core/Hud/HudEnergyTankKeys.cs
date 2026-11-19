namespace SuperMetroid.Core.Hud;

/// <summary>
/// The installed key of each <see cref="HudEnergyTank"/> (#2520), written out so renaming a member never changes a name
/// the installation, an installed document or a save spells it by.
/// </summary>
internal static class HudEnergyTankKeys
{
    /// <summary>Each HudEnergyTank member's installed key.</summary>
    internal static InstalledKeys<HudEnergyTank> Installed { get; } = new(new Dictionary<HudEnergyTank, string>
    {
        [HudEnergyTank.Full] = "Full",
        [HudEnergyTank.Empty] = "Empty",
    });
}

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The installed key of each <see cref="CeresRearViewActorRole"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static class CeresRearViewActorRoleKeys
{
    /// <summary>Each CeresRearViewActorRole member's installed key.</summary>
    internal static InstalledKeys<CeresRearViewActorRole> Installed { get; } = new(new Dictionary<CeresRearViewActorRole, string>
    {
        [CeresRearViewActorRole.LargeAsteroids] = "LargeAsteroids",
        [CeresRearViewActorRole.StationUnderAttack] = "StationUnderAttack",
        [CeresRearViewActorRole.SmallAsteroids] = "SmallAsteroids",
        [CeresRearViewActorRole.Vortex] = "Vortex",
        [CeresRearViewActorRole.RearStars] = "RearStars",
    });
}

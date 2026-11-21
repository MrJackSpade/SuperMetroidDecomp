namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// The installed key of each <see cref="CeresDestructionActorRole"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class CeresDestructionActorRoleKeys
{
    /// <summary>Each CeresDestructionActorRole member's installed key.</summary>
    internal static InstalledKeys<CeresDestructionActorRole> Installed { get; } = new(new Dictionary<CeresDestructionActorRole, string>
    {
        [CeresDestructionActorRole.LargeAsteroids] = "LargeAsteroids",
        [CeresDestructionActorRole.SmallAsteroids] = "SmallAsteroids",
        [CeresDestructionActorRole.Vortex] = "Vortex",
    });
}

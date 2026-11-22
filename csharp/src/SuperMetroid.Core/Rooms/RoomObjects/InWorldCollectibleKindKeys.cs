namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The installed key of each <see cref="InWorldCollectibleKind"/> (#2520), written out so renaming a member never
/// changes a name the installation, an installed document or a save spells it by.
/// </summary>
internal static partial class InWorldCollectibleKindKeys
{
    /// <summary>Each InWorldCollectibleKind member's installed key.</summary>
    internal static InstalledKeys<InWorldCollectibleKind> Installed { get; } = new(new Dictionary<InWorldCollectibleKind, string>
    {
        [InWorldCollectibleKind.EnergyTank] = "EnergyTank",
        [InWorldCollectibleKind.MissileTank] = "MissileTank",
        [InWorldCollectibleKind.SuperMissileTank] = "SuperMissileTank",
        [InWorldCollectibleKind.PowerBombTank] = "PowerBombTank",
        [InWorldCollectibleKind.Bombs] = "Bombs",
        [InWorldCollectibleKind.ChargeBeam] = "ChargeBeam",
        [InWorldCollectibleKind.IceBeam] = "IceBeam",
        [InWorldCollectibleKind.HiJumpBoots] = "HiJumpBoots",
        [InWorldCollectibleKind.SpeedBooster] = "SpeedBooster",
        [InWorldCollectibleKind.WaveBeam] = "WaveBeam",
        [InWorldCollectibleKind.SpazerBeam] = "SpazerBeam",
        [InWorldCollectibleKind.SpringBall] = "SpringBall",
        [InWorldCollectibleKind.VariaSuit] = "VariaSuit",
        [InWorldCollectibleKind.GravitySuit] = "GravitySuit",
        [InWorldCollectibleKind.XrayScope] = "XrayScope",
        [InWorldCollectibleKind.PlasmaBeam] = "PlasmaBeam",
        [InWorldCollectibleKind.GrappleBeam] = "GrappleBeam",
        [InWorldCollectibleKind.SpaceJump] = "SpaceJump",
        [InWorldCollectibleKind.ScrewAttack] = "ScrewAttack",
        [InWorldCollectibleKind.MorphBall] = "MorphBall",
        [InWorldCollectibleKind.ReserveTank] = "ReserveTank",
    });
}

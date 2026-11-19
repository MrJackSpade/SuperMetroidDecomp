namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The installed key of each <see cref="GameplayMessageId"/> (#2520), written out so renaming a member never changes a
/// name the installation, an installed document or a save spells it by.
/// </summary>
internal static class GameplayMessageIdKeys
{
    /// <summary>Each GameplayMessageId member's installed key.</summary>
    internal static InstalledKeys<GameplayMessageId> Installed { get; } = new(new Dictionary<GameplayMessageId, string>
    {
        [GameplayMessageId.EnergyTank] = "EnergyTank",
        [GameplayMessageId.MissileTank] = "MissileTank",
        [GameplayMessageId.SuperMissileTank] = "SuperMissileTank",
        [GameplayMessageId.PowerBombTank] = "PowerBombTank",
        [GameplayMessageId.GrappleBeam] = "GrappleBeam",
        [GameplayMessageId.XrayScope] = "XrayScope",
        [GameplayMessageId.VariaSuit] = "VariaSuit",
        [GameplayMessageId.SpringBall] = "SpringBall",
        [GameplayMessageId.MorphBall] = "MorphBall",
        [GameplayMessageId.ScrewAttack] = "ScrewAttack",
        [GameplayMessageId.HiJumpBoots] = "HiJumpBoots",
        [GameplayMessageId.SpaceJump] = "SpaceJump",
        [GameplayMessageId.SpeedBooster] = "SpeedBooster",
        [GameplayMessageId.ChargeBeam] = "ChargeBeam",
        [GameplayMessageId.IceBeam] = "IceBeam",
        [GameplayMessageId.WaveBeam] = "WaveBeam",
        [GameplayMessageId.SpazerBeam] = "SpazerBeam",
        [GameplayMessageId.PlasmaBeam] = "PlasmaBeam",
        [GameplayMessageId.Bombs] = "Bombs",
        [GameplayMessageId.MapDataAccessCompleted] = "MapDataAccessCompleted",
        [GameplayMessageId.EnergyRechargeCompleted] = "EnergyRechargeCompleted",
        [GameplayMessageId.MissileRechargeCompleted] = "MissileRechargeCompleted",
        [GameplayMessageId.SaveConfirmation] = "SaveConfirmation",
        [GameplayMessageId.SaveCompleted] = "SaveCompleted",
        [GameplayMessageId.ReserveTank] = "ReserveTank",
        [GameplayMessageId.GravitySuit] = "GravitySuit",
        [GameplayMessageId.GunshipSaveConfirmation] = "GunshipSaveConfirmation",
    });
}

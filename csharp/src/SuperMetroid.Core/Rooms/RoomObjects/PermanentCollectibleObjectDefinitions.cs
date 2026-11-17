namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The permanent-item room objects: one for each collectible kind some room places in each of the exposed, Chozo-orb and
/// concealed shot-block presentations (#2354).
/// </summary>
internal static class PermanentCollectibleObjectDefinitions
{
    /// <summary>The item and presentation of each permanent-item room object.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, PermanentCollectibleObject> Items { get; } = new Dictionary<RoomObjectKind, PermanentCollectibleObject>
    {
        [RoomObjectKind.ExposedEnergyTank] = new(InWorldCollectibleKind.EnergyTank, CollectiblePresentation.Exposed),
        [RoomObjectKind.ExposedMissileTank] = new(InWorldCollectibleKind.MissileTank, CollectiblePresentation.Exposed),
        [RoomObjectKind.ExposedSuperMissileTank] = new(InWorldCollectibleKind.SuperMissileTank, CollectiblePresentation.Exposed),
        [RoomObjectKind.ExposedPowerBombTank] = new(InWorldCollectibleKind.PowerBombTank, CollectiblePresentation.Exposed),
        [RoomObjectKind.ExposedMorphBall] = new(InWorldCollectibleKind.MorphBall, CollectiblePresentation.Exposed),
        [RoomObjectKind.ChozoMissileTank] = new(InWorldCollectibleKind.MissileTank, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoSuperMissileTank] = new(InWorldCollectibleKind.SuperMissileTank, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoPowerBombTank] = new(InWorldCollectibleKind.PowerBombTank, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoBombs] = new(InWorldCollectibleKind.Bombs, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoChargeBeam] = new(InWorldCollectibleKind.ChargeBeam, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoIceBeam] = new(InWorldCollectibleKind.IceBeam, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoHiJumpBoots] = new(InWorldCollectibleKind.HiJumpBoots, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoSpeedBooster] = new(InWorldCollectibleKind.SpeedBooster, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoWaveBeam] = new(InWorldCollectibleKind.WaveBeam, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoSpazerBeam] = new(InWorldCollectibleKind.SpazerBeam, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoSpringBall] = new(InWorldCollectibleKind.SpringBall, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoVariaSuit] = new(InWorldCollectibleKind.VariaSuit, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoGravitySuit] = new(InWorldCollectibleKind.GravitySuit, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoXrayScope] = new(InWorldCollectibleKind.XrayScope, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoPlasmaBeam] = new(InWorldCollectibleKind.PlasmaBeam, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoGrappleBeam] = new(InWorldCollectibleKind.GrappleBeam, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoSpaceJump] = new(InWorldCollectibleKind.SpaceJump, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoScrewAttack] = new(InWorldCollectibleKind.ScrewAttack, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ChozoReserveTank] = new(InWorldCollectibleKind.ReserveTank, CollectiblePresentation.ChozoOrb),
        [RoomObjectKind.ShotBlockEnergyTank] = new(InWorldCollectibleKind.EnergyTank, CollectiblePresentation.ShotBlock),
        [RoomObjectKind.ShotBlockMissileTank] = new(InWorldCollectibleKind.MissileTank, CollectiblePresentation.ShotBlock),
        [RoomObjectKind.ShotBlockSuperMissileTank] = new(InWorldCollectibleKind.SuperMissileTank, CollectiblePresentation.ShotBlock),
    };
}

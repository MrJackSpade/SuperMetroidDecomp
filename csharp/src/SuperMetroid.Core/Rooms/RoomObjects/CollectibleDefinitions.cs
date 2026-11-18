using SuperMetroid.Core.Progression;
using Consequence = SuperMetroid.Core.Rooms.RoomObjects.CollectiblePickupConsequence;
using Grant = SuperMetroid.Core.Rooms.RoomObjects.CollectibleGrant;
using Kind = SuperMetroid.Core.Rooms.RoomObjects.InWorldCollectibleKind;
using Message = SuperMetroid.Core.Rooms.GameplayMessageId;
using Picture = SuperMetroid.Core.Rooms.RoomObjects.CollectiblePictureSource;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What each permanent item does once collected (#2481): the tanks add their energy or ammunition, the equipment and
/// suits are collected and equipped (a suit clearing the beam's charge), and each beam is collected and switched on, Spazer
/// switching Plasma off and Plasma Spazer. A beam's pickup shows the newly equipped beams' colours and a suit's starts its
/// transformation; every other pickup changes nothing more. The four tanks are pictured by blocks every room loads, every
/// other item by a picture of its own.
/// </summary>
internal static class CollectibleDefinitions
{
    /// <summary>Each permanent item's definition.</summary>
    internal static IReadOnlyDictionary<Kind, CollectibleDefinition> Items { get; } = new Dictionary<Kind, CollectibleDefinition>
    {
        [Kind.EnergyTank] = new(Grant.EnergyTank(SamusEnergyDefinitions.EnergyPerTank), Message.EnergyTank, Consequence.Nothing,
            Picture.SharedTankBlocks),
        [Kind.MissileTank] = new(Grant.MissileTank(CollectibleCapacityDefinitions.MissileTankMissiles), Message.MissileTank,
            Consequence.Nothing, Picture.SharedTankBlocks),
        [Kind.SuperMissileTank] = new(Grant.SuperMissileTank(CollectibleCapacityDefinitions.SuperMissileTankSuperMissiles),
            Message.SuperMissileTank, Consequence.Nothing, Picture.SharedTankBlocks),
        [Kind.PowerBombTank] = new(Grant.PowerBombTank(CollectibleCapacityDefinitions.PowerBombTankPowerBombs), Message.PowerBombTank,
            Consequence.Nothing, Picture.SharedTankBlocks),
        [Kind.ReserveTank] = new(Grant.ReserveTank(SamusEnergyDefinitions.EnergyPerTank), Message.ReserveTank, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.Bombs] = new(Grant.Equipment(SamusEquipmentFlags.Bombs), Message.Bombs, Consequence.Nothing, Picture.OwnPicture),
        [Kind.HiJumpBoots] = new(Grant.Equipment(SamusEquipmentFlags.HiJumpBoots), Message.HiJumpBoots, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.SpeedBooster] = new(Grant.Equipment(SamusEquipmentFlags.SpeedBooster), Message.SpeedBooster, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.SpringBall] = new(Grant.Equipment(SamusEquipmentFlags.SpringBall), Message.SpringBall, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.XrayScope] = new(Grant.Equipment(SamusEquipmentFlags.XrayScope), Message.XrayScope, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.GrappleBeam] = new(Grant.Equipment(SamusEquipmentFlags.GrappleBeam), Message.GrappleBeam, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.SpaceJump] = new(Grant.Equipment(SamusEquipmentFlags.SpaceJump), Message.SpaceJump, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.ScrewAttack] = new(Grant.Equipment(SamusEquipmentFlags.ScrewAttack), Message.ScrewAttack, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.MorphBall] = new(Grant.Equipment(SamusEquipmentFlags.MorphBall), Message.MorphBall, Consequence.Nothing,
            Picture.OwnPicture),
        [Kind.VariaSuit] = new(Grant.Suit(SamusEquipmentFlags.VariaSuit), Message.VariaSuit,
            Consequence.TransformsSuit(SamusSuitPickupKind.Varia), Picture.OwnPicture),
        [Kind.GravitySuit] = new(Grant.Suit(SamusEquipmentFlags.GravitySuit), Message.GravitySuit,
            Consequence.TransformsSuit(SamusSuitPickupKind.Gravity), Picture.OwnPicture),
        [Kind.ChargeBeam] = new(Grant.Beam(SamusBeamFlags.Charge, SamusBeamFlags.None), Message.ChargeBeam, Consequence.ShowsEquippedBeams,
            Picture.OwnPicture),
        [Kind.IceBeam] = new(Grant.Beam(SamusBeamFlags.Ice, SamusBeamFlags.None), Message.IceBeam, Consequence.ShowsEquippedBeams,
            Picture.OwnPicture),
        [Kind.WaveBeam] = new(Grant.Beam(SamusBeamFlags.Wave, SamusBeamFlags.None), Message.WaveBeam, Consequence.ShowsEquippedBeams,
            Picture.OwnPicture),
        [Kind.SpazerBeam] = new(Grant.Beam(SamusBeamFlags.Spazer, SamusBeamFlags.Plasma), Message.SpazerBeam,
            Consequence.ShowsEquippedBeams, Picture.OwnPicture),
        [Kind.PlasmaBeam] = new(Grant.Beam(SamusBeamFlags.Plasma, SamusBeamFlags.Spazer), Message.PlasmaBeam,
            Consequence.ShowsEquippedBeams, Picture.OwnPicture),
    };
}

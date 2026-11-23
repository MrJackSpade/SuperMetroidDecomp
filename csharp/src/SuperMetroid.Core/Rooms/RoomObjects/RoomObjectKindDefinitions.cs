using System.Collections.Frozen;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room-object kind each family's setup places, for the families whose setup carries what tells its kinds apart: a
/// placement names its object once, by its setup, and its kind follows from it (#2557).
/// </summary>
internal static partial class RoomObjectKindDefinitions
{
    /// <summary>The coloured-door kind each cap colour and facing places.</summary>
    internal static FrozenDictionary<ColoredDoorVariant, RoomObjectKind> ColoredDoors { get; } = new Dictionary<ColoredDoorVariant, RoomObjectKind>
    {
        [new ColoredDoorVariant(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left)] = RoomObjectKind.YellowDoorFacingLeft,
        [new ColoredDoorVariant(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right)] = RoomObjectKind.YellowDoorFacingRight,
        [new ColoredDoorVariant(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up)] = RoomObjectKind.YellowDoorFacingUp,
        [new ColoredDoorVariant(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down)] = RoomObjectKind.YellowDoorFacingDown,
        [new ColoredDoorVariant(ColoredDoorColor.Green, ColoredDoorOrientation.Left)] = RoomObjectKind.GreenDoorFacingLeft,
        [new ColoredDoorVariant(ColoredDoorColor.Green, ColoredDoorOrientation.Right)] = RoomObjectKind.GreenDoorFacingRight,
        [new ColoredDoorVariant(ColoredDoorColor.Green, ColoredDoorOrientation.Up)] = RoomObjectKind.GreenDoorFacingUp,
        [new ColoredDoorVariant(ColoredDoorColor.Green, ColoredDoorOrientation.Down)] = RoomObjectKind.GreenDoorFacingDown,
        [new ColoredDoorVariant(ColoredDoorColor.Red, ColoredDoorOrientation.Left)] = RoomObjectKind.RedDoorFacingLeft,
        [new ColoredDoorVariant(ColoredDoorColor.Red, ColoredDoorOrientation.Right)] = RoomObjectKind.RedDoorFacingRight,
    }.ToFrozenDictionary();

    /// <summary>The grey-door kind each grey cap's facing and closing places: Bomb Torizo's door is the one that waits for Bombs.</summary>
    internal static FrozenDictionary<GreyDoorVariant, RoomObjectKind> GreyDoors { get; } = new Dictionary<GreyDoorVariant, RoomObjectKind>
    {
        [new GreyDoorVariant(ColoredDoorOrientation.Right, GreyDoorClosing.AfterBombs)] = RoomObjectKind.BombTorizoGreyDoor,
        [new GreyDoorVariant(ColoredDoorOrientation.Right, GreyDoorClosing.AtOnce)] = RoomObjectKind.GreyDoorFacingRight,
        [new GreyDoorVariant(ColoredDoorOrientation.Left, GreyDoorClosing.AtOnce)] = RoomObjectKind.GreyDoorFacingLeft,
        [new GreyDoorVariant(ColoredDoorOrientation.Up, GreyDoorClosing.AtOnce)] = RoomObjectKind.GreyDoorFacingUp,
        [new GreyDoorVariant(ColoredDoorOrientation.Down, GreyDoorClosing.AtOnce)] = RoomObjectKind.GreyDoorFacingDown,
    }.ToFrozenDictionary();

    /// <summary>The item kind each permanent item and presentation places (#2354).</summary>
    internal static FrozenDictionary<PermanentCollectibleObject, RoomObjectKind> Items { get; } = new Dictionary<PermanentCollectibleObject, RoomObjectKind>
    {
        [new PermanentCollectibleObject(InWorldCollectibleKind.EnergyTank, CollectiblePresentation.Exposed)] = RoomObjectKind.ExposedEnergyTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.MissileTank, CollectiblePresentation.Exposed)] = RoomObjectKind.ExposedMissileTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SuperMissileTank, CollectiblePresentation.Exposed)] = RoomObjectKind.ExposedSuperMissileTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.PowerBombTank, CollectiblePresentation.Exposed)] = RoomObjectKind.ExposedPowerBombTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.MorphBall, CollectiblePresentation.Exposed)] = RoomObjectKind.ExposedMorphBall,
        [new PermanentCollectibleObject(InWorldCollectibleKind.MissileTank, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoMissileTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SuperMissileTank, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoSuperMissileTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.PowerBombTank, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoPowerBombTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.Bombs, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoBombs,
        [new PermanentCollectibleObject(InWorldCollectibleKind.ChargeBeam, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoChargeBeam,
        [new PermanentCollectibleObject(InWorldCollectibleKind.IceBeam, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoIceBeam,
        [new PermanentCollectibleObject(InWorldCollectibleKind.HiJumpBoots, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoHiJumpBoots,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SpeedBooster, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoSpeedBooster,
        [new PermanentCollectibleObject(InWorldCollectibleKind.WaveBeam, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoWaveBeam,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SpazerBeam, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoSpazerBeam,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SpringBall, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoSpringBall,
        [new PermanentCollectibleObject(InWorldCollectibleKind.VariaSuit, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoVariaSuit,
        [new PermanentCollectibleObject(InWorldCollectibleKind.GravitySuit, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoGravitySuit,
        [new PermanentCollectibleObject(InWorldCollectibleKind.XrayScope, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoXrayScope,
        [new PermanentCollectibleObject(InWorldCollectibleKind.PlasmaBeam, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoPlasmaBeam,
        [new PermanentCollectibleObject(InWorldCollectibleKind.GrappleBeam, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoGrappleBeam,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SpaceJump, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoSpaceJump,
        [new PermanentCollectibleObject(InWorldCollectibleKind.ScrewAttack, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoScrewAttack,
        [new PermanentCollectibleObject(InWorldCollectibleKind.ReserveTank, CollectiblePresentation.ChozoOrb)] = RoomObjectKind.ChozoReserveTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.EnergyTank, CollectiblePresentation.ShotBlock)] = RoomObjectKind.ShotBlockEnergyTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.MissileTank, CollectiblePresentation.ShotBlock)] = RoomObjectKind.ShotBlockMissileTank,
        [new PermanentCollectibleObject(InWorldCollectibleKind.SuperMissileTank, CollectiblePresentation.ShotBlock)] = RoomObjectKind.ShotBlockSuperMissileTank,
    }.ToFrozenDictionary();

    /// <summary>The eye kind each eye door's facing places.</summary>
    internal static FrozenDictionary<EyeDoorOrientation, RoomObjectKind> EyeDoorEyes { get; } = new Dictionary<EyeDoorOrientation, RoomObjectKind>
    {
        [EyeDoorOrientation.Right] = RoomObjectKind.EyeDoorEyeFacingRight,
        [EyeDoorOrientation.Left] = RoomObjectKind.EyeDoorEyeFacingLeft,
    }.ToFrozenDictionary();

    /// <summary>The body or bottom kind each eye-door part and facing places.</summary>
    internal static FrozenDictionary<EyeDoorPart, RoomObjectKind> EyeDoorParts { get; } = new Dictionary<EyeDoorPart, RoomObjectKind>
    {
        [new EyeDoorPart(EyeDoorBodyPart.Middle, EyeDoorOrientation.Right)] = RoomObjectKind.EyeDoorFacingRight,
        [new EyeDoorPart(EyeDoorBodyPart.Bottom, EyeDoorOrientation.Right)] = RoomObjectKind.EyeDoorBottomFacingRight,
        [new EyeDoorPart(EyeDoorBodyPart.Middle, EyeDoorOrientation.Left)] = RoomObjectKind.EyeDoorFacingLeft,
        [new EyeDoorPart(EyeDoorBodyPart.Bottom, EyeDoorOrientation.Left)] = RoomObjectKind.EyeDoorBottomFacingLeft,
    }.ToFrozenDictionary();

    /// <summary>The station kind each map, energy and missile access rule places.</summary>
    internal static FrozenDictionary<StationAccessRule, RoomObjectKind> AccessStations { get; } = new Dictionary<StationAccessRule, RoomObjectKind>
    {
        [StationAccessRuleDefinitions.Map] = RoomObjectKind.MapStation,
        [StationAccessRuleDefinitions.Energy] = RoomObjectKind.EnergyStation,
        [StationAccessRuleDefinitions.Missile] = RoomObjectKind.MissileStation,
    }.ToFrozenDictionary();

    /// <summary>The scroll-extension kind each extension write places.</summary>
    internal static FrozenDictionary<ScrollExtension, RoomObjectKind> ScrollExtensions { get; } = new Dictionary<ScrollExtension, RoomObjectKind>
    {
        [ScrollExtensionDefinitions.Rightwards] = RoomObjectKind.RightwardsScrollExtension,
        [ScrollExtensionDefinitions.Leftwards] = RoomObjectKind.LeftwardsScrollExtension,
        [ScrollExtensionDefinitions.Downwards] = RoomObjectKind.DownwardsScrollExtension,
        [ScrollExtensionDefinitions.Upwards] = RoomObjectKind.UpwardsScrollExtension,
    }.ToFrozenDictionary();

    /// <summary>The Draygon-cannon kind each facing and start places (#2400).</summary>
    internal static FrozenDictionary<DraygonCannonVariant, RoomObjectKind> DraygonCannons { get; } =
        new Dictionary<DraygonCannonVariant, RoomObjectKind>
        {
            [new DraygonCannonVariant(DraygonCannonOrientation.Right, DraygonCannonStart.Whole)] = RoomObjectKind.DraygonCannonFacingRight,
            [new DraygonCannonVariant(DraygonCannonOrientation.Right, DraygonCannonStart.Destroyed)] = RoomObjectKind.DraygonCannonFacingRightDestroyed,
            [new DraygonCannonVariant(DraygonCannonOrientation.Left, DraygonCannonStart.Whole)] = RoomObjectKind.DraygonCannonFacingLeft,
        }.ToFrozenDictionary();
}

using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// What a placement of each door, item, station and scroll-extension kind carries in its setup (#2481), as the population
/// generator decodes it: the generator reads these tables to write each placement's setup, and verification builds a
/// synthetic placement of a kind from them. Each kind the retail rooms place is listed by the one table of its family.
/// </summary>
internal static class RoomObjectKindVariants
{
    /// <summary>The colour and facing of the cap each coloured-door kind places.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ColoredDoorVariant> ColoredDoors { get; } = new Dictionary<RoomObjectKind, ColoredDoorVariant>
    {
        [RoomObjectKind.YellowDoorFacingLeft] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left),
        [RoomObjectKind.YellowDoorFacingRight] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right),
        [RoomObjectKind.YellowDoorFacingUp] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up),
        [RoomObjectKind.YellowDoorFacingDown] = new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down),
        [RoomObjectKind.GreenDoorFacingLeft] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Left),
        [RoomObjectKind.GreenDoorFacingRight] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Right),
        [RoomObjectKind.GreenDoorFacingUp] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Up),
        [RoomObjectKind.GreenDoorFacingDown] = new(ColoredDoorColor.Green, ColoredDoorOrientation.Down),
        [RoomObjectKind.RedDoorFacingLeft] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Left),
        [RoomObjectKind.RedDoorFacingRight] = new(ColoredDoorColor.Red, ColoredDoorOrientation.Right),
    };

    /// <summary>
    /// The grey cap each grey-door kind places: its facing, and whether it is Bomb Torizo's door, which faces right like the
    /// generic right-facing one and whose closing waits for Bombs.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectKind, GreyDoorVariant> GreyDoors { get; } = new Dictionary<RoomObjectKind, GreyDoorVariant>
    {
        [RoomObjectKind.BombTorizoGreyDoor] = new(ColoredDoorOrientation.Right, GreyDoorClosing.AfterBombs),
        [RoomObjectKind.GreyDoorFacingRight] = new(ColoredDoorOrientation.Right, GreyDoorClosing.AtOnce),
        [RoomObjectKind.GreyDoorFacingLeft] = new(ColoredDoorOrientation.Left, GreyDoorClosing.AtOnce),
        [RoomObjectKind.GreyDoorFacingUp] = new(ColoredDoorOrientation.Up, GreyDoorClosing.AtOnce),
        [RoomObjectKind.GreyDoorFacingDown] = new(ColoredDoorOrientation.Down, GreyDoorClosing.AtOnce),
    };

    /// <summary>
    /// The item and presentation each permanent-item kind places: one for each collectible some room places in each of the
    /// exposed, Chozo-orb and concealed shot-block presentations (#2354).
    /// </summary>
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

    /// <summary>The way the door of each eye-door eye kind faces.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, EyeDoorOrientation> EyeDoorEyes { get; } = new Dictionary<RoomObjectKind, EyeDoorOrientation>
    {
        [RoomObjectKind.EyeDoorEyeFacingRight] = EyeDoorOrientation.Right,
        [RoomObjectKind.EyeDoorEyeFacingLeft] = EyeDoorOrientation.Left,
    };

    /// <summary>The body part each eye-door body or bottom kind places, and the way its door faces.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, EyeDoorPart> EyeDoorParts { get; } = new Dictionary<RoomObjectKind, EyeDoorPart>
    {
        [RoomObjectKind.EyeDoorFacingRight] = new(EyeDoorBodyPart.Middle, EyeDoorOrientation.Right),
        [RoomObjectKind.EyeDoorBottomFacingRight] = new(EyeDoorBodyPart.Bottom, EyeDoorOrientation.Right),
        [RoomObjectKind.EyeDoorFacingLeft] = new(EyeDoorBodyPart.Middle, EyeDoorOrientation.Left),
        [RoomObjectKind.EyeDoorBottomFacingLeft] = new(EyeDoorBodyPart.Bottom, EyeDoorOrientation.Left),
    };

    /// <summary>The map, energy or missile station each such kind places; the save station's kind places its own station.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, StationKind> AccessStations { get; } = new Dictionary<RoomObjectKind, StationKind>
    {
        [RoomObjectKind.MapStation] = StationKind.Map,
        [RoomObjectKind.EnergyStation] = StationKind.Energy,
        [RoomObjectKind.MissileStation] = StationKind.Missile,
    };

    /// <summary>What each scroll-extension kind writes to extend the trigger beside it.</summary>
    internal static IReadOnlyDictionary<RoomObjectKind, ScrollExtension> ScrollExtensions { get; } = new Dictionary<RoomObjectKind, ScrollExtension>
    {
        [RoomObjectKind.RightwardsScrollExtension] = ScrollExtensionDefinitions.Rightwards,
        [RoomObjectKind.LeftwardsScrollExtension] = ScrollExtensionDefinitions.Leftwards,
        [RoomObjectKind.DownwardsScrollExtension] = ScrollExtensionDefinitions.Downwards,
        [RoomObjectKind.UpwardsScrollExtension] = ScrollExtensionDefinitions.Upwards,
    };
}

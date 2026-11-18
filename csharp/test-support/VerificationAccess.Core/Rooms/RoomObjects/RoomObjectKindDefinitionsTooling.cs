namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room-object kinds in the order of their retail header words, for development checks; never linked by player hosts.</summary>
internal static class RoomObjectKindDefinitionsTooling
{
    /// <summary>The room-object kinds retail room populations place, inspected by development dependency audits.</summary>
    internal static IEnumerable<RoomObjectKind> All => Enumerate();
    /// <summary>
    /// Yields the room-object kinds known to the extracted definition catalog, including
    /// doors, stations, bosses' room objects, and collectible variants.
    /// </summary>
    internal static IEnumerable<RoomObjectKind> Enumerate()
    {
        yield return RoomObjectKind.RightwardsScrollExtension;
        yield return RoomObjectKind.LeftwardsScrollExtension;
        yield return RoomObjectKind.DownwardsScrollExtension;
        yield return RoomObjectKind.UpwardsScrollExtension;
        yield return RoomObjectKind.MapStation;
        yield return RoomObjectKind.EnergyStation;
        yield return RoomObjectKind.MissileStation;
        yield return RoomObjectKind.ScrollTrigger;
        yield return RoomObjectKind.ElevatorPlatform;
        yield return RoomObjectKind.SaveStation;
        yield return RoomObjectKind.SpeedBoosterEscape;
        yield return RoomObjectKind.BombTorizoGreyDoor;
        yield return RoomObjectKind.WreckedShipAtticHeldPlace;
        yield return RoomObjectKind.DownwardGate;
        yield return RoomObjectKind.DownwardGateShotBlock;
        yield return RoomObjectKind.GreyDoorFacingLeft;
        yield return RoomObjectKind.GreyDoorFacingRight;
        yield return RoomObjectKind.GreyDoorFacingUp;
        yield return RoomObjectKind.GreyDoorFacingDown;
        yield return RoomObjectKind.YellowDoorFacingLeft;
        yield return RoomObjectKind.YellowDoorFacingRight;
        yield return RoomObjectKind.YellowDoorFacingUp;
        yield return RoomObjectKind.YellowDoorFacingDown;
        yield return RoomObjectKind.GreenDoorFacingLeft;
        yield return RoomObjectKind.GreenDoorFacingRight;
        yield return RoomObjectKind.GreenDoorFacingUp;
        yield return RoomObjectKind.GreenDoorFacingDown;
        yield return RoomObjectKind.RedDoorFacingLeft;
        yield return RoomObjectKind.RedDoorFacingRight;
        yield return RoomObjectKind.MotherBrainEscapeRoomGate;
        yield return RoomObjectKind.MotherBrainGlass;
        yield return RoomObjectKind.BombTorizoHand;
        yield return RoomObjectKind.NoobTube;
        yield return RoomObjectKind.MetroidRoomClearance;
        yield return RoomObjectKind.EyeDoorEyeFacingRight;
        yield return RoomObjectKind.EyeDoorFacingRight;
        yield return RoomObjectKind.EyeDoorBottomFacingRight;
        yield return RoomObjectKind.EyeDoorEyeFacingLeft;
        yield return RoomObjectKind.EyeDoorFacingLeft;
        yield return RoomObjectKind.EyeDoorBottomFacingLeft;
        yield return RoomObjectKind.DraygonCannonFacingRight;
        yield return RoomObjectKind.DraygonCannonFacingRightDestroyed;
        yield return RoomObjectKind.DraygonCannonFacingLeft;
        yield return RoomObjectKind.ExposedEnergyTank;
        yield return RoomObjectKind.ExposedMissileTank;
        yield return RoomObjectKind.ExposedSuperMissileTank;
        yield return RoomObjectKind.ExposedPowerBombTank;
        yield return RoomObjectKind.ExposedMorphBall;
        yield return RoomObjectKind.ChozoMissileTank;
        yield return RoomObjectKind.ChozoSuperMissileTank;
        yield return RoomObjectKind.ChozoPowerBombTank;
        yield return RoomObjectKind.ChozoBombs;
        yield return RoomObjectKind.ChozoChargeBeam;
        yield return RoomObjectKind.ChozoIceBeam;
        yield return RoomObjectKind.ChozoHiJumpBoots;
        yield return RoomObjectKind.ChozoSpeedBooster;
        yield return RoomObjectKind.ChozoWaveBeam;
        yield return RoomObjectKind.ChozoSpazerBeam;
        yield return RoomObjectKind.ChozoSpringBall;
        yield return RoomObjectKind.ChozoVariaSuit;
        yield return RoomObjectKind.ChozoGravitySuit;
        yield return RoomObjectKind.ChozoXrayScope;
        yield return RoomObjectKind.ChozoPlasmaBeam;
        yield return RoomObjectKind.ChozoGrappleBeam;
        yield return RoomObjectKind.ChozoSpaceJump;
        yield return RoomObjectKind.ChozoScrewAttack;
        yield return RoomObjectKind.ChozoReserveTank;
        yield return RoomObjectKind.ShotBlockEnergyTank;
        yield return RoomObjectKind.ShotBlockMissileTank;
        yield return RoomObjectKind.ShotBlockSuperMissileTank;
    }
}

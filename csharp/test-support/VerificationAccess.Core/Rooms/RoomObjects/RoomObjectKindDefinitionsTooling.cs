namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Development-tool members of <see cref="RoomObjectKindDefinitions"/>; never linked by player hosts.</summary>
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
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.RightwardsScrollExtension);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.LeftwardsScrollExtension);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.DownwardsScrollExtension);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.UpwardsScrollExtension);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.MapStation);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EnergyStation);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.MissileStation);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ScrollTrigger);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ElevatorPlatform);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.SaveStation);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.SpeedBoosterEscape);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.BombTorizoGreyDoor);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.WreckedShipAttic);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.DownwardGate);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.DownwardGateShotBlock);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreyDoorFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreyDoorFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreyDoorFacingUp);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreyDoorFacingDown);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.YellowDoorFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.YellowDoorFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.YellowDoorFacingUp);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.YellowDoorFacingDown);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreenDoorFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreenDoorFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreenDoorFacingUp);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.GreenDoorFacingDown);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.RedDoorFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.RedDoorFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.MotherBrainEscapeRoomGate);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.MotherBrainGlass);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.BombTorizoHand);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.NoobTube);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.MetroidRoomClearance);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EyeDoorEyeFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EyeDoorFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EyeDoorBottomFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EyeDoorEyeFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EyeDoorFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.EyeDoorBottomFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.DraygonCannonFacingRight);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.DraygonCannonFacingRightDestroyed);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.DraygonCannonFacingLeft);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ExposedEnergyTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ExposedMissileTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ExposedSuperMissileTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ExposedPowerBombTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ExposedMorphBall);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoMissileTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoSuperMissileTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoPowerBombTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoBombs);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoChargeBeam);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoIceBeam);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoHiJumpBoots);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoSpeedBooster);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoWaveBeam);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoSpazerBeam);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoSpringBall);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoVariaSuit);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoGravitySuit);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoXrayScope);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoPlasmaBeam);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoGrappleBeam);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoSpaceJump);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoScrewAttack);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ChozoReserveTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ShotBlockEnergyTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ShotBlockMissileTank);
        yield return RoomObjectKinds.RequirePlaced(RoomObjectKind.ShotBlockSuperMissileTank);
    }
}

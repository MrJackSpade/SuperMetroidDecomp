namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="RoomPlmHeaderDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomPlmHeaderDefinitionsTooling
{
    internal static IEnumerable<RoomPlmHeaderDefinition> All => Enumerate();
    internal static IEnumerable<RoomPlmHeaderDefinition> Enumerate()
    {
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.RightwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.LeftwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.DownwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.UpwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.MapStation);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EnergyStation);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.MissileStation);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ScrollTrigger);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ElevatorPlatform);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.SaveStation);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.SpeedBoosterEscape);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.BombTorizoGreyDoor);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.WreckedShipAttic);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.DownwardGate);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.DownwardGateShotBlock);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreyDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreyDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreyDoorFacingUp);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreyDoorFacingDown);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.YellowDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.YellowDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.YellowDoorFacingUp);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.YellowDoorFacingDown);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreenDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreenDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreenDoorFacingUp);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.GreenDoorFacingDown);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.RedDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.RedDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.MotherBrainEscapeRoomGate);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.MotherBrainGlass);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.BombTorizoHand);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.NoobTube);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.SetMetroidsClearedStatesWhenRequired);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EyeDoorEyeFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EyeDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EyeDoorBottomFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EyeDoorEyeFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EyeDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.EyeDoorBottomFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.DraygonCannonFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.DraygonCannonFacingRightDestroyed);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.DraygonCannonFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ExposedEnergyTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ExposedMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ExposedSuperMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ExposedPowerBombTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ExposedMorphBall);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoSuperMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoPowerBombTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoBombs);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoChargeBeam);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoIceBeam);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoHiJumpBoots);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoSpeedBooster);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoWaveBeam);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoSpazerBeam);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoSpringBall);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoVariaSuit);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoGravitySuit);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoXrayScope);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoPlasmaBeam);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoGrappleBeam);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoSpaceJump);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoScrewAttack);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ChozoReserveTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ShotBlockEnergyTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ShotBlockMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ShotBlockSuperMissileTank);
    }
}

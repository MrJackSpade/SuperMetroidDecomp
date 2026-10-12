namespace SuperMetroid.Core.Rooms;

/// <summary>Development-tool members of <see cref="RoomPlmHeaderDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomPlmHeaderDefinitionsTooling
{
    internal static IEnumerable<RoomPlmHeaderDefinition> All => Enumerate();
    internal static IEnumerable<RoomPlmHeaderDefinition> Enumerate()
    {
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.RightwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.LeftwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.DownwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.UpwardsScrollExtension);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.MapStation);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EnergyStation);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.MissileStation);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ScrollTrigger);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ElevatorPlatform);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.SaveStation);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.SpeedBoosterEscape);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.BombTorizoGreyDoor);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.WreckedShipAttic);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.DownwardGate);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.DownwardGateShotBlock);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreyDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreyDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreyDoorFacingUp);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreyDoorFacingDown);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.YellowDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.YellowDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.YellowDoorFacingUp);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.YellowDoorFacingDown);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreenDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreenDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreenDoorFacingUp);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.GreenDoorFacingDown);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.RedDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.RedDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.MotherBrainEscapeRoomGate);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.MotherBrainGlass);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.BombTorizoHand);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.NoobTube);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.SetMetroidsClearedStatesWhenRequired);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EyeDoorEyeFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EyeDoorFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EyeDoorBottomFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EyeDoorEyeFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EyeDoorFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.EyeDoorBottomFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.DraygonCannonFacingRight);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.DraygonCannonFacingRightDestroyed);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.DraygonCannonFacingLeft);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ExposedEnergyTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ExposedMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ExposedSuperMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ExposedPowerBombTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ExposedMorphBall);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoSuperMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoPowerBombTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoBombs);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoChargeBeam);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoIceBeam);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoHiJumpBoots);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoSpeedBooster);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoWaveBeam);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoSpazerBeam);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoSpringBall);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoVariaSuit);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoGravitySuit);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoXrayScope);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoPlasmaBeam);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoGrappleBeam);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoSpaceJump);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoScrewAttack);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ChozoReserveTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ShotBlockEnergyTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ShotBlockMissileTank);
        yield return RoomPlmHeaderDefinitions.Get(PlmHeaderId.ShotBlockSuperMissileTank);
    }
}

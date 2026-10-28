namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Development-tool members of <see cref="RoomPlmHeaderDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomPlmHeaderDefinitionsTooling
{
    /// <summary>The supported room PLM headers inspected by development dependency audits.</summary>
    internal static IEnumerable<RoomPlmHeaderDefinition> All => Enumerate();
    /// <summary>
    /// Yields the room PLM headers known to the extracted definition catalog, including
    /// doors, stations, bosses' room objects, and collectible variants.
    /// </summary>
    internal static IEnumerable<RoomPlmHeaderDefinition> Enumerate()
    {
        yield return RoomPlmHeaders.Get(PlmHeaderId.RightwardsScrollExtension);
        yield return RoomPlmHeaders.Get(PlmHeaderId.LeftwardsScrollExtension);
        yield return RoomPlmHeaders.Get(PlmHeaderId.DownwardsScrollExtension);
        yield return RoomPlmHeaders.Get(PlmHeaderId.UpwardsScrollExtension);
        yield return RoomPlmHeaders.Get(PlmHeaderId.MapStation);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EnergyStation);
        yield return RoomPlmHeaders.Get(PlmHeaderId.MissileStation);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ScrollTrigger);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ElevatorPlatform);
        yield return RoomPlmHeaders.Get(PlmHeaderId.SaveStation);
        yield return RoomPlmHeaders.Get(PlmHeaderId.SpeedBoosterEscape);
        yield return RoomPlmHeaders.Get(PlmHeaderId.BombTorizoGreyDoor);
        yield return RoomPlmHeaders.Get(PlmHeaderId.WreckedShipAttic);
        yield return RoomPlmHeaders.Get(PlmHeaderId.DownwardGate);
        yield return RoomPlmHeaders.Get(PlmHeaderId.DownwardGateShotBlock);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreyDoorFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreyDoorFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreyDoorFacingUp);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreyDoorFacingDown);
        yield return RoomPlmHeaders.Get(PlmHeaderId.YellowDoorFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.YellowDoorFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.YellowDoorFacingUp);
        yield return RoomPlmHeaders.Get(PlmHeaderId.YellowDoorFacingDown);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreenDoorFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreenDoorFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreenDoorFacingUp);
        yield return RoomPlmHeaders.Get(PlmHeaderId.GreenDoorFacingDown);
        yield return RoomPlmHeaders.Get(PlmHeaderId.RedDoorFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.RedDoorFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.MotherBrainEscapeRoomGate);
        yield return RoomPlmHeaders.Get(PlmHeaderId.MotherBrainGlass);
        yield return RoomPlmHeaders.Get(PlmHeaderId.BombTorizoHand);
        yield return RoomPlmHeaders.Get(PlmHeaderId.NoobTube);
        yield return RoomPlmHeaders.Get(PlmHeaderId.SetMetroidsClearedStatesWhenRequired);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EyeDoorEyeFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EyeDoorFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EyeDoorBottomFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EyeDoorEyeFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EyeDoorFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.EyeDoorBottomFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.DraygonCannonFacingRight);
        yield return RoomPlmHeaders.Get(PlmHeaderId.DraygonCannonFacingRightDestroyed);
        yield return RoomPlmHeaders.Get(PlmHeaderId.DraygonCannonFacingLeft);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ExposedEnergyTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ExposedMissileTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ExposedSuperMissileTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ExposedPowerBombTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ExposedMorphBall);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoMissileTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoSuperMissileTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoPowerBombTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoBombs);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoChargeBeam);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoIceBeam);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoHiJumpBoots);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoSpeedBooster);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoWaveBeam);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoSpazerBeam);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoSpringBall);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoVariaSuit);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoGravitySuit);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoXrayScope);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoPlasmaBeam);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoGrappleBeam);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoSpaceJump);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoScrewAttack);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ChozoReserveTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ShotBlockEnergyTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ShotBlockMissileTank);
        yield return RoomPlmHeaders.Get(PlmHeaderId.ShotBlockSuperMissileTank);
    }
}

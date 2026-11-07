namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Development-tool members of <see cref="RoomObjectHeaderDefinitions"/>; never linked by player hosts.</summary>
internal static class RoomObjectHeaderDefinitionsTooling
{
    /// <summary>The supported room PLM headers inspected by development dependency audits.</summary>
    internal static IEnumerable<RoomObjectHeaderDefinition> All => Enumerate();
    /// <summary>
    /// Yields the room PLM headers known to the extracted definition catalog, including
    /// doors, stations, bosses' room objects, and collectible variants.
    /// </summary>
    internal static IEnumerable<RoomObjectHeaderDefinition> Enumerate()
    {
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.RightwardsScrollExtension);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.LeftwardsScrollExtension);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.DownwardsScrollExtension);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.UpwardsScrollExtension);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.MapStation);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EnergyStation);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.MissileStation);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ScrollTrigger);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ElevatorPlatform);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.SaveStation);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.SpeedBoosterEscape);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.BombTorizoGreyDoor);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.WreckedShipAttic);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.DownwardGate);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.DownwardGateShotBlock);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreyDoorFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreyDoorFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreyDoorFacingUp);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreyDoorFacingDown);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.YellowDoorFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.YellowDoorFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.YellowDoorFacingUp);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.YellowDoorFacingDown);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreenDoorFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreenDoorFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreenDoorFacingUp);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.GreenDoorFacingDown);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.RedDoorFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.RedDoorFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.MotherBrainEscapeRoomGate);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.MotherBrainGlass);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.BombTorizoHand);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.NoobTube);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.SetMetroidsClearedStatesWhenRequired);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EyeDoorEyeFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EyeDoorFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EyeDoorBottomFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EyeDoorEyeFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EyeDoorFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.EyeDoorBottomFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.DraygonCannonFacingRight);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.DraygonCannonFacingRightDestroyed);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.DraygonCannonFacingLeft);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ExposedEnergyTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ExposedMissileTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ExposedSuperMissileTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ExposedPowerBombTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ExposedMorphBall);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoMissileTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoSuperMissileTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoPowerBombTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoBombs);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoChargeBeam);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoIceBeam);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoHiJumpBoots);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoSpeedBooster);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoWaveBeam);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoSpazerBeam);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoSpringBall);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoVariaSuit);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoGravitySuit);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoXrayScope);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoPlasmaBeam);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoGrappleBeam);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoSpaceJump);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoScrewAttack);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ChozoReserveTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ShotBlockEnergyTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ShotBlockMissileTank);
        yield return RoomObjectHeaders.Get(RoomObjectHeaderId.ShotBlockSuperMissileTank);
    }
}

namespace SuperMetroid.Core.Rooms;

/// <summary>Named retail header dispatch; regenerate with tools/generate-room-plm-population-definitions.ps1 -HeadersOnly.</summary>
internal static partial class RoomPlmHeaderDefinitions
{
    private static bool TrySelect(PlmHeaderId header, out RoomPlmHeaderDefinition value)
    {
        value = header switch
        {
            PlmHeaderId.RightwardsScrollExtension => new(header, 0xB33A, 0xAFA4),
            PlmHeaderId.LeftwardsScrollExtension => new(header, 0xB345, 0xAF9E),
            PlmHeaderId.DownwardsScrollExtension => new(header, 0xB350, 0xAFB0),
            PlmHeaderId.UpwardsScrollExtension => new(header, 0xB35B, 0xAFAA),
            PlmHeaderId.MapStation => new(header, 0xB18B, 0xAD62),
            PlmHeaderId.EnergyStation => new(header, 0xB21D, 0xADC2),
            PlmHeaderId.MissileStation => new(header, 0xB245, 0xAE4C),
            PlmHeaderId.ScrollTrigger => new(header, 0xB371, 0xAF86),
            PlmHeaderId.ElevatorPlatform => new(header, 0xB3C1, 0xAFB6),
            PlmHeaderId.SaveStation => new(header, 0xB5EE, 0xAFE8),
            PlmHeaderId.SpeedBoosterEscape => new(header, 0xB89C, 0xB88A),
            PlmHeaderId.BombTorizoGreyDoor => new(header, 0xC794, 0xBA7F),
            PlmHeaderId.WreckedShipAttic => new(header, 0xBAFA, 0xBAFF),
            PlmHeaderId.DownwardGate => new(header, 0xC6BE, 0xBC3A),
            PlmHeaderId.DownwardGateShotBlock => new(header, 0xC6E0, 0xBCAF),
            PlmHeaderId.GreyDoorFacingLeft => new(header, 0xC794, 0xBE70),
            PlmHeaderId.GreyDoorFacingRight => new(header, 0xC794, 0xBED9),
            PlmHeaderId.GreyDoorFacingUp => new(header, 0xC794, 0xBF42),
            PlmHeaderId.GreyDoorFacingDown => new(header, 0xC794, 0xBFAB),
            PlmHeaderId.YellowDoorFacingLeft => new(header, 0xC7B1, 0xC014),
            PlmHeaderId.YellowDoorFacingRight => new(header, 0xC7B1, 0xC077),
            PlmHeaderId.YellowDoorFacingUp => new(header, 0xC7B1, 0xC0DA),
            PlmHeaderId.YellowDoorFacingDown => new(header, 0xC7B1, 0xC139),
            PlmHeaderId.GreenDoorFacingLeft => new(header, 0xC7B1, 0xC19C),
            PlmHeaderId.GreenDoorFacingRight => new(header, 0xC7B1, 0xC1FB),
            PlmHeaderId.GreenDoorFacingUp => new(header, 0xC7B1, 0xC25A),
            PlmHeaderId.GreenDoorFacingDown => new(header, 0xC7B1, 0xC2B9),
            PlmHeaderId.RedDoorFacingLeft => new(header, 0xC7B1, 0xC318),
            PlmHeaderId.RedDoorFacingRight => new(header, 0xC7B1, 0xC37A),
            PlmHeaderId.MotherBrainEscapeRoomGate => new(header, 0xB3C1, 0xBB34),
            PlmHeaderId.MotherBrainGlass => new(header, 0xD5F6, 0xD202),
            PlmHeaderId.BombTorizoHand => new(header, 0xD606, 0xD368),
            PlmHeaderId.NoobTube => new(header, 0xD6CC, 0xD4D4),
            PlmHeaderId.SetMetroidsClearedStatesWhenRequired => new(header, 0xDB1E, 0xDB42),
            PlmHeaderId.EyeDoorEyeFacingRight => new(header, 0xDA8C, 0xD955),
            PlmHeaderId.EyeDoorFacingRight => new(header, 0xDAB9, 0xDA20),
            PlmHeaderId.EyeDoorBottomFacingRight => new(header, 0xDAB9, 0xDA56),
            PlmHeaderId.EyeDoorEyeFacingLeft => new(header, 0xDA8C, 0xD81E),
            PlmHeaderId.EyeDoorFacingLeft => new(header, 0xDAB9, 0xD8E9),
            PlmHeaderId.EyeDoorBottomFacingLeft => new(header, 0xDAB9, 0xD91F),
            PlmHeaderId.DraygonCannonFacingRight => new(header, 0xDE94, 0xDCDE),
            PlmHeaderId.DraygonCannonFacingRightDestroyed => new(header, 0xDF4C, 0xDD11),
            PlmHeaderId.DraygonCannonFacingLeft => new(header, 0xDEF0, 0xDDB9),
            PlmHeaderId.ExposedEnergyTank => new(header, 0xEE4D, 0xE099),
            PlmHeaderId.ExposedMissileTank => new(header, 0xEE52, 0xE0BE),
            PlmHeaderId.ExposedSuperMissileTank => new(header, 0xEE57, 0xE0E3),
            PlmHeaderId.ExposedPowerBombTank => new(header, 0xEE5C, 0xE108),
            PlmHeaderId.ExposedMorphBall => new(header, 0xEE64, 0xE3EF),
            PlmHeaderId.ChozoMissileTank => new(header, 0xEE52, 0xE47C),
            PlmHeaderId.ChozoSuperMissileTank => new(header, 0xEE57, 0xE4AE),
            PlmHeaderId.ChozoPowerBombTank => new(header, 0xEE5C, 0xE4E0),
            PlmHeaderId.ChozoBombs => new(header, 0xEE64, 0xE512),
            PlmHeaderId.ChozoChargeBeam => new(header, 0xEE64, 0xE54D),
            PlmHeaderId.ChozoIceBeam => new(header, 0xEE64, 0xE588),
            PlmHeaderId.ChozoHiJumpBoots => new(header, 0xEE64, 0xE5C3),
            PlmHeaderId.ChozoSpeedBooster => new(header, 0xEE64, 0xE5FE),
            PlmHeaderId.ChozoWaveBeam => new(header, 0xEE64, 0xE642),
            PlmHeaderId.ChozoSpazerBeam => new(header, 0xEE64, 0xE67D),
            PlmHeaderId.ChozoSpringBall => new(header, 0xEE64, 0xE6B8),
            PlmHeaderId.ChozoVariaSuit => new(header, 0xEE64, 0xE6F3),
            PlmHeaderId.ChozoGravitySuit => new(header, 0xEE64, 0xE735),
            PlmHeaderId.ChozoXrayScope => new(header, 0xEE64, 0xE777),
            PlmHeaderId.ChozoPlasmaBeam => new(header, 0xEE64, 0xE7B1),
            PlmHeaderId.ChozoGrappleBeam => new(header, 0xEE64, 0xE7EC),
            PlmHeaderId.ChozoSpaceJump => new(header, 0xEE64, 0xE826),
            PlmHeaderId.ChozoScrewAttack => new(header, 0xEE64, 0xE861),
            PlmHeaderId.ChozoReserveTank => new(header, 0xEE64, 0xE8D7),
            PlmHeaderId.ShotBlockEnergyTank => new(header, 0xEE77, 0xE911),
            PlmHeaderId.ShotBlockMissileTank => new(header, 0xEE7C, 0xE949),
            PlmHeaderId.ShotBlockSuperMissileTank => new(header, 0xEE81, 0xE981),
            PlmHeaderId.None or PlmHeaderId.Nothing or PlmHeaderId.CollisionReactionClearCarry or PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
            PlmHeaderId.WreckedShipEntranceTreadmillFromEast or PlmHeaderId.InsideReactionNothingB653 or PlmHeaderId.InsideReactionNothingB657 or PlmHeaderId.InsideReactionNothingB65B or
            PlmHeaderId.FillMotherBrainsWall or PlmHeaderId.MotherBrainsRoomEscapeDoor or PlmHeaderId.MotherBrainsBackgroundRow2 or PlmHeaderId.MotherBrainsBackgroundRow3 or
            PlmHeaderId.MotherBrainsBackgroundRow4 or PlmHeaderId.MotherBrainsBackgroundRow5 or PlmHeaderId.MotherBrainsBackgroundRow6 or PlmHeaderId.MotherBrainsBackgroundRow7 or
            PlmHeaderId.MotherBrainsBackgroundRow8 or PlmHeaderId.MotherBrainsBackgroundRow9 or PlmHeaderId.MotherBrainsBackgroundRowA or PlmHeaderId.MotherBrainsBackgroundRowB or
            PlmHeaderId.MotherBrainsBackgroundRowC or PlmHeaderId.MotherBrainsBackgroundRowD or PlmHeaderId.ClearMotherBrainCeilingBlock or PlmHeaderId.ClearMotherBrainCeilingTube or
            PlmHeaderId.ClearMotherBrainBottomMiddleSideTube or PlmHeaderId.ClearMotherBrainBottomMiddleTubes or PlmHeaderId.ClearMotherBrainBottomLeftTube or PlmHeaderId.ClearMotherBrainBottomRightTube or
            PlmHeaderId.InsideReactionBrinstarFloorPlant or PlmHeaderId.InsideReactionBrinstarCeilingPlant or PlmHeaderId.MapStationRightAccess or PlmHeaderId.MapStationLeftAccess or
            PlmHeaderId.EnergyStationRightAccess or PlmHeaderId.EnergyStationLeftAccess or PlmHeaderId.MissileStationRightAccess or PlmHeaderId.MissileStationLeftAccess or
            PlmHeaderId.Nothing84B6F7 or PlmHeaderId.Nothing84B6FB or PlmHeaderId.ScrollTriggerCollision or PlmHeaderId.UnusedSolidScrollPLM or
            PlmHeaderId.InsideReactionCrateria80 or PlmHeaderId.InsideReactionQuicksandSurface or PlmHeaderId.InsideReactionSubmergingQuicksand or PlmHeaderId.InsideReactionSandFallsSlow or
            PlmHeaderId.InsideReactionSandFallsFast or PlmHeaderId.CollisionReactionQuicksandSurface or PlmHeaderId.CollisionReactionSubmergingQuicksand or PlmHeaderId.CollisionReactionSandFallsSlow or
            PlmHeaderId.CollisionReactionSandFallsFast or PlmHeaderId.ClearCrocomireBridge or PlmHeaderId.CrumbleCrocomireBridgeBlock or PlmHeaderId.ClearCrocomireBridgeBlock or
            PlmHeaderId.ClearCrocomireInvisibleWall or PlmHeaderId.CreateCrocomireInvisibleWall or PlmHeaderId.UnusedDraw13BlankAirTiles or PlmHeaderId.UnusedDraw13BlankSolidTiles or
            PlmHeaderId.ClearBabyMetroidInvisibleWall or PlmHeaderId.CreateBabyMetroidInvisibleWall or PlmHeaderId.SaveStationTrigger or PlmHeaderId.CrumbleAccessToTourianElevator or
            PlmHeaderId.ClearAccessToTourianElevator or PlmHeaderId.DrawPhantoonDoorDuringBossFight or PlmHeaderId.RestorePhantoonDoorAfterBossFight or PlmHeaderId.CrumbleSporeSpawnCeiling or
            PlmHeaderId.ClearSporeSpawnCeiling or PlmHeaderId.ClearBotwoonWall or PlmHeaderId.CrumbleBotwoonWall or PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
            PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or PlmHeaderId.CrumbleKraidPlatformVariant1 or PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or PlmHeaderId.CrumbleKraidPlatformVariant2 or
            PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or PlmHeaderId.ClearKraidCeiling or PlmHeaderId.ClearKraidSpikes or PlmHeaderId.CrumbleKraidSpikes or
            PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or PlmHeaderId.ShaktoolsRoom or PlmHeaderId.MaridiaElevatube or PlmHeaderId.OldTourianEscapeShaftFakeWall or
            PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or PlmHeaderId.GateBlock or PlmHeaderId.ReactionCrittersEscapeBlock or PlmHeaderId.CrittersEscapeBlock or
            PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or PlmHeaderId.CrateriaMainstreetEscapePassage or PlmHeaderId.LeftGreenGateTrigger or PlmHeaderId.RightGreenGateTrigger or
            PlmHeaderId.LeftRedGateTrigger or PlmHeaderId.RightRedGateTrigger or PlmHeaderId.LeftBlueGateTrigger or PlmHeaderId.RightBlueGateTrigger or
            PlmHeaderId.LeftYellowGateTrigger or PlmHeaderId.RightYellowGateTrigger or PlmHeaderId.DownwardsOpenGate or PlmHeaderId.UpwardsOpenGate or
            PlmHeaderId.UpwardsClosedGate or PlmHeaderId.UpwardsGateShotblock or PlmHeaderId.GenericShotTriggerForAPLM or PlmHeaderId.RedDoorFacingUp or
            PlmHeaderId.RedDoorFacingDown or PlmHeaderId.BlueDoorFacingLeft or PlmHeaderId.BlueDoorFacingRight or PlmHeaderId.BlueDoorFacingUp or
            PlmHeaderId.BlueDoorFacingDown or PlmHeaderId.BlueDoorClosingFacingLeft or PlmHeaderId.BlueDoorClosingFacingRight or PlmHeaderId.BlueDoorClosingFacingUp or
            PlmHeaderId.BlueDoorClosingFacingDown or PlmHeaderId.MotherBrainEscapeRoomGateClosing or PlmHeaderId.Plm1x1RespawningCrumbleBlock or PlmHeaderId.Plm2x1RespawningCrumbleBlock or
            PlmHeaderId.Plm1x2RespawningCrumbleBlock or PlmHeaderId.Plm2x2RespawningCrumbleBlock or PlmHeaderId.BombReactionSpeedBoostBlock or PlmHeaderId.SpeedBlockBrinstarSlowRespawning or
            PlmHeaderId.SpeedBlockBrinstarSlowPermanent or PlmHeaderId.SpeedBlockRespawning or PlmHeaderId.SpeedBlockDachoraRespawning or PlmHeaderId.SpeedBlockPermanent or
            PlmHeaderId.ContactCrumble1x1Respawning or PlmHeaderId.ContactCrumble2x1Respawning or PlmHeaderId.ContactCrumble1x2Respawning or PlmHeaderId.ContactCrumble2x2Respawning or
            PlmHeaderId.ContactCrumble1x1Permanent or PlmHeaderId.ContactCrumble2x1Permanent or PlmHeaderId.ContactCrumble1x2Permanent or PlmHeaderId.ContactCrumble2x2Permanent or
            PlmHeaderId.Reaction1x1RespawningShotBlock or PlmHeaderId.Reaction2x1RespawningShotBlock or PlmHeaderId.Reaction1x2RespawningShotBlock or PlmHeaderId.Reaction2x2RespawningShotBlock or
            PlmHeaderId.Reaction1x1ShotBlock or PlmHeaderId.Reaction2x1ShotBlock or PlmHeaderId.Reaction1x2ShotBlock or PlmHeaderId.Reaction2x2ShotBlock or
            PlmHeaderId.ReactionRespawningPowerBombBlock or PlmHeaderId.ReactionPowerBombBlock or PlmHeaderId.ReactionRespawningSuperMissileBlock or PlmHeaderId.ReactionSuperMissileBlock or
            PlmHeaderId.EnemyBreakableBlock or PlmHeaderId.Collision1x1RespawningBombBlock or PlmHeaderId.Collision2x1RespawningBombBlock or PlmHeaderId.Collision1x2RespawningBombBlock or
            PlmHeaderId.Collision2x2RespawningBombBlock or PlmHeaderId.Collision1x1BombBlock or PlmHeaderId.Collision2x1BombBlock or PlmHeaderId.Collision1x2BombBlock or
            PlmHeaderId.Collision2x2BombBlock or PlmHeaderId.Reaction1x1RespawningBombBlock or PlmHeaderId.Reaction2x1RespawningBombBlock or PlmHeaderId.Reaction1x2RespawningBombBlock or
            PlmHeaderId.Reaction2x2RespawningBombBlock or PlmHeaderId.Reaction1x1BombBlock or PlmHeaderId.Reaction2x1BombBlock or PlmHeaderId.Reaction1x2BombBlock or
            PlmHeaderId.Reaction2x2BombBlock or PlmHeaderId.GrappledGrappleBlock or PlmHeaderId.GrappledRespawningBreakableGrappleBlock or PlmHeaderId.GrappledBreakableGrappleBlock or
            PlmHeaderId.GrappledGenericSpikeBlock or PlmHeaderId.GrappledDraygonsBrokenTurret or PlmHeaderId.UnusedBlueBrinstarFaceBlock or PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug or
            PlmHeaderId.UnusedShotBlock or PlmHeaderId.UnusedGrappleBlock or PlmHeaderId.LowerNorfairChozoHand or PlmHeaderId.CollisionLowerNorfairChozoHandCheck or
            PlmHeaderId.WreckedShipChozoHand or PlmHeaderId.CollisionWreckedShipChozoHandCheck or PlmHeaderId.ClearSlopeAccessForWreckedShipChozo or PlmHeaderId.BlockSlopeAccessForWreckedShipChozo or
            PlmHeaderId.DraygonCannonFacingLeftUnshielded or PlmHeaderId.ItemCollisionDetection or PlmHeaderId.ExposedBombs or PlmHeaderId.ExposedChargeBeam or
            PlmHeaderId.ExposedIceBeam or PlmHeaderId.ExposedHiJumpBoots or PlmHeaderId.ExposedSpeedBooster or PlmHeaderId.ExposedWaveBeam or
            PlmHeaderId.ExposedSpazerBeam or PlmHeaderId.ExposedSpringBall or PlmHeaderId.ExposedVariaSuit or PlmHeaderId.ExposedGravitySuit or
            PlmHeaderId.ExposedXrayScope or PlmHeaderId.ExposedPlasmaBeam or PlmHeaderId.ExposedGrappleBeam or PlmHeaderId.ExposedSpaceJump or
            PlmHeaderId.ExposedScrewAttack or PlmHeaderId.ExposedReserveTank or PlmHeaderId.ChozoEnergyTank or PlmHeaderId.ChozoMorphBall or
            PlmHeaderId.ShotBlockPowerBombTank or PlmHeaderId.ShotBlockBombs or PlmHeaderId.ShotBlockChargeBeam or PlmHeaderId.ShotBlockIceBeam or
            PlmHeaderId.ShotBlockHiJumpBoots or PlmHeaderId.ShotBlockSpeedBooster or PlmHeaderId.ShotBlockWaveBeam or PlmHeaderId.ShotBlockSpazerBeam or
            PlmHeaderId.ShotBlockSpringBall or PlmHeaderId.ShotBlockVariaSuit or PlmHeaderId.ShotBlockGravitySuit or PlmHeaderId.ShotBlockXrayScope or
            PlmHeaderId.ShotBlockPlasmaBeam or PlmHeaderId.ShotBlockGrappleBeam or PlmHeaderId.ShotBlockSpaceJump or PlmHeaderId.ShotBlockScrewAttack or
            PlmHeaderId.ShotBlockMorphBall or PlmHeaderId.ShotBlockReserveTank => default,
            _ => throw new InvalidOperationException($"Undefined PlmHeaderId {header}."),
        };
        return value.Header != PlmHeaderId.None;
    }
}

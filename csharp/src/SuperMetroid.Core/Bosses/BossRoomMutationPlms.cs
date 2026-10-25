using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses;

/// <summary>Boss-room terrain PLMs: Botwoon's wall, the Spore Spawn ceiling, and the Kraid, Crocomire, baby Metroid wall and Mother Brain room mutations.</summary>
internal sealed class BossRoomMutationPlms
{
    /// <summary>The shared native PLM allocation table used by each boss-room hardcoded spawn.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Uses the room's PLM table so boss mutations contend with ordinary room PLMs for the same forty slots.</summary>
    internal BossRoomMutationPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>
    /// Spawns Botwoon's hardcoded wall PLM at room block (15,4), preserving the header's
    /// setup routine and the retail descending 40-slot allocation order.
    /// </summary>
    /// <remarks>
    /// Header <c>$B797</c> has an RTS setup and clears an already-defeated room on the next
    /// handler pass. Header <c>$B79B</c> runs setup <c>$84:AB28</c>, which delays its crumble
    /// list for exactly 64 PLM frames. The live list itself moves one block downward after
    /// each four-frame, four-image row and repeats nine times using PLM_Timers.
    /// </remarks>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnBotwoonWall(RoomLevelData level, PlmHeaderId header)
    {
        Ensure.NotNull(level);
        if (header is not (
            PlmHeaderId.ClearBotwoonWall or
            PlmHeaderId.CrumbleBotwoonWall))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Botwoon wall header must be $B797 (clear) or $B79B (crumble).");
        }

        // Both `$B3:9590` and `$B3:9ADD` pass the literal hardcoded coordinates (15,4).
        // Validate through RoomLevelData rather than allowing a malformed fixture to install
        // a PLM whose later vertical draws would address unrelated host memory.
        int blockIndex = level.GetBlockIndex(new(15, 4));
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreLevelWord = 0;
        // SpawnHardcodedPlm stores the header before setup. Keep identity intact
        // for owner queries and mixed populations, not just instruction dispatch.
        slot.Header = header;
        slot.LoopTimer = 0;
        slot.StartAnimation(new BotwoonWallPlmAnimation(header == PlmHeaderId.ClearBotwoonWall
            ? BotwoonWallProgram.Clear
            : BotwoonWallProgram.Crumble));

        // SpawnHardcodedPLM initializes a new slot's instruction timer to one. Only the
        // live crumble header replaces it: setup `$84:AB28` writes $0040 to the separate
        // PLM instruction-timer allocation before returning to the enemy initializer.
        slot.InstructionTimer = WrappingCountdown.FromRaw(header == PlmHeaderId.CrumbleBotwoonWall
            ? (ushort)64
            : (ushort)1);
        return true;
    }

    /// <summary>
    /// Spawns Spore Spawn's hardcoded 2x2 ceiling mutation at room block (7,30).
    /// Header $B78F animates three crumble frames and then falls through to $AB21's clear;
    /// header $B793 starts directly at that clear list for an already-defeated room.
    /// </summary>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnSporeSpawnCeiling(RoomLevelData level, PlmHeaderId header)
    {
        Ensure.NotNull(level);
        if (header is not (
                PlmHeaderId.CrumbleSporeSpawnCeiling or
                PlmHeaderId.ClearSporeSpawnCeiling))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Spore Spawn ceiling header must be $B78F (crumble) or $B793 (clear).");
        }

        int blockIndex = level.GetBlockIndex(new(7, 30));
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreLevelWord = 0;
        slot.LoopTimer = 0;
        slot.StartAnimation(new SporeSpawnCeilingPlmAnimation(header == PlmHeaderId.CrumbleSporeSpawnCeiling
            ? SporeSpawnCeilingProgram.Crumble
            : SporeSpawnCeilingProgram.Clear));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        return true;
    }

    /// <summary>
    /// Spawns one hardcoded Kraid ceiling/platform mutation and runs setup $84:B3C1.
    /// Setup immediately deactivates the collision type while preserving priority and the
    /// visual block index; the selected ROM list then draws its three crumble frames and
    /// deletes the finite PLM on the authored cadence.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnKraidRoomMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        KraidRoomProgram program = header switch
        {
            PlmHeaderId.CrumbleKraidCeilingIntoBackground1 => KraidRoomProgram.CrumbleCeilingIntoBackground1,
            PlmHeaderId.CrumbleKraidPlatformVariant1 => KraidRoomProgram.SetCeilingToBackground2,
            PlmHeaderId.CrumbleKraidCeilingIntoBackground2 => KraidRoomProgram.CrumbleCeilingIntoBackground2,
            PlmHeaderId.CrumbleKraidPlatformVariant2 => KraidRoomProgram.SetCeilingToBackground3,
            PlmHeaderId.CrumbleKraidCeilingIntoBackground3 => KraidRoomProgram.CrumbleCeilingIntoBackground3,
            PlmHeaderId.ClearKraidCeiling => KraidRoomProgram.ClearCeiling,
            PlmHeaderId.ClearKraidSpikes => KraidRoomProgram.ClearSpikes,
            PlmHeaderId.CrumbleKraidSpikes => KraidRoomProgram.CrumbleSpikes,
            PlmHeaderId.None or
                PlmHeaderId.Nothing or
                PlmHeaderId.CollisionReactionClearCarry or
                PlmHeaderId.RightwardsScrollExtension or
                PlmHeaderId.LeftwardsScrollExtension or
                PlmHeaderId.DownwardsScrollExtension or
                PlmHeaderId.UpwardsScrollExtension or
                PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
                PlmHeaderId.WreckedShipEntranceTreadmillFromEast or
                PlmHeaderId.InsideReactionNothingB653 or
                PlmHeaderId.InsideReactionNothingB657 or
                PlmHeaderId.InsideReactionNothingB65B or
                PlmHeaderId.FillMotherBrainsWall or
                PlmHeaderId.MotherBrainsRoomEscapeDoor or
                PlmHeaderId.MotherBrainsBackgroundRow2 or
                PlmHeaderId.MotherBrainsBackgroundRow3 or
                PlmHeaderId.MotherBrainsBackgroundRow4 or
                PlmHeaderId.MotherBrainsBackgroundRow5 or
                PlmHeaderId.MotherBrainsBackgroundRow6 or
                PlmHeaderId.MotherBrainsBackgroundRow7 or
                PlmHeaderId.MotherBrainsBackgroundRow8 or
                PlmHeaderId.MotherBrainsBackgroundRow9 or
                PlmHeaderId.MotherBrainsBackgroundRowA or
                PlmHeaderId.MotherBrainsBackgroundRowB or
                PlmHeaderId.MotherBrainsBackgroundRowC or
                PlmHeaderId.MotherBrainsBackgroundRowD or
                PlmHeaderId.ClearMotherBrainCeilingBlock or
                PlmHeaderId.ClearMotherBrainCeilingTube or
                PlmHeaderId.ClearMotherBrainBottomMiddleSideTube or
                PlmHeaderId.ClearMotherBrainBottomMiddleTubes or
                PlmHeaderId.ClearMotherBrainBottomLeftTube or
                PlmHeaderId.ClearMotherBrainBottomRightTube or
                PlmHeaderId.InsideReactionBrinstarFloorPlant or
                PlmHeaderId.InsideReactionBrinstarCeilingPlant or
                PlmHeaderId.MapStation or
                PlmHeaderId.MapStationRightAccess or
                PlmHeaderId.MapStationLeftAccess or
                PlmHeaderId.EnergyStation or
                PlmHeaderId.EnergyStationRightAccess or
                PlmHeaderId.EnergyStationLeftAccess or
                PlmHeaderId.MissileStation or
                PlmHeaderId.MissileStationRightAccess or
                PlmHeaderId.MissileStationLeftAccess or
                PlmHeaderId.Nothing84B6F7 or
                PlmHeaderId.Nothing84B6FB or
                PlmHeaderId.ScrollTriggerCollision or
                PlmHeaderId.ScrollTrigger or
                PlmHeaderId.UnusedSolidScrollPLM or
                PlmHeaderId.ElevatorPlatform or
                PlmHeaderId.InsideReactionCrateria80 or
                PlmHeaderId.InsideReactionQuicksandSurface or
                PlmHeaderId.InsideReactionSubmergingQuicksand or
                PlmHeaderId.InsideReactionSandFallsSlow or
                PlmHeaderId.InsideReactionSandFallsFast or
                PlmHeaderId.CollisionReactionQuicksandSurface or
                PlmHeaderId.CollisionReactionSubmergingQuicksand or
                PlmHeaderId.CollisionReactionSandFallsSlow or
                PlmHeaderId.CollisionReactionSandFallsFast or
                PlmHeaderId.ClearCrocomireBridge or
                PlmHeaderId.CrumbleCrocomireBridgeBlock or
                PlmHeaderId.ClearCrocomireBridgeBlock or
                PlmHeaderId.ClearCrocomireInvisibleWall or
                PlmHeaderId.CreateCrocomireInvisibleWall or
                PlmHeaderId.UnusedDraw13BlankAirTiles or
                PlmHeaderId.UnusedDraw13BlankSolidTiles or
                PlmHeaderId.ClearBabyMetroidInvisibleWall or
                PlmHeaderId.CreateBabyMetroidInvisibleWall or
                PlmHeaderId.SaveStationTrigger or
                PlmHeaderId.SaveStation or
                PlmHeaderId.CrumbleAccessToTourianElevator or
                PlmHeaderId.ClearAccessToTourianElevator or
                PlmHeaderId.DrawPhantoonDoorDuringBossFight or
                PlmHeaderId.RestorePhantoonDoorAfterBossFight or
                PlmHeaderId.CrumbleSporeSpawnCeiling or
                PlmHeaderId.ClearSporeSpawnCeiling or
                PlmHeaderId.ClearBotwoonWall or
                PlmHeaderId.CrumbleBotwoonWall or
                PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                PlmHeaderId.SpeedBoosterEscape or
                PlmHeaderId.ShaktoolsRoom or
                PlmHeaderId.MaridiaElevatube or
                PlmHeaderId.OldTourianEscapeShaftFakeWall or
                PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                PlmHeaderId.GateBlock or
                PlmHeaderId.ReactionCrittersEscapeBlock or
                PlmHeaderId.CrittersEscapeBlock or
                PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                PlmHeaderId.BombTorizoGreyDoor or
                PlmHeaderId.WreckedShipAttic or
                PlmHeaderId.CrateriaMainstreetEscapePassage or
                PlmHeaderId.LeftGreenGateTrigger or
                PlmHeaderId.RightGreenGateTrigger or
                PlmHeaderId.LeftRedGateTrigger or
                PlmHeaderId.RightRedGateTrigger or
                PlmHeaderId.LeftBlueGateTrigger or
                PlmHeaderId.RightBlueGateTrigger or
                PlmHeaderId.LeftYellowGateTrigger or
                PlmHeaderId.RightYellowGateTrigger or
                PlmHeaderId.DownwardsOpenGate or
                PlmHeaderId.DownwardGate or
                PlmHeaderId.UpwardsOpenGate or
                PlmHeaderId.UpwardsClosedGate or
                PlmHeaderId.DownwardGateShotBlock or
                PlmHeaderId.UpwardsGateShotblock or
                PlmHeaderId.GenericShotTriggerForAPLM or
                PlmHeaderId.GreyDoorFacingLeft or
                PlmHeaderId.GreyDoorFacingRight or
                PlmHeaderId.GreyDoorFacingUp or
                PlmHeaderId.GreyDoorFacingDown or
                PlmHeaderId.YellowDoorFacingLeft or
                PlmHeaderId.YellowDoorFacingRight or
                PlmHeaderId.YellowDoorFacingUp or
                PlmHeaderId.YellowDoorFacingDown or
                PlmHeaderId.GreenDoorFacingLeft or
                PlmHeaderId.GreenDoorFacingRight or
                PlmHeaderId.GreenDoorFacingUp or
                PlmHeaderId.GreenDoorFacingDown or
                PlmHeaderId.RedDoorFacingLeft or
                PlmHeaderId.RedDoorFacingRight or
                PlmHeaderId.RedDoorFacingUp or
                PlmHeaderId.RedDoorFacingDown or
                PlmHeaderId.BlueDoorFacingLeft or
                PlmHeaderId.BlueDoorFacingRight or
                PlmHeaderId.BlueDoorFacingUp or
                PlmHeaderId.BlueDoorFacingDown or
                PlmHeaderId.BlueDoorClosingFacingLeft or
                PlmHeaderId.BlueDoorClosingFacingRight or
                PlmHeaderId.BlueDoorClosingFacingUp or
                PlmHeaderId.BlueDoorClosingFacingDown or
                PlmHeaderId.MotherBrainEscapeRoomGate or
                PlmHeaderId.MotherBrainEscapeRoomGateClosing or
                PlmHeaderId.Plm1x1RespawningCrumbleBlock or
                PlmHeaderId.Plm2x1RespawningCrumbleBlock or
                PlmHeaderId.Plm1x2RespawningCrumbleBlock or
                PlmHeaderId.Plm2x2RespawningCrumbleBlock or
                PlmHeaderId.BombReactionSpeedBoostBlock or
                PlmHeaderId.SpeedBlockBrinstarSlowRespawning or
                PlmHeaderId.SpeedBlockBrinstarSlowPermanent or
                PlmHeaderId.SpeedBlockRespawning or
                PlmHeaderId.SpeedBlockDachoraRespawning or
                PlmHeaderId.SpeedBlockPermanent or
                PlmHeaderId.ContactCrumble1x1Respawning or
                PlmHeaderId.ContactCrumble2x1Respawning or
                PlmHeaderId.ContactCrumble1x2Respawning or
                PlmHeaderId.ContactCrumble2x2Respawning or
                PlmHeaderId.ContactCrumble1x1Permanent or
                PlmHeaderId.ContactCrumble2x1Permanent or
                PlmHeaderId.ContactCrumble1x2Permanent or
                PlmHeaderId.ContactCrumble2x2Permanent or
                PlmHeaderId.Reaction1x1RespawningShotBlock or
                PlmHeaderId.Reaction2x1RespawningShotBlock or
                PlmHeaderId.Reaction1x2RespawningShotBlock or
                PlmHeaderId.Reaction2x2RespawningShotBlock or
                PlmHeaderId.Reaction1x1ShotBlock or
                PlmHeaderId.Reaction2x1ShotBlock or
                PlmHeaderId.Reaction1x2ShotBlock or
                PlmHeaderId.Reaction2x2ShotBlock or
                PlmHeaderId.ReactionRespawningPowerBombBlock or
                PlmHeaderId.ReactionPowerBombBlock or
                PlmHeaderId.ReactionRespawningSuperMissileBlock or
                PlmHeaderId.ReactionSuperMissileBlock or
                PlmHeaderId.EnemyBreakableBlock or
                PlmHeaderId.Collision1x1RespawningBombBlock or
                PlmHeaderId.Collision2x1RespawningBombBlock or
                PlmHeaderId.Collision1x2RespawningBombBlock or
                PlmHeaderId.Collision2x2RespawningBombBlock or
                PlmHeaderId.Collision1x1BombBlock or
                PlmHeaderId.Collision2x1BombBlock or
                PlmHeaderId.Collision1x2BombBlock or
                PlmHeaderId.Collision2x2BombBlock or
                PlmHeaderId.Reaction1x1RespawningBombBlock or
                PlmHeaderId.Reaction2x1RespawningBombBlock or
                PlmHeaderId.Reaction1x2RespawningBombBlock or
                PlmHeaderId.Reaction2x2RespawningBombBlock or
                PlmHeaderId.Reaction1x1BombBlock or
                PlmHeaderId.Reaction2x1BombBlock or
                PlmHeaderId.Reaction1x2BombBlock or
                PlmHeaderId.Reaction2x2BombBlock or
                PlmHeaderId.GrappledGrappleBlock or
                PlmHeaderId.GrappledRespawningBreakableGrappleBlock or
                PlmHeaderId.GrappledBreakableGrappleBlock or
                PlmHeaderId.GrappledGenericSpikeBlock or
                PlmHeaderId.GrappledDraygonsBrokenTurret or
                PlmHeaderId.UnusedBlueBrinstarFaceBlock or
                PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                PlmHeaderId.UnusedShotBlock or
                PlmHeaderId.UnusedGrappleBlock or
                PlmHeaderId.LowerNorfairChozoHand or
                PlmHeaderId.CollisionLowerNorfairChozoHandCheck or
                PlmHeaderId.MotherBrainGlass or
                PlmHeaderId.BombTorizoHand or
                PlmHeaderId.WreckedShipChozoHand or
                PlmHeaderId.CollisionWreckedShipChozoHandCheck or
                PlmHeaderId.ClearSlopeAccessForWreckedShipChozo or
                PlmHeaderId.BlockSlopeAccessForWreckedShipChozo or
                PlmHeaderId.NoobTube or
                PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                PlmHeaderId.EyeDoorEyeFacingRight or
                PlmHeaderId.EyeDoorFacingRight or
                PlmHeaderId.EyeDoorBottomFacingRight or
                PlmHeaderId.EyeDoorEyeFacingLeft or
                PlmHeaderId.EyeDoorFacingLeft or
                PlmHeaderId.EyeDoorBottomFacingLeft or
                PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed or
                PlmHeaderId.DraygonCannonFacingLeft or
                PlmHeaderId.DraygonCannonFacingLeftUnshielded or
                PlmHeaderId.ItemCollisionDetection or
                PlmHeaderId.ExposedEnergyTank or
                PlmHeaderId.ExposedMissileTank or
                PlmHeaderId.ExposedSuperMissileTank or
                PlmHeaderId.ExposedPowerBombTank or
                PlmHeaderId.ExposedBombs or
                PlmHeaderId.ExposedChargeBeam or
                PlmHeaderId.ExposedIceBeam or
                PlmHeaderId.ExposedHiJumpBoots or
                PlmHeaderId.ExposedSpeedBooster or
                PlmHeaderId.ExposedWaveBeam or
                PlmHeaderId.ExposedSpazerBeam or
                PlmHeaderId.ExposedSpringBall or
                PlmHeaderId.ExposedVariaSuit or
                PlmHeaderId.ExposedGravitySuit or
                PlmHeaderId.ExposedXrayScope or
                PlmHeaderId.ExposedPlasmaBeam or
                PlmHeaderId.ExposedGrappleBeam or
                PlmHeaderId.ExposedSpaceJump or
                PlmHeaderId.ExposedScrewAttack or
                PlmHeaderId.ExposedMorphBall or
                PlmHeaderId.ExposedReserveTank or
                PlmHeaderId.ChozoEnergyTank or
                PlmHeaderId.ChozoMissileTank or
                PlmHeaderId.ChozoSuperMissileTank or
                PlmHeaderId.ChozoPowerBombTank or
                PlmHeaderId.ChozoBombs or
                PlmHeaderId.ChozoChargeBeam or
                PlmHeaderId.ChozoIceBeam or
                PlmHeaderId.ChozoHiJumpBoots or
                PlmHeaderId.ChozoSpeedBooster or
                PlmHeaderId.ChozoWaveBeam or
                PlmHeaderId.ChozoSpazerBeam or
                PlmHeaderId.ChozoSpringBall or
                PlmHeaderId.ChozoVariaSuit or
                PlmHeaderId.ChozoGravitySuit or
                PlmHeaderId.ChozoXrayScope or
                PlmHeaderId.ChozoPlasmaBeam or
                PlmHeaderId.ChozoGrappleBeam or
                PlmHeaderId.ChozoSpaceJump or
                PlmHeaderId.ChozoScrewAttack or
                PlmHeaderId.ChozoMorphBall or
                PlmHeaderId.ChozoReserveTank or
                PlmHeaderId.ShotBlockEnergyTank or
                PlmHeaderId.ShotBlockMissileTank or
                PlmHeaderId.ShotBlockSuperMissileTank or
                PlmHeaderId.ShotBlockPowerBombTank or
                PlmHeaderId.ShotBlockBombs or
                PlmHeaderId.ShotBlockChargeBeam or
                PlmHeaderId.ShotBlockIceBeam or
                PlmHeaderId.ShotBlockHiJumpBoots or
                PlmHeaderId.ShotBlockSpeedBooster or
                PlmHeaderId.ShotBlockWaveBeam or
                PlmHeaderId.ShotBlockSpazerBeam or
                PlmHeaderId.ShotBlockSpringBall or
                PlmHeaderId.ShotBlockVariaSuit or
                PlmHeaderId.ShotBlockGravitySuit or
                PlmHeaderId.ShotBlockXrayScope or
                PlmHeaderId.ShotBlockPlasmaBeam or
                PlmHeaderId.ShotBlockGrappleBeam or
                PlmHeaderId.ShotBlockSpaceJump or
                PlmHeaderId.ShotBlockScrewAttack or
                PlmHeaderId.ShotBlockMorphBall or
                PlmHeaderId.ShotBlockReserveTank => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Kraid room mutation header is outside the authored Kraid mutation set."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Kraid room mutation header is outside the authored Kraid mutation set."),
        };

        int blockIndex = level.GetBlockIndex(block);
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = header;
        slot.BlockIndex = blockIndex;
        slot.StartAnimation(new KraidRoomPlmAnimation(program));
        slot.InstructionTimer = WrappingCountdown.Start(1);

        // PlmSetup_DeactivatePlm clears level-word bits 12..14, not all four collision
        // bits. Bit 15 is the independent tile priority flag and survives setup.
        ushort authored = level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw;
        level.SetForegroundEntry(blockIndex, new RoomLevelWord(authored).WithPlmDeactivation().Raw);
        return true;
    }

    /// <summary>
    /// Spawns one of Crocomire's five hardcoded arena PLMs at its literal room-block origin.
    /// </summary>
    /// <remarks>
    /// The instruction pointers are the words selected by entries $84:B747-$B757 after
    /// their no-op setup. The shared instruction interpreter below already understands the
    /// positive timer/draw pair and $86BC deletion used by every one of these lists.
    /// </remarks>
    /// <returns>False only when all 40 native PLM slots are occupied.</returns>
    public bool TrySpawnCrocomireArenaMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        CrocomireArenaProgram program = header switch
        {
            PlmHeaderId.ClearCrocomireBridge => CrocomireArenaProgram.ClearBridge,
            PlmHeaderId.CrumbleCrocomireBridgeBlock => CrocomireArenaProgram.CrumbleBridgeBlock,
            PlmHeaderId.ClearCrocomireBridgeBlock => CrocomireArenaProgram.ClearBridgeBlock,
            PlmHeaderId.ClearCrocomireInvisibleWall => CrocomireArenaProgram.ClearInvisibleWall,
            PlmHeaderId.CreateCrocomireInvisibleWall => CrocomireArenaProgram.CreateInvisibleWall,
            PlmHeaderId.None or
                PlmHeaderId.Nothing or
                PlmHeaderId.CollisionReactionClearCarry or
                PlmHeaderId.RightwardsScrollExtension or
                PlmHeaderId.LeftwardsScrollExtension or
                PlmHeaderId.DownwardsScrollExtension or
                PlmHeaderId.UpwardsScrollExtension or
                PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
                PlmHeaderId.WreckedShipEntranceTreadmillFromEast or
                PlmHeaderId.InsideReactionNothingB653 or
                PlmHeaderId.InsideReactionNothingB657 or
                PlmHeaderId.InsideReactionNothingB65B or
                PlmHeaderId.FillMotherBrainsWall or
                PlmHeaderId.MotherBrainsRoomEscapeDoor or
                PlmHeaderId.MotherBrainsBackgroundRow2 or
                PlmHeaderId.MotherBrainsBackgroundRow3 or
                PlmHeaderId.MotherBrainsBackgroundRow4 or
                PlmHeaderId.MotherBrainsBackgroundRow5 or
                PlmHeaderId.MotherBrainsBackgroundRow6 or
                PlmHeaderId.MotherBrainsBackgroundRow7 or
                PlmHeaderId.MotherBrainsBackgroundRow8 or
                PlmHeaderId.MotherBrainsBackgroundRow9 or
                PlmHeaderId.MotherBrainsBackgroundRowA or
                PlmHeaderId.MotherBrainsBackgroundRowB or
                PlmHeaderId.MotherBrainsBackgroundRowC or
                PlmHeaderId.MotherBrainsBackgroundRowD or
                PlmHeaderId.ClearMotherBrainCeilingBlock or
                PlmHeaderId.ClearMotherBrainCeilingTube or
                PlmHeaderId.ClearMotherBrainBottomMiddleSideTube or
                PlmHeaderId.ClearMotherBrainBottomMiddleTubes or
                PlmHeaderId.ClearMotherBrainBottomLeftTube or
                PlmHeaderId.ClearMotherBrainBottomRightTube or
                PlmHeaderId.InsideReactionBrinstarFloorPlant or
                PlmHeaderId.InsideReactionBrinstarCeilingPlant or
                PlmHeaderId.MapStation or
                PlmHeaderId.MapStationRightAccess or
                PlmHeaderId.MapStationLeftAccess or
                PlmHeaderId.EnergyStation or
                PlmHeaderId.EnergyStationRightAccess or
                PlmHeaderId.EnergyStationLeftAccess or
                PlmHeaderId.MissileStation or
                PlmHeaderId.MissileStationRightAccess or
                PlmHeaderId.MissileStationLeftAccess or
                PlmHeaderId.Nothing84B6F7 or
                PlmHeaderId.Nothing84B6FB or
                PlmHeaderId.ScrollTriggerCollision or
                PlmHeaderId.ScrollTrigger or
                PlmHeaderId.UnusedSolidScrollPLM or
                PlmHeaderId.ElevatorPlatform or
                PlmHeaderId.InsideReactionCrateria80 or
                PlmHeaderId.InsideReactionQuicksandSurface or
                PlmHeaderId.InsideReactionSubmergingQuicksand or
                PlmHeaderId.InsideReactionSandFallsSlow or
                PlmHeaderId.InsideReactionSandFallsFast or
                PlmHeaderId.CollisionReactionQuicksandSurface or
                PlmHeaderId.CollisionReactionSubmergingQuicksand or
                PlmHeaderId.CollisionReactionSandFallsSlow or
                PlmHeaderId.CollisionReactionSandFallsFast or
                PlmHeaderId.UnusedDraw13BlankAirTiles or
                PlmHeaderId.UnusedDraw13BlankSolidTiles or
                PlmHeaderId.ClearBabyMetroidInvisibleWall or
                PlmHeaderId.CreateBabyMetroidInvisibleWall or
                PlmHeaderId.SaveStationTrigger or
                PlmHeaderId.SaveStation or
                PlmHeaderId.CrumbleAccessToTourianElevator or
                PlmHeaderId.ClearAccessToTourianElevator or
                PlmHeaderId.DrawPhantoonDoorDuringBossFight or
                PlmHeaderId.RestorePhantoonDoorAfterBossFight or
                PlmHeaderId.CrumbleSporeSpawnCeiling or
                PlmHeaderId.ClearSporeSpawnCeiling or
                PlmHeaderId.ClearBotwoonWall or
                PlmHeaderId.CrumbleBotwoonWall or
                PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
                PlmHeaderId.CrumbleKraidPlatformVariant1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
                PlmHeaderId.CrumbleKraidPlatformVariant2 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
                PlmHeaderId.ClearKraidCeiling or
                PlmHeaderId.ClearKraidSpikes or
                PlmHeaderId.CrumbleKraidSpikes or
                PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                PlmHeaderId.SpeedBoosterEscape or
                PlmHeaderId.ShaktoolsRoom or
                PlmHeaderId.MaridiaElevatube or
                PlmHeaderId.OldTourianEscapeShaftFakeWall or
                PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                PlmHeaderId.GateBlock or
                PlmHeaderId.ReactionCrittersEscapeBlock or
                PlmHeaderId.CrittersEscapeBlock or
                PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                PlmHeaderId.BombTorizoGreyDoor or
                PlmHeaderId.WreckedShipAttic or
                PlmHeaderId.CrateriaMainstreetEscapePassage or
                PlmHeaderId.LeftGreenGateTrigger or
                PlmHeaderId.RightGreenGateTrigger or
                PlmHeaderId.LeftRedGateTrigger or
                PlmHeaderId.RightRedGateTrigger or
                PlmHeaderId.LeftBlueGateTrigger or
                PlmHeaderId.RightBlueGateTrigger or
                PlmHeaderId.LeftYellowGateTrigger or
                PlmHeaderId.RightYellowGateTrigger or
                PlmHeaderId.DownwardsOpenGate or
                PlmHeaderId.DownwardGate or
                PlmHeaderId.UpwardsOpenGate or
                PlmHeaderId.UpwardsClosedGate or
                PlmHeaderId.DownwardGateShotBlock or
                PlmHeaderId.UpwardsGateShotblock or
                PlmHeaderId.GenericShotTriggerForAPLM or
                PlmHeaderId.GreyDoorFacingLeft or
                PlmHeaderId.GreyDoorFacingRight or
                PlmHeaderId.GreyDoorFacingUp or
                PlmHeaderId.GreyDoorFacingDown or
                PlmHeaderId.YellowDoorFacingLeft or
                PlmHeaderId.YellowDoorFacingRight or
                PlmHeaderId.YellowDoorFacingUp or
                PlmHeaderId.YellowDoorFacingDown or
                PlmHeaderId.GreenDoorFacingLeft or
                PlmHeaderId.GreenDoorFacingRight or
                PlmHeaderId.GreenDoorFacingUp or
                PlmHeaderId.GreenDoorFacingDown or
                PlmHeaderId.RedDoorFacingLeft or
                PlmHeaderId.RedDoorFacingRight or
                PlmHeaderId.RedDoorFacingUp or
                PlmHeaderId.RedDoorFacingDown or
                PlmHeaderId.BlueDoorFacingLeft or
                PlmHeaderId.BlueDoorFacingRight or
                PlmHeaderId.BlueDoorFacingUp or
                PlmHeaderId.BlueDoorFacingDown or
                PlmHeaderId.BlueDoorClosingFacingLeft or
                PlmHeaderId.BlueDoorClosingFacingRight or
                PlmHeaderId.BlueDoorClosingFacingUp or
                PlmHeaderId.BlueDoorClosingFacingDown or
                PlmHeaderId.MotherBrainEscapeRoomGate or
                PlmHeaderId.MotherBrainEscapeRoomGateClosing or
                PlmHeaderId.Plm1x1RespawningCrumbleBlock or
                PlmHeaderId.Plm2x1RespawningCrumbleBlock or
                PlmHeaderId.Plm1x2RespawningCrumbleBlock or
                PlmHeaderId.Plm2x2RespawningCrumbleBlock or
                PlmHeaderId.BombReactionSpeedBoostBlock or
                PlmHeaderId.SpeedBlockBrinstarSlowRespawning or
                PlmHeaderId.SpeedBlockBrinstarSlowPermanent or
                PlmHeaderId.SpeedBlockRespawning or
                PlmHeaderId.SpeedBlockDachoraRespawning or
                PlmHeaderId.SpeedBlockPermanent or
                PlmHeaderId.ContactCrumble1x1Respawning or
                PlmHeaderId.ContactCrumble2x1Respawning or
                PlmHeaderId.ContactCrumble1x2Respawning or
                PlmHeaderId.ContactCrumble2x2Respawning or
                PlmHeaderId.ContactCrumble1x1Permanent or
                PlmHeaderId.ContactCrumble2x1Permanent or
                PlmHeaderId.ContactCrumble1x2Permanent or
                PlmHeaderId.ContactCrumble2x2Permanent or
                PlmHeaderId.Reaction1x1RespawningShotBlock or
                PlmHeaderId.Reaction2x1RespawningShotBlock or
                PlmHeaderId.Reaction1x2RespawningShotBlock or
                PlmHeaderId.Reaction2x2RespawningShotBlock or
                PlmHeaderId.Reaction1x1ShotBlock or
                PlmHeaderId.Reaction2x1ShotBlock or
                PlmHeaderId.Reaction1x2ShotBlock or
                PlmHeaderId.Reaction2x2ShotBlock or
                PlmHeaderId.ReactionRespawningPowerBombBlock or
                PlmHeaderId.ReactionPowerBombBlock or
                PlmHeaderId.ReactionRespawningSuperMissileBlock or
                PlmHeaderId.ReactionSuperMissileBlock or
                PlmHeaderId.EnemyBreakableBlock or
                PlmHeaderId.Collision1x1RespawningBombBlock or
                PlmHeaderId.Collision2x1RespawningBombBlock or
                PlmHeaderId.Collision1x2RespawningBombBlock or
                PlmHeaderId.Collision2x2RespawningBombBlock or
                PlmHeaderId.Collision1x1BombBlock or
                PlmHeaderId.Collision2x1BombBlock or
                PlmHeaderId.Collision1x2BombBlock or
                PlmHeaderId.Collision2x2BombBlock or
                PlmHeaderId.Reaction1x1RespawningBombBlock or
                PlmHeaderId.Reaction2x1RespawningBombBlock or
                PlmHeaderId.Reaction1x2RespawningBombBlock or
                PlmHeaderId.Reaction2x2RespawningBombBlock or
                PlmHeaderId.Reaction1x1BombBlock or
                PlmHeaderId.Reaction2x1BombBlock or
                PlmHeaderId.Reaction1x2BombBlock or
                PlmHeaderId.Reaction2x2BombBlock or
                PlmHeaderId.GrappledGrappleBlock or
                PlmHeaderId.GrappledRespawningBreakableGrappleBlock or
                PlmHeaderId.GrappledBreakableGrappleBlock or
                PlmHeaderId.GrappledGenericSpikeBlock or
                PlmHeaderId.GrappledDraygonsBrokenTurret or
                PlmHeaderId.UnusedBlueBrinstarFaceBlock or
                PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                PlmHeaderId.UnusedShotBlock or
                PlmHeaderId.UnusedGrappleBlock or
                PlmHeaderId.LowerNorfairChozoHand or
                PlmHeaderId.CollisionLowerNorfairChozoHandCheck or
                PlmHeaderId.MotherBrainGlass or
                PlmHeaderId.BombTorizoHand or
                PlmHeaderId.WreckedShipChozoHand or
                PlmHeaderId.CollisionWreckedShipChozoHandCheck or
                PlmHeaderId.ClearSlopeAccessForWreckedShipChozo or
                PlmHeaderId.BlockSlopeAccessForWreckedShipChozo or
                PlmHeaderId.NoobTube or
                PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                PlmHeaderId.EyeDoorEyeFacingRight or
                PlmHeaderId.EyeDoorFacingRight or
                PlmHeaderId.EyeDoorBottomFacingRight or
                PlmHeaderId.EyeDoorEyeFacingLeft or
                PlmHeaderId.EyeDoorFacingLeft or
                PlmHeaderId.EyeDoorBottomFacingLeft or
                PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed or
                PlmHeaderId.DraygonCannonFacingLeft or
                PlmHeaderId.DraygonCannonFacingLeftUnshielded or
                PlmHeaderId.ItemCollisionDetection or
                PlmHeaderId.ExposedEnergyTank or
                PlmHeaderId.ExposedMissileTank or
                PlmHeaderId.ExposedSuperMissileTank or
                PlmHeaderId.ExposedPowerBombTank or
                PlmHeaderId.ExposedBombs or
                PlmHeaderId.ExposedChargeBeam or
                PlmHeaderId.ExposedIceBeam or
                PlmHeaderId.ExposedHiJumpBoots or
                PlmHeaderId.ExposedSpeedBooster or
                PlmHeaderId.ExposedWaveBeam or
                PlmHeaderId.ExposedSpazerBeam or
                PlmHeaderId.ExposedSpringBall or
                PlmHeaderId.ExposedVariaSuit or
                PlmHeaderId.ExposedGravitySuit or
                PlmHeaderId.ExposedXrayScope or
                PlmHeaderId.ExposedPlasmaBeam or
                PlmHeaderId.ExposedGrappleBeam or
                PlmHeaderId.ExposedSpaceJump or
                PlmHeaderId.ExposedScrewAttack or
                PlmHeaderId.ExposedMorphBall or
                PlmHeaderId.ExposedReserveTank or
                PlmHeaderId.ChozoEnergyTank or
                PlmHeaderId.ChozoMissileTank or
                PlmHeaderId.ChozoSuperMissileTank or
                PlmHeaderId.ChozoPowerBombTank or
                PlmHeaderId.ChozoBombs or
                PlmHeaderId.ChozoChargeBeam or
                PlmHeaderId.ChozoIceBeam or
                PlmHeaderId.ChozoHiJumpBoots or
                PlmHeaderId.ChozoSpeedBooster or
                PlmHeaderId.ChozoWaveBeam or
                PlmHeaderId.ChozoSpazerBeam or
                PlmHeaderId.ChozoSpringBall or
                PlmHeaderId.ChozoVariaSuit or
                PlmHeaderId.ChozoGravitySuit or
                PlmHeaderId.ChozoXrayScope or
                PlmHeaderId.ChozoPlasmaBeam or
                PlmHeaderId.ChozoGrappleBeam or
                PlmHeaderId.ChozoSpaceJump or
                PlmHeaderId.ChozoScrewAttack or
                PlmHeaderId.ChozoMorphBall or
                PlmHeaderId.ChozoReserveTank or
                PlmHeaderId.ShotBlockEnergyTank or
                PlmHeaderId.ShotBlockMissileTank or
                PlmHeaderId.ShotBlockSuperMissileTank or
                PlmHeaderId.ShotBlockPowerBombTank or
                PlmHeaderId.ShotBlockBombs or
                PlmHeaderId.ShotBlockChargeBeam or
                PlmHeaderId.ShotBlockIceBeam or
                PlmHeaderId.ShotBlockHiJumpBoots or
                PlmHeaderId.ShotBlockSpeedBooster or
                PlmHeaderId.ShotBlockWaveBeam or
                PlmHeaderId.ShotBlockSpazerBeam or
                PlmHeaderId.ShotBlockSpringBall or
                PlmHeaderId.ShotBlockVariaSuit or
                PlmHeaderId.ShotBlockGravitySuit or
                PlmHeaderId.ShotBlockXrayScope or
                PlmHeaderId.ShotBlockPlasmaBeam or
                PlmHeaderId.ShotBlockGrappleBeam or
                PlmHeaderId.ShotBlockSpaceJump or
                PlmHeaderId.ShotBlockScrewAttack or
                PlmHeaderId.ShotBlockMorphBall or
                PlmHeaderId.ShotBlockReserveTank => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Crocomire arena header must be $B747, $B74B, $B74F, $B753, or $B757."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Crocomire arena header must be $B747, $B74B, $B74F, $B753, or $B757."),
        };

        int blockIndex = level.GetBlockIndex(block);
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = blockIndex;
        slot.RestoreLevelWord = 0;
        slot.LoopTimer = 0;
        slot.StartAnimation(new CrocomireArenaPlmAnimation(program));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Header = header;
        return true;
    }

    /// <summary>
    /// Spawns Shitroid's hardcoded ten-block vertical wall mutation at `$84:B763/$B767`.
    /// Setup executes synchronously inside <c>SpawnHardcodedPLM</c>: `$B763` clears the
    /// collision nibble and `$B767` replaces it with type eight while preserving the low
    /// twelve bits of every authored level word.
    /// </summary>
    /// <returns>False when all forty native PLM slots are occupied; no terrain is changed.</returns>
    public bool TrySpawnShitroidWallMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        if (header is not (
            PlmHeaderId.ClearBabyMetroidInvisibleWall or
            PlmHeaderId.CreateBabyMetroidInvisibleWall))
        {
            throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Shitroid wall header must be $B763 or $B767.");
        }

        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = header;
        slot.BlockIndex = level.GetBlockIndex(block);
        slot.RestoreLevelWord = 0;
        slot.LoopTimer = 0;
        slot.PreInstruction = PlmPreInstructions.Inert;
        slot.RoomArgument = 0;
        PlmProgramStart.Delete.StartOn(slot);
        slot.InstructionTimer = WrappingCountdown.Start(1);

        // Both setup routines walk downward through ten blocks in the same column.
        // The mutation is part of setup, so it must occur before the PLM handler's
        // instruction timer is allowed to consume and delete this allocated slot.
        for (int rowOffset = 0; rowOffset < 10; rowOffset++)
        {
            int blockIndex = level.GetBlockIndex(block + new Offset16<RoomBlocks>(0, checked((short)rowOffset)));
            ushort current = level.GetCollisionBlockByIndex(blockIndex).PackedWord.Raw;
            ushort replacement = new RoomLevelWord(current).WithCollisionType(header == PlmHeaderId.CreateBabyMetroidInvisibleWall
                ? RoomCollisionType.SolidBlock
                : RoomCollisionType.Air).Raw;
            level.SetForegroundEntry(blockIndex, replacement);
        }
        return true;
    }

    /// <summary>
    /// Allocates one of Mother Brain's literal fake-death room mutations from
    /// <c>$84:B673-$84:B6C7</c>. Every listed header uses setup $B3D0 (deactivate) and a
    /// one-frame draw/delete list, so the shared PLM interpreter—not boss-specific terrain
    /// painting—owns the actual room edit and tilemap upload.
    /// </summary>
    /// <returns>False only when all forty native PLM slots are occupied.</returns>
    public bool TrySpawnMotherBrainMutation(
        RoomLevelData level,
        Position16<RoomBlocks> block,
        PlmHeaderId header)
    {
        Ensure.NotNull(level);
        MotherBrainFakeDeathProgram program = header switch
        {
            PlmHeaderId.FillMotherBrainsWall => MotherBrainFakeDeathProgram.FillWall,
            PlmHeaderId.MotherBrainsRoomEscapeDoor => MotherBrainFakeDeathProgram.EscapeDoor,
            PlmHeaderId.MotherBrainsBackgroundRow2 => MotherBrainFakeDeathProgram.BackgroundRow2,
            PlmHeaderId.MotherBrainsBackgroundRow3 => MotherBrainFakeDeathProgram.BackgroundRow3,
            PlmHeaderId.MotherBrainsBackgroundRow4 => MotherBrainFakeDeathProgram.BackgroundRow4,
            PlmHeaderId.MotherBrainsBackgroundRow5 => MotherBrainFakeDeathProgram.BackgroundRow5,
            PlmHeaderId.MotherBrainsBackgroundRow6 => MotherBrainFakeDeathProgram.BackgroundRow6,
            PlmHeaderId.MotherBrainsBackgroundRow7 => MotherBrainFakeDeathProgram.BackgroundRow7,
            PlmHeaderId.MotherBrainsBackgroundRow8 => MotherBrainFakeDeathProgram.BackgroundRow8,
            PlmHeaderId.MotherBrainsBackgroundRow9 => MotherBrainFakeDeathProgram.BackgroundRow9,
            PlmHeaderId.MotherBrainsBackgroundRowA => MotherBrainFakeDeathProgram.BackgroundRowA,
            PlmHeaderId.MotherBrainsBackgroundRowB => MotherBrainFakeDeathProgram.BackgroundRowB,
            PlmHeaderId.MotherBrainsBackgroundRowC => MotherBrainFakeDeathProgram.BackgroundRowC,
            PlmHeaderId.MotherBrainsBackgroundRowD => MotherBrainFakeDeathProgram.BackgroundRowD,
            PlmHeaderId.ClearMotherBrainCeilingBlock => MotherBrainFakeDeathProgram.ClearCeilingBlock,
            PlmHeaderId.ClearMotherBrainCeilingTube => MotherBrainFakeDeathProgram.ClearCeilingTube,
            PlmHeaderId.ClearMotherBrainBottomMiddleSideTube => MotherBrainFakeDeathProgram.ClearBottomMiddleSideTube,
            PlmHeaderId.ClearMotherBrainBottomMiddleTubes => MotherBrainFakeDeathProgram.ClearBottomMiddleTubes,
            PlmHeaderId.ClearMotherBrainBottomLeftTube => MotherBrainFakeDeathProgram.ClearBottomLeftTube,
            PlmHeaderId.ClearMotherBrainBottomRightTube => MotherBrainFakeDeathProgram.ClearBottomRightTube,
            PlmHeaderId.None or
                PlmHeaderId.Nothing or
                PlmHeaderId.CollisionReactionClearCarry or
                PlmHeaderId.RightwardsScrollExtension or
                PlmHeaderId.LeftwardsScrollExtension or
                PlmHeaderId.DownwardsScrollExtension or
                PlmHeaderId.UpwardsScrollExtension or
                PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
                PlmHeaderId.WreckedShipEntranceTreadmillFromEast or
                PlmHeaderId.InsideReactionNothingB653 or
                PlmHeaderId.InsideReactionNothingB657 or
                PlmHeaderId.InsideReactionNothingB65B or
                PlmHeaderId.InsideReactionBrinstarFloorPlant or
                PlmHeaderId.InsideReactionBrinstarCeilingPlant or
                PlmHeaderId.MapStation or
                PlmHeaderId.MapStationRightAccess or
                PlmHeaderId.MapStationLeftAccess or
                PlmHeaderId.EnergyStation or
                PlmHeaderId.EnergyStationRightAccess or
                PlmHeaderId.EnergyStationLeftAccess or
                PlmHeaderId.MissileStation or
                PlmHeaderId.MissileStationRightAccess or
                PlmHeaderId.MissileStationLeftAccess or
                PlmHeaderId.Nothing84B6F7 or
                PlmHeaderId.Nothing84B6FB or
                PlmHeaderId.ScrollTriggerCollision or
                PlmHeaderId.ScrollTrigger or
                PlmHeaderId.UnusedSolidScrollPLM or
                PlmHeaderId.ElevatorPlatform or
                PlmHeaderId.InsideReactionCrateria80 or
                PlmHeaderId.InsideReactionQuicksandSurface or
                PlmHeaderId.InsideReactionSubmergingQuicksand or
                PlmHeaderId.InsideReactionSandFallsSlow or
                PlmHeaderId.InsideReactionSandFallsFast or
                PlmHeaderId.CollisionReactionQuicksandSurface or
                PlmHeaderId.CollisionReactionSubmergingQuicksand or
                PlmHeaderId.CollisionReactionSandFallsSlow or
                PlmHeaderId.CollisionReactionSandFallsFast or
                PlmHeaderId.ClearCrocomireBridge or
                PlmHeaderId.CrumbleCrocomireBridgeBlock or
                PlmHeaderId.ClearCrocomireBridgeBlock or
                PlmHeaderId.ClearCrocomireInvisibleWall or
                PlmHeaderId.CreateCrocomireInvisibleWall or
                PlmHeaderId.UnusedDraw13BlankAirTiles or
                PlmHeaderId.UnusedDraw13BlankSolidTiles or
                PlmHeaderId.ClearBabyMetroidInvisibleWall or
                PlmHeaderId.CreateBabyMetroidInvisibleWall or
                PlmHeaderId.SaveStationTrigger or
                PlmHeaderId.SaveStation or
                PlmHeaderId.CrumbleAccessToTourianElevator or
                PlmHeaderId.ClearAccessToTourianElevator or
                PlmHeaderId.DrawPhantoonDoorDuringBossFight or
                PlmHeaderId.RestorePhantoonDoorAfterBossFight or
                PlmHeaderId.CrumbleSporeSpawnCeiling or
                PlmHeaderId.ClearSporeSpawnCeiling or
                PlmHeaderId.ClearBotwoonWall or
                PlmHeaderId.CrumbleBotwoonWall or
                PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
                PlmHeaderId.CrumbleKraidPlatformVariant1 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
                PlmHeaderId.CrumbleKraidPlatformVariant2 or
                PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
                PlmHeaderId.ClearKraidCeiling or
                PlmHeaderId.ClearKraidSpikes or
                PlmHeaderId.CrumbleKraidSpikes or
                PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                PlmHeaderId.SpeedBoosterEscape or
                PlmHeaderId.ShaktoolsRoom or
                PlmHeaderId.MaridiaElevatube or
                PlmHeaderId.OldTourianEscapeShaftFakeWall or
                PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                PlmHeaderId.GateBlock or
                PlmHeaderId.ReactionCrittersEscapeBlock or
                PlmHeaderId.CrittersEscapeBlock or
                PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                PlmHeaderId.BombTorizoGreyDoor or
                PlmHeaderId.WreckedShipAttic or
                PlmHeaderId.CrateriaMainstreetEscapePassage or
                PlmHeaderId.LeftGreenGateTrigger or
                PlmHeaderId.RightGreenGateTrigger or
                PlmHeaderId.LeftRedGateTrigger or
                PlmHeaderId.RightRedGateTrigger or
                PlmHeaderId.LeftBlueGateTrigger or
                PlmHeaderId.RightBlueGateTrigger or
                PlmHeaderId.LeftYellowGateTrigger or
                PlmHeaderId.RightYellowGateTrigger or
                PlmHeaderId.DownwardsOpenGate or
                PlmHeaderId.DownwardGate or
                PlmHeaderId.UpwardsOpenGate or
                PlmHeaderId.UpwardsClosedGate or
                PlmHeaderId.DownwardGateShotBlock or
                PlmHeaderId.UpwardsGateShotblock or
                PlmHeaderId.GenericShotTriggerForAPLM or
                PlmHeaderId.GreyDoorFacingLeft or
                PlmHeaderId.GreyDoorFacingRight or
                PlmHeaderId.GreyDoorFacingUp or
                PlmHeaderId.GreyDoorFacingDown or
                PlmHeaderId.YellowDoorFacingLeft or
                PlmHeaderId.YellowDoorFacingRight or
                PlmHeaderId.YellowDoorFacingUp or
                PlmHeaderId.YellowDoorFacingDown or
                PlmHeaderId.GreenDoorFacingLeft or
                PlmHeaderId.GreenDoorFacingRight or
                PlmHeaderId.GreenDoorFacingUp or
                PlmHeaderId.GreenDoorFacingDown or
                PlmHeaderId.RedDoorFacingLeft or
                PlmHeaderId.RedDoorFacingRight or
                PlmHeaderId.RedDoorFacingUp or
                PlmHeaderId.RedDoorFacingDown or
                PlmHeaderId.BlueDoorFacingLeft or
                PlmHeaderId.BlueDoorFacingRight or
                PlmHeaderId.BlueDoorFacingUp or
                PlmHeaderId.BlueDoorFacingDown or
                PlmHeaderId.BlueDoorClosingFacingLeft or
                PlmHeaderId.BlueDoorClosingFacingRight or
                PlmHeaderId.BlueDoorClosingFacingUp or
                PlmHeaderId.BlueDoorClosingFacingDown or
                PlmHeaderId.MotherBrainEscapeRoomGate or
                PlmHeaderId.MotherBrainEscapeRoomGateClosing or
                PlmHeaderId.Plm1x1RespawningCrumbleBlock or
                PlmHeaderId.Plm2x1RespawningCrumbleBlock or
                PlmHeaderId.Plm1x2RespawningCrumbleBlock or
                PlmHeaderId.Plm2x2RespawningCrumbleBlock or
                PlmHeaderId.BombReactionSpeedBoostBlock or
                PlmHeaderId.SpeedBlockBrinstarSlowRespawning or
                PlmHeaderId.SpeedBlockBrinstarSlowPermanent or
                PlmHeaderId.SpeedBlockRespawning or
                PlmHeaderId.SpeedBlockDachoraRespawning or
                PlmHeaderId.SpeedBlockPermanent or
                PlmHeaderId.ContactCrumble1x1Respawning or
                PlmHeaderId.ContactCrumble2x1Respawning or
                PlmHeaderId.ContactCrumble1x2Respawning or
                PlmHeaderId.ContactCrumble2x2Respawning or
                PlmHeaderId.ContactCrumble1x1Permanent or
                PlmHeaderId.ContactCrumble2x1Permanent or
                PlmHeaderId.ContactCrumble1x2Permanent or
                PlmHeaderId.ContactCrumble2x2Permanent or
                PlmHeaderId.Reaction1x1RespawningShotBlock or
                PlmHeaderId.Reaction2x1RespawningShotBlock or
                PlmHeaderId.Reaction1x2RespawningShotBlock or
                PlmHeaderId.Reaction2x2RespawningShotBlock or
                PlmHeaderId.Reaction1x1ShotBlock or
                PlmHeaderId.Reaction2x1ShotBlock or
                PlmHeaderId.Reaction1x2ShotBlock or
                PlmHeaderId.Reaction2x2ShotBlock or
                PlmHeaderId.ReactionRespawningPowerBombBlock or
                PlmHeaderId.ReactionPowerBombBlock or
                PlmHeaderId.ReactionRespawningSuperMissileBlock or
                PlmHeaderId.ReactionSuperMissileBlock or
                PlmHeaderId.EnemyBreakableBlock or
                PlmHeaderId.Collision1x1RespawningBombBlock or
                PlmHeaderId.Collision2x1RespawningBombBlock or
                PlmHeaderId.Collision1x2RespawningBombBlock or
                PlmHeaderId.Collision2x2RespawningBombBlock or
                PlmHeaderId.Collision1x1BombBlock or
                PlmHeaderId.Collision2x1BombBlock or
                PlmHeaderId.Collision1x2BombBlock or
                PlmHeaderId.Collision2x2BombBlock or
                PlmHeaderId.Reaction1x1RespawningBombBlock or
                PlmHeaderId.Reaction2x1RespawningBombBlock or
                PlmHeaderId.Reaction1x2RespawningBombBlock or
                PlmHeaderId.Reaction2x2RespawningBombBlock or
                PlmHeaderId.Reaction1x1BombBlock or
                PlmHeaderId.Reaction2x1BombBlock or
                PlmHeaderId.Reaction1x2BombBlock or
                PlmHeaderId.Reaction2x2BombBlock or
                PlmHeaderId.GrappledGrappleBlock or
                PlmHeaderId.GrappledRespawningBreakableGrappleBlock or
                PlmHeaderId.GrappledBreakableGrappleBlock or
                PlmHeaderId.GrappledGenericSpikeBlock or
                PlmHeaderId.GrappledDraygonsBrokenTurret or
                PlmHeaderId.UnusedBlueBrinstarFaceBlock or
                PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                PlmHeaderId.UnusedShotBlock or
                PlmHeaderId.UnusedGrappleBlock or
                PlmHeaderId.LowerNorfairChozoHand or
                PlmHeaderId.CollisionLowerNorfairChozoHandCheck or
                PlmHeaderId.MotherBrainGlass or
                PlmHeaderId.BombTorizoHand or
                PlmHeaderId.WreckedShipChozoHand or
                PlmHeaderId.CollisionWreckedShipChozoHandCheck or
                PlmHeaderId.ClearSlopeAccessForWreckedShipChozo or
                PlmHeaderId.BlockSlopeAccessForWreckedShipChozo or
                PlmHeaderId.NoobTube or
                PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                PlmHeaderId.EyeDoorEyeFacingRight or
                PlmHeaderId.EyeDoorFacingRight or
                PlmHeaderId.EyeDoorBottomFacingRight or
                PlmHeaderId.EyeDoorEyeFacingLeft or
                PlmHeaderId.EyeDoorFacingLeft or
                PlmHeaderId.EyeDoorBottomFacingLeft or
                PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed or
                PlmHeaderId.DraygonCannonFacingLeft or
                PlmHeaderId.DraygonCannonFacingLeftUnshielded or
                PlmHeaderId.ItemCollisionDetection or
                PlmHeaderId.ExposedEnergyTank or
                PlmHeaderId.ExposedMissileTank or
                PlmHeaderId.ExposedSuperMissileTank or
                PlmHeaderId.ExposedPowerBombTank or
                PlmHeaderId.ExposedBombs or
                PlmHeaderId.ExposedChargeBeam or
                PlmHeaderId.ExposedIceBeam or
                PlmHeaderId.ExposedHiJumpBoots or
                PlmHeaderId.ExposedSpeedBooster or
                PlmHeaderId.ExposedWaveBeam or
                PlmHeaderId.ExposedSpazerBeam or
                PlmHeaderId.ExposedSpringBall or
                PlmHeaderId.ExposedVariaSuit or
                PlmHeaderId.ExposedGravitySuit or
                PlmHeaderId.ExposedXrayScope or
                PlmHeaderId.ExposedPlasmaBeam or
                PlmHeaderId.ExposedGrappleBeam or
                PlmHeaderId.ExposedSpaceJump or
                PlmHeaderId.ExposedScrewAttack or
                PlmHeaderId.ExposedMorphBall or
                PlmHeaderId.ExposedReserveTank or
                PlmHeaderId.ChozoEnergyTank or
                PlmHeaderId.ChozoMissileTank or
                PlmHeaderId.ChozoSuperMissileTank or
                PlmHeaderId.ChozoPowerBombTank or
                PlmHeaderId.ChozoBombs or
                PlmHeaderId.ChozoChargeBeam or
                PlmHeaderId.ChozoIceBeam or
                PlmHeaderId.ChozoHiJumpBoots or
                PlmHeaderId.ChozoSpeedBooster or
                PlmHeaderId.ChozoWaveBeam or
                PlmHeaderId.ChozoSpazerBeam or
                PlmHeaderId.ChozoSpringBall or
                PlmHeaderId.ChozoVariaSuit or
                PlmHeaderId.ChozoGravitySuit or
                PlmHeaderId.ChozoXrayScope or
                PlmHeaderId.ChozoPlasmaBeam or
                PlmHeaderId.ChozoGrappleBeam or
                PlmHeaderId.ChozoSpaceJump or
                PlmHeaderId.ChozoScrewAttack or
                PlmHeaderId.ChozoMorphBall or
                PlmHeaderId.ChozoReserveTank or
                PlmHeaderId.ShotBlockEnergyTank or
                PlmHeaderId.ShotBlockMissileTank or
                PlmHeaderId.ShotBlockSuperMissileTank or
                PlmHeaderId.ShotBlockPowerBombTank or
                PlmHeaderId.ShotBlockBombs or
                PlmHeaderId.ShotBlockChargeBeam or
                PlmHeaderId.ShotBlockIceBeam or
                PlmHeaderId.ShotBlockHiJumpBoots or
                PlmHeaderId.ShotBlockSpeedBooster or
                PlmHeaderId.ShotBlockWaveBeam or
                PlmHeaderId.ShotBlockSpazerBeam or
                PlmHeaderId.ShotBlockSpringBall or
                PlmHeaderId.ShotBlockVariaSuit or
                PlmHeaderId.ShotBlockGravitySuit or
                PlmHeaderId.ShotBlockXrayScope or
                PlmHeaderId.ShotBlockPlasmaBeam or
                PlmHeaderId.ShotBlockGrappleBeam or
                PlmHeaderId.ShotBlockSpaceJump or
                PlmHeaderId.ShotBlockScrewAttack or
                PlmHeaderId.ShotBlockMorphBall or
                PlmHeaderId.ShotBlockReserveTank => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Mother Brain mutation header is outside $B673-$B6C7's authored set."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(header),
                header,
                "Mother Brain mutation header is outside $B673-$B6C7's authored set."),
        };

        // SpawnHardcodedPLM probes native slots $4E,$4C,...,$00. Do not reserve a boss
        // slot or coalesce adjacent row requests: their allocation order is observable.
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.BlockIndex = level.GetBlockIndex(block);
        slot.RestoreLevelWord = 0;
        slot.LoopTimer = 0;
        slot.StartAnimation(new MotherBrainFakeDeathPlmAnimation(program));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Header = header;
        if (header == PlmHeaderId.MotherBrainsRoomEscapeDoor)
        {
            // B5F8 installs the door-list index and three upward-linked extensions
            // synchronously, before the first draw instruction changes their artwork.
            PlmLevelWrites.WritePlmCollisionTypeAndBts(level, slot.BlockIndex, RoomCollisionType.DoorBlock,
                MotherBrainDeathDefinitions.EscapeDoorListIndex);
            for (int row = 1; row < 4; row++)
                PlmLevelWrites.WritePlmCollisionTypeAndBts(level, slot.BlockIndex + row * level.Size.X,
                    RoomCollisionType.VerticalExtension, unchecked((byte)MotherBrainDeathDefinitions.EscapeDoorExtensionOffset));
        }
        return true;
    }
}

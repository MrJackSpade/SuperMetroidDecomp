using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Scroll PLMs: setup, stepping and the scroll-cell mutation they apply when Samus touches them.</summary>
internal sealed class ScrollPlms : IPlmFamilySource
{
    /// <summary>Registers the Scroll family's step.</summary>
    public void AddPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(PlmFamily.Scroll, static (slot, context) =>
            TryStepScrollPlm(context.Level, context.Frame.Scrolls, slot) ? PlmStepOutcome.Consumed : PlmStepOutcome.Shared);
    }

    /// <summary>Native-order PLM slots searched for resident scroll triggers.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds scroll-trigger operations to the room's PLM slot table.</summary>
    internal ScrollPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>Every live cartridge-authored scroll trigger, by slot ordinal.</summary>
    public IReadOnlyList<ScrollPlmSnapshot> Snapshots => _slotTable.Slots
        .Where(slot => slot.Active && slot.Family is ScrollPlmState)
        .Select(slot => new ScrollPlmSnapshot(
            slot.BlockIndex))
        .ToArray();

    /// <summary>
    /// Applies the scroll trigger's touch to the already-resolved origin of its special-air trigger block.
    /// </summary>
    /// <param name="blockIndex">Zero-based row-major 16x16 room-block index of the trigger origin, after any scroll-extension traversal.</param>
    /// <returns>True when a live resident scroll PLM owns the block, including an already-triggered one; false when no matching owner exists.</returns>
    /// <remarks>The first touch latches the trigger and wakes its instruction list with a one-tick timer; repeated collision probes do not wake it again before ordered scroll writes finish.</remarks>
    public bool TryNotifyScrollTouch(int blockIndex)
    {
        foreach (PlmSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not ScrollPlmState scroll)
                continue;

            // The trigger latch prevents multiple collision probes in one frame from repeatedly advancing the sleeping
            // program.
            if (!scroll.Triggered)
            {
                scroll.Triggered = true;
                slot.InstructionTimer = WrappingCountdown.Start(1);
            }
            return true;
        }
        return false;
    }

    /// <summary>Runs ordered scroll writes after a trigger wakes, or preserves its dormant sleep.</summary>
    internal static bool TryStepScrollPlm(
        RoomLevelData level,
        RoomScrollGrid? scrolls,
        PlmSlot slot)
    {
        if (slot.Family is not ScrollPlmState scroll)
            return false;
        if (!scroll.Triggered)
            return true; // $86B4 sleep leaves this resident trigger dormant.
        if (scrolls is null)
            throw new InvalidOperationException("A triggered scroll PLM requires the active scroll grid.");

        // A retail trigger runs its program's ordered writes; a constructed one its validated pairs.
        if (scroll.Program is null)
        {
            RoomPlmScrollPrograms.Apply(scroll.CompiledProgram ?? throw new InvalidOperationException(
                "A scroll trigger has neither a retail program nor constructed pairs."), scrolls.SetStorage);
            FinishScrollMutation(level, slot);
            return true;
        }
        ReadOnlyMemory<byte> program = scroll.Program;
        for (int pairIndex = 0; pairIndex < RoomScrollGrid.StorageByteCount; pairIndex++)
        {
            int offset = pairIndex * 2;
            if (offset >= program.Length)
                throw new InvalidDataException(
                    $"Scroll PLM program at block {slot.BlockIndex} ended without a terminator.");
            int scrollIndex = program.Span[offset];
            if (unchecked((sbyte)scrollIndex) < 0)
            {
                FinishScrollMutation(level, slot);
                return true;
            }

            if (offset + 1 >= program.Length)
                throw new InvalidDataException(
                    $"Scroll PLM program at block {slot.BlockIndex} lacks a state byte.");
            scrolls.SetStorage(
                scrollIndex,
                RoomScrollStates.FromAuthoredByte(
                    program.Span[offset + 1],
                    $"scroll PLM program at block {slot.BlockIndex} pair {pairIndex}"));
        }

        throw new InvalidDataException(
            $"Scroll PLM data at block {slot.BlockIndex} has no negative terminator.");
    }

    /// <summary>Restores the special-air trigger and rearms its waiting program.</summary>
    private static void FinishScrollMutation(RoomLevelData level, PlmSlot slot)
    {
        // The finishing instruction clears the trigger latch and restores special air, then the program loops to
        // sleep. The scroll-trigger behaviour remains unchanged and can wake it again.
        ((slot.Family as ScrollPlmState) ?? throw new InvalidOperationException("A scroll PLM slot owns scroll trigger state.")).Triggered = false;
        level.SetTerrain(
            slot.BlockIndex,
            level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ScrollTrigger);
    }

    /// <summary>Runs one scroll/extension setup after the shared allocator chose its ID.</summary>
    internal static void SetupScrollSlot(RoomLevelData level, PlmSlot slot,
        PlmHeaderId header, ReadOnlySpan<byte> program, RoomScrollProgram? compiledProgram)
    {
        if (header != PlmHeaderId.ScrollTrigger)
        {
            (RoomCollisionType collisionType, int offset) = header switch
            {
                PlmHeaderId.RightwardsScrollExtension => new ScrollExtension(RoomCollisionType.HorizontalExtension, -1),
                PlmHeaderId.LeftwardsScrollExtension => new ScrollExtension(RoomCollisionType.HorizontalExtension, 1),
                PlmHeaderId.DownwardsScrollExtension => new ScrollExtension(RoomCollisionType.VerticalExtension, -1),
                PlmHeaderId.UpwardsScrollExtension => new ScrollExtension(RoomCollisionType.VerticalExtension, 1),
                PlmHeaderId.None or
                    PlmHeaderId.Nothing or
                    PlmHeaderId.CollisionReactionClearCarry or
                    PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
                    PlmHeaderId.WreckedShipEntranceTreadmillFromEast or
                    PlmHeaderId.InsideReactionNothingFirst or
                    PlmHeaderId.InsideReactionNothingSecond or
                    PlmHeaderId.InsideReactionNothingThird or
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
                    PlmHeaderId.NothingSecond or
                    PlmHeaderId.NothingThird or
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
                    PlmHeaderId.ShotBlockReserveTank => throw new InvalidOperationException(
                    "Validated scroll extension escaped its setup table."),
                _ => throw new InvalidOperationException(
                    "Validated scroll extension escaped its setup table."),
            };
            level.SetTerrain(
                slot.BlockIndex,
                level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(collisionType));
            level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.OfExtensionOffset(offset));

            // Every extension setup ends in DeletePLM. Clearing this preallocated slot is
            // what permits the following ROM record to reuse the same highest native ID.
            slot.Clear();
            return;
        }

        slot.Family = new ScrollPlmState
        {
            Program = compiledProgram.HasValue ? null : program.ToArray(),
            CompiledProgram = compiledProgram,
        };
        level.SetTerrain(
            slot.BlockIndex,
            level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.ScrollTrigger);
    }

    /// <summary>Per-PLM scroll trigger latch and its bound retail or constructed write program.</summary>
    internal sealed class ScrollPlmState : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.Scroll;

        /// <summary>Latched on first touch until ordered scroll writes finish.</summary>
        public bool Triggered { get; set; }
        /// <summary>The retail program the trigger runs; independent of later room-argument edits.</summary>
        public RoomScrollProgram? CompiledProgram { get; set; }
        /// <summary>Validated constructed pairs. Null runs the retail program.</summary>
        public byte[]? Program { get; set; }
    }
}

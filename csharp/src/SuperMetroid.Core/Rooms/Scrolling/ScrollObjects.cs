using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>Scroll PLMs: setup, stepping and the scroll-cell mutation they apply when Samus touches them.</summary>
internal sealed class ScrollObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the Scroll family's step.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.Scroll, static (slot, context) =>
            TryStepScrollObject(context.Level, context.Frame.Scrolls, slot) ? RoomObjectStepOutcome.Consumed : RoomObjectStepOutcome.Shared);
    }

    /// <summary>PLM slots, in the game's search order, searched for resident scroll triggers.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds scroll-trigger operations to the room's PLM slot table.</summary>
    internal ScrollObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>Every live room-authored scroll trigger, by slot ordinal.</summary>
    public IReadOnlyList<ScrollObjectSnapshot> Snapshots => _slotTable.Slots
        .Where(slot => slot.Active && slot.Family is ScrollObjectState)
        .Select(slot => new ScrollObjectSnapshot(
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
        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not ScrollObjectState scroll)
                continue;

            // The trigger latch prevents multiple collision probes in one frame from repeatedly advancing the sleeping
            // program.
            if (!scroll.Triggered)
            {
                scroll.Triggered = true;
                slot.FrameHold = 1;
            }
            return true;
        }
        return false;
    }

    /// <summary>Runs ordered scroll writes after a trigger wakes, or preserves its dormant sleep.</summary>
    internal static bool TryStepScrollObject(
        RoomLevelData level,
        RoomScrollGrid? scrolls,
        RoomObjectSlot slot)
    {
        if (slot.Family is not ScrollObjectState scroll)
            return false;
        if (!scroll.Triggered)
            return true; // The sleep leaves this resident trigger dormant.
        if (scrolls is null)
            throw new InvalidOperationException("A triggered scroll PLM requires the active scroll grid.");

        // A constructed trigger runs its validated writes, a retail one its program's ordered writes; either program
        // ends after its last write.
        if (scroll.Program is null)
        {
            RoomObjectScrollPrograms.Apply(scroll.CompiledProgram ?? throw new InvalidOperationException(
                "A scroll trigger has neither a retail program nor constructed writes."), scrolls.SetStorage);
        }
        else
        {
            foreach (RoomScrollWrite write in scroll.Program)
                scrolls.SetStorage(write.Index, write.State);
        }
        FinishScrollMutation(level, slot);
        return true;
    }

    /// <summary>Restores the special-air trigger and rearms its waiting program.</summary>
    private static void FinishScrollMutation(RoomLevelData level, RoomObjectSlot slot)
    {
        // The finishing instruction clears the trigger latch and restores special air, then the program loops to
        // sleep. The scroll-trigger behaviour remains unchanged and can wake it again.
        ((slot.Family as ScrollObjectState) ?? throw new InvalidOperationException("A scroll PLM slot owns scroll trigger state.")).Triggered = false;
        level.SetTerrain(
            slot.BlockIndex,
            level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger));
    }

    /// <summary>Runs one scroll/extension setup after the shared allocator chose its ID.</summary>
    internal static void SetupScrollSlot(RoomLevelData level, RoomObjectSlot slot,
        RoomObjectHeaderId header, IReadOnlyList<RoomScrollWrite>? program, RoomScrollProgram? compiledProgram)
    {
        if (header != RoomObjectHeaderId.ScrollTrigger)
        {
            (RoomCollisionType collisionType, int offset) = header switch
            {
                RoomObjectHeaderId.RightwardsScrollExtension => new ScrollExtension(RoomCollisionType.HorizontalExtension, -1),
                RoomObjectHeaderId.LeftwardsScrollExtension => new ScrollExtension(RoomCollisionType.HorizontalExtension, 1),
                RoomObjectHeaderId.DownwardsScrollExtension => new ScrollExtension(RoomCollisionType.VerticalExtension, -1),
                RoomObjectHeaderId.UpwardsScrollExtension => new ScrollExtension(RoomCollisionType.VerticalExtension, 1),
                RoomObjectHeaderId.None or
                    RoomObjectHeaderId.Nothing or
                    RoomObjectHeaderId.CollisionReactionClearCarry or
                    RoomObjectHeaderId.WreckedShipEntranceTreadmillFromWest or
                    RoomObjectHeaderId.WreckedShipEntranceTreadmillFromEast or
                    RoomObjectHeaderId.InsideReactionNothingFirst or
                    RoomObjectHeaderId.InsideReactionNothingSecond or
                    RoomObjectHeaderId.InsideReactionNothingThird or
                    RoomObjectHeaderId.FillMotherBrainsWall or
                    RoomObjectHeaderId.MotherBrainsRoomEscapeDoor or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow2 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow3 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow4 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow5 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow6 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow7 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow8 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow9 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowA or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowB or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowC or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowD or
                    RoomObjectHeaderId.ClearMotherBrainCeilingBlock or
                    RoomObjectHeaderId.ClearMotherBrainCeilingTube or
                    RoomObjectHeaderId.ClearMotherBrainBottomMiddleSideTube or
                    RoomObjectHeaderId.ClearMotherBrainBottomMiddleTubes or
                    RoomObjectHeaderId.ClearMotherBrainBottomLeftTube or
                    RoomObjectHeaderId.ClearMotherBrainBottomRightTube or
                    RoomObjectHeaderId.InsideReactionBrinstarFloorPlant or
                    RoomObjectHeaderId.InsideReactionBrinstarCeilingPlant or
                    RoomObjectHeaderId.MapStation or
                    RoomObjectHeaderId.MapStationRightAccess or
                    RoomObjectHeaderId.MapStationLeftAccess or
                    RoomObjectHeaderId.EnergyStation or
                    RoomObjectHeaderId.EnergyStationRightAccess or
                    RoomObjectHeaderId.EnergyStationLeftAccess or
                    RoomObjectHeaderId.MissileStation or
                    RoomObjectHeaderId.MissileStationRightAccess or
                    RoomObjectHeaderId.MissileStationLeftAccess or
                    RoomObjectHeaderId.NothingSecond or
                    RoomObjectHeaderId.NothingThird or
                    RoomObjectHeaderId.ScrollTriggerCollision or
                    RoomObjectHeaderId.ScrollTrigger or
                    RoomObjectHeaderId.UnusedSolidScrollPLM or
                    RoomObjectHeaderId.ElevatorPlatform or
                    RoomObjectHeaderId.InsideReactionCrateria80 or
                    RoomObjectHeaderId.InsideReactionQuicksandSurface or
                    RoomObjectHeaderId.InsideReactionSubmergingQuicksand or
                    RoomObjectHeaderId.InsideReactionSandFallsSlow or
                    RoomObjectHeaderId.InsideReactionSandFallsFast or
                    RoomObjectHeaderId.CollisionReactionQuicksandSurface or
                    RoomObjectHeaderId.CollisionReactionSubmergingQuicksand or
                    RoomObjectHeaderId.CollisionReactionSandFallsSlow or
                    RoomObjectHeaderId.CollisionReactionSandFallsFast or
                    RoomObjectHeaderId.ClearCrocomireBridge or
                    RoomObjectHeaderId.CrumbleCrocomireBridgeBlock or
                    RoomObjectHeaderId.ClearCrocomireBridgeBlock or
                    RoomObjectHeaderId.ClearCrocomireInvisibleWall or
                    RoomObjectHeaderId.CreateCrocomireInvisibleWall or
                    RoomObjectHeaderId.UnusedDraw13BlankAirTiles or
                    RoomObjectHeaderId.UnusedDraw13BlankSolidTiles or
                    RoomObjectHeaderId.ClearBabyMetroidInvisibleWall or
                    RoomObjectHeaderId.CreateBabyMetroidInvisibleWall or
                    RoomObjectHeaderId.SaveStationTrigger or
                    RoomObjectHeaderId.SaveStation or
                    RoomObjectHeaderId.CrumbleAccessToTourianElevator or
                    RoomObjectHeaderId.ClearAccessToTourianElevator or
                    RoomObjectHeaderId.DrawPhantoonDoorDuringBossFight or
                    RoomObjectHeaderId.RestorePhantoonDoorAfterBossFight or
                    RoomObjectHeaderId.CrumbleSporeSpawnCeiling or
                    RoomObjectHeaderId.ClearSporeSpawnCeiling or
                    RoomObjectHeaderId.ClearBotwoonWall or
                    RoomObjectHeaderId.CrumbleBotwoonWall or
                    RoomObjectHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                    RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground1 or
                    RoomObjectHeaderId.CrumbleKraidPlatformVariant1 or
                    RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground2 or
                    RoomObjectHeaderId.CrumbleKraidPlatformVariant2 or
                    RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground3 or
                    RoomObjectHeaderId.ClearKraidCeiling or
                    RoomObjectHeaderId.ClearKraidSpikes or
                    RoomObjectHeaderId.CrumbleKraidSpikes or
                    RoomObjectHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                    RoomObjectHeaderId.SpeedBoosterEscape or
                    RoomObjectHeaderId.ShaktoolsRoom or
                    RoomObjectHeaderId.MaridiaElevatube or
                    RoomObjectHeaderId.OldTourianEscapeShaftFakeWall or
                    RoomObjectHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                    RoomObjectHeaderId.GateBlock or
                    RoomObjectHeaderId.ReactionCrittersEscapeBlock or
                    RoomObjectHeaderId.CrittersEscapeBlock or
                    RoomObjectHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                    RoomObjectHeaderId.BombTorizoGreyDoor or
                    RoomObjectHeaderId.WreckedShipAttic or
                    RoomObjectHeaderId.CrateriaMainstreetEscapePassage or
                    RoomObjectHeaderId.LeftGreenGateTrigger or
                    RoomObjectHeaderId.RightGreenGateTrigger or
                    RoomObjectHeaderId.LeftRedGateTrigger or
                    RoomObjectHeaderId.RightRedGateTrigger or
                    RoomObjectHeaderId.LeftBlueGateTrigger or
                    RoomObjectHeaderId.RightBlueGateTrigger or
                    RoomObjectHeaderId.LeftYellowGateTrigger or
                    RoomObjectHeaderId.RightYellowGateTrigger or
                    RoomObjectHeaderId.DownwardsOpenGate or
                    RoomObjectHeaderId.DownwardGate or
                    RoomObjectHeaderId.UpwardsOpenGate or
                    RoomObjectHeaderId.UpwardsClosedGate or
                    RoomObjectHeaderId.DownwardGateShotBlock or
                    RoomObjectHeaderId.UpwardsGateShotblock or
                    RoomObjectHeaderId.GenericShotTriggerForAPLM or
                    RoomObjectHeaderId.GreyDoorFacingLeft or
                    RoomObjectHeaderId.GreyDoorFacingRight or
                    RoomObjectHeaderId.GreyDoorFacingUp or
                    RoomObjectHeaderId.GreyDoorFacingDown or
                    RoomObjectHeaderId.YellowDoorFacingLeft or
                    RoomObjectHeaderId.YellowDoorFacingRight or
                    RoomObjectHeaderId.YellowDoorFacingUp or
                    RoomObjectHeaderId.YellowDoorFacingDown or
                    RoomObjectHeaderId.GreenDoorFacingLeft or
                    RoomObjectHeaderId.GreenDoorFacingRight or
                    RoomObjectHeaderId.GreenDoorFacingUp or
                    RoomObjectHeaderId.GreenDoorFacingDown or
                    RoomObjectHeaderId.RedDoorFacingLeft or
                    RoomObjectHeaderId.RedDoorFacingRight or
                    RoomObjectHeaderId.RedDoorFacingUp or
                    RoomObjectHeaderId.RedDoorFacingDown or
                    RoomObjectHeaderId.BlueDoorFacingLeft or
                    RoomObjectHeaderId.BlueDoorFacingRight or
                    RoomObjectHeaderId.BlueDoorFacingUp or
                    RoomObjectHeaderId.BlueDoorFacingDown or
                    RoomObjectHeaderId.BlueDoorClosingFacingLeft or
                    RoomObjectHeaderId.BlueDoorClosingFacingRight or
                    RoomObjectHeaderId.BlueDoorClosingFacingUp or
                    RoomObjectHeaderId.BlueDoorClosingFacingDown or
                    RoomObjectHeaderId.MotherBrainEscapeRoomGate or
                    RoomObjectHeaderId.MotherBrainEscapeRoomGateClosing or
                    RoomObjectHeaderId.RoomObject1x1RespawningCrumbleBlock or
                    RoomObjectHeaderId.RoomObject2x1RespawningCrumbleBlock or
                    RoomObjectHeaderId.RoomObject1x2RespawningCrumbleBlock or
                    RoomObjectHeaderId.RoomObject2x2RespawningCrumbleBlock or
                    RoomObjectHeaderId.BombReactionSpeedBoostBlock or
                    RoomObjectHeaderId.SpeedBlockBrinstarSlowRespawning or
                    RoomObjectHeaderId.SpeedBlockBrinstarSlowPermanent or
                    RoomObjectHeaderId.SpeedBlockRespawning or
                    RoomObjectHeaderId.SpeedBlockDachoraRespawning or
                    RoomObjectHeaderId.SpeedBlockPermanent or
                    RoomObjectHeaderId.ContactCrumble1x1Respawning or
                    RoomObjectHeaderId.ContactCrumble2x1Respawning or
                    RoomObjectHeaderId.ContactCrumble1x2Respawning or
                    RoomObjectHeaderId.ContactCrumble2x2Respawning or
                    RoomObjectHeaderId.ContactCrumble1x1Permanent or
                    RoomObjectHeaderId.ContactCrumble2x1Permanent or
                    RoomObjectHeaderId.ContactCrumble1x2Permanent or
                    RoomObjectHeaderId.ContactCrumble2x2Permanent or
                    RoomObjectHeaderId.Reaction1x1RespawningShotBlock or
                    RoomObjectHeaderId.Reaction2x1RespawningShotBlock or
                    RoomObjectHeaderId.Reaction1x2RespawningShotBlock or
                    RoomObjectHeaderId.Reaction2x2RespawningShotBlock or
                    RoomObjectHeaderId.Reaction1x1ShotBlock or
                    RoomObjectHeaderId.Reaction2x1ShotBlock or
                    RoomObjectHeaderId.Reaction1x2ShotBlock or
                    RoomObjectHeaderId.Reaction2x2ShotBlock or
                    RoomObjectHeaderId.ReactionRespawningPowerBombBlock or
                    RoomObjectHeaderId.ReactionPowerBombBlock or
                    RoomObjectHeaderId.ReactionRespawningSuperMissileBlock or
                    RoomObjectHeaderId.ReactionSuperMissileBlock or
                    RoomObjectHeaderId.EnemyBreakableBlock or
                    RoomObjectHeaderId.Collision1x1RespawningBombBlock or
                    RoomObjectHeaderId.Collision2x1RespawningBombBlock or
                    RoomObjectHeaderId.Collision1x2RespawningBombBlock or
                    RoomObjectHeaderId.Collision2x2RespawningBombBlock or
                    RoomObjectHeaderId.Collision1x1BombBlock or
                    RoomObjectHeaderId.Collision2x1BombBlock or
                    RoomObjectHeaderId.Collision1x2BombBlock or
                    RoomObjectHeaderId.Collision2x2BombBlock or
                    RoomObjectHeaderId.Reaction1x1RespawningBombBlock or
                    RoomObjectHeaderId.Reaction2x1RespawningBombBlock or
                    RoomObjectHeaderId.Reaction1x2RespawningBombBlock or
                    RoomObjectHeaderId.Reaction2x2RespawningBombBlock or
                    RoomObjectHeaderId.Reaction1x1BombBlock or
                    RoomObjectHeaderId.Reaction2x1BombBlock or
                    RoomObjectHeaderId.Reaction1x2BombBlock or
                    RoomObjectHeaderId.Reaction2x2BombBlock or
                    RoomObjectHeaderId.GrappledGrappleBlock or
                    RoomObjectHeaderId.GrappledRespawningBreakableGrappleBlock or
                    RoomObjectHeaderId.GrappledBreakableGrappleBlock or
                    RoomObjectHeaderId.GrappledGenericSpikeBlock or
                    RoomObjectHeaderId.GrappledDraygonsBrokenTurret or
                    RoomObjectHeaderId.UnusedBlueBrinstarFaceBlock or
                    RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                    RoomObjectHeaderId.UnusedShotBlock or
                    RoomObjectHeaderId.UnusedGrappleBlock or
                    RoomObjectHeaderId.LowerNorfairChozoHand or
                    RoomObjectHeaderId.CollisionLowerNorfairChozoHandCheck or
                    RoomObjectHeaderId.MotherBrainGlass or
                    RoomObjectHeaderId.BombTorizoHand or
                    RoomObjectHeaderId.WreckedShipChozoHand or
                    RoomObjectHeaderId.CollisionWreckedShipChozoHandCheck or
                    RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo or
                    RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo or
                    RoomObjectHeaderId.NoobTube or
                    RoomObjectHeaderId.SetMetroidsClearedStatesWhenRequired or
                    RoomObjectHeaderId.EyeDoorEyeFacingRight or
                    RoomObjectHeaderId.EyeDoorFacingRight or
                    RoomObjectHeaderId.EyeDoorBottomFacingRight or
                    RoomObjectHeaderId.EyeDoorEyeFacingLeft or
                    RoomObjectHeaderId.EyeDoorFacingLeft or
                    RoomObjectHeaderId.EyeDoorBottomFacingLeft or
                    RoomObjectHeaderId.DraygonCannonFacingRight or
                    RoomObjectHeaderId.DraygonCannonFacingRightDestroyed or
                    RoomObjectHeaderId.DraygonCannonFacingLeft or
                    RoomObjectHeaderId.DraygonCannonFacingLeftUnshielded or
                    RoomObjectHeaderId.ItemCollisionDetection or
                    RoomObjectHeaderId.ExposedEnergyTank or
                    RoomObjectHeaderId.ExposedMissileTank or
                    RoomObjectHeaderId.ExposedSuperMissileTank or
                    RoomObjectHeaderId.ExposedPowerBombTank or
                    RoomObjectHeaderId.ExposedBombs or
                    RoomObjectHeaderId.ExposedChargeBeam or
                    RoomObjectHeaderId.ExposedIceBeam or
                    RoomObjectHeaderId.ExposedHiJumpBoots or
                    RoomObjectHeaderId.ExposedSpeedBooster or
                    RoomObjectHeaderId.ExposedWaveBeam or
                    RoomObjectHeaderId.ExposedSpazerBeam or
                    RoomObjectHeaderId.ExposedSpringBall or
                    RoomObjectHeaderId.ExposedVariaSuit or
                    RoomObjectHeaderId.ExposedGravitySuit or
                    RoomObjectHeaderId.ExposedXrayScope or
                    RoomObjectHeaderId.ExposedPlasmaBeam or
                    RoomObjectHeaderId.ExposedGrappleBeam or
                    RoomObjectHeaderId.ExposedSpaceJump or
                    RoomObjectHeaderId.ExposedScrewAttack or
                    RoomObjectHeaderId.ExposedMorphBall or
                    RoomObjectHeaderId.ExposedReserveTank or
                    RoomObjectHeaderId.ChozoEnergyTank or
                    RoomObjectHeaderId.ChozoMissileTank or
                    RoomObjectHeaderId.ChozoSuperMissileTank or
                    RoomObjectHeaderId.ChozoPowerBombTank or
                    RoomObjectHeaderId.ChozoBombs or
                    RoomObjectHeaderId.ChozoChargeBeam or
                    RoomObjectHeaderId.ChozoIceBeam or
                    RoomObjectHeaderId.ChozoHiJumpBoots or
                    RoomObjectHeaderId.ChozoSpeedBooster or
                    RoomObjectHeaderId.ChozoWaveBeam or
                    RoomObjectHeaderId.ChozoSpazerBeam or
                    RoomObjectHeaderId.ChozoSpringBall or
                    RoomObjectHeaderId.ChozoVariaSuit or
                    RoomObjectHeaderId.ChozoGravitySuit or
                    RoomObjectHeaderId.ChozoXrayScope or
                    RoomObjectHeaderId.ChozoPlasmaBeam or
                    RoomObjectHeaderId.ChozoGrappleBeam or
                    RoomObjectHeaderId.ChozoSpaceJump or
                    RoomObjectHeaderId.ChozoScrewAttack or
                    RoomObjectHeaderId.ChozoMorphBall or
                    RoomObjectHeaderId.ChozoReserveTank or
                    RoomObjectHeaderId.ShotBlockEnergyTank or
                    RoomObjectHeaderId.ShotBlockMissileTank or
                    RoomObjectHeaderId.ShotBlockSuperMissileTank or
                    RoomObjectHeaderId.ShotBlockPowerBombTank or
                    RoomObjectHeaderId.ShotBlockBombs or
                    RoomObjectHeaderId.ShotBlockChargeBeam or
                    RoomObjectHeaderId.ShotBlockIceBeam or
                    RoomObjectHeaderId.ShotBlockHiJumpBoots or
                    RoomObjectHeaderId.ShotBlockSpeedBooster or
                    RoomObjectHeaderId.ShotBlockWaveBeam or
                    RoomObjectHeaderId.ShotBlockSpazerBeam or
                    RoomObjectHeaderId.ShotBlockSpringBall or
                    RoomObjectHeaderId.ShotBlockVariaSuit or
                    RoomObjectHeaderId.ShotBlockGravitySuit or
                    RoomObjectHeaderId.ShotBlockXrayScope or
                    RoomObjectHeaderId.ShotBlockPlasmaBeam or
                    RoomObjectHeaderId.ShotBlockGrappleBeam or
                    RoomObjectHeaderId.ShotBlockSpaceJump or
                    RoomObjectHeaderId.ShotBlockScrewAttack or
                    RoomObjectHeaderId.ShotBlockMorphBall or
                    RoomObjectHeaderId.ShotBlockReserveTank => throw new InvalidOperationException(
                    "Validated scroll extension escaped its setup table."),
                _ => throw new InvalidOperationException(
                    "Validated scroll extension escaped its setup table."),
            };
            level.SetTerrain(
                slot.BlockIndex,
                level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(collisionType));
            level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Extending(offset));

            // Every extension setup ends in DeletePLM. Clearing this preallocated slot is
            // what permits the following authored record to reuse the same highest ID.
            slot.Clear();
            return;
        }

        slot.Family = new ScrollObjectState
        {
            Program = compiledProgram.HasValue ? null : program?.ToArray() ?? throw new InvalidOperationException(
                "A scroll trigger has neither a retail program nor constructed writes."),
            CompiledProgram = compiledProgram,
        };
        level.SetTerrain(
            slot.BlockIndex,
            level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.WithCollision(RoomCollisionType.SpecialAir));
        level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger));
    }

    /// <summary>Per-PLM scroll trigger latch and its bound retail or constructed write program.</summary>
    internal sealed class ScrollObjectState : RoomObjectFamilyState
    {
        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.Scroll;

        /// <summary>Latched on first touch until ordered scroll writes finish.</summary>
        public bool Triggered { get; set; }
        /// <summary>The retail program the trigger runs; independent of later room-argument edits.</summary>
        public RoomScrollProgram? CompiledProgram { get; set; }
        /// <summary>Validated constructed writes, in order. Null runs the retail program.</summary>
        public RoomScrollWrite[]? Program { get; set; }
    }
}

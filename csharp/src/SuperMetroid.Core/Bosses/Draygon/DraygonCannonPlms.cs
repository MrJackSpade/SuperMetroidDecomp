using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Draygon's wall-turret PLMs: header identification and the cannon state Draygon's grab fight drives.</summary>
[RoomLifetime]
internal sealed class DraygonCannonPlms : IPlmFamilySource
{
    /// <summary>Registers the DraygonCannon family, which has no step of its own: its slots take the shared path.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.DraygonCannon, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomPlmEnemyEffects? _enemies;

    /// <summary>Connects cannon PLM instruction handling to the active room's enemy services.</summary>
    internal DraygonCannonPlms(IRoomPlmEnemyEffects? enemies)
    {
        _enemies = enemies;
    }

    /// <summary>Recognizes the active right, pre-destroyed right, and left retail cannon headers.</summary>
    internal static bool IsDraygonCannonHeader(PlmHeaderId header) => header is
        PlmHeaderId.DraygonCannonFacingRight or
        PlmHeaderId.DraygonCannonFacingRightDestroyed or
        PlmHeaderId.DraygonCannonFacingLeft;

    /// <summary>Captures cannon orientation and its enemy control word, installing shootable terrain unless the cannon starts destroyed.</summary>
    internal static void SetupDraygonCannonSlot(RoomLevelData level, PlmSlot slot, DraygonCannonControlWord controlWord)
    {
        DraygonCannonOrientation orientation = slot.Header switch
        {
            PlmHeaderId.DraygonCannonFacingRight or
                PlmHeaderId.DraygonCannonFacingRightDestroyed =>
                    DraygonCannonOrientation.Right,
            PlmHeaderId.DraygonCannonFacingLeft => DraygonCannonOrientation.Left,
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
                nameof(slot), slot.Header, "Not a retail Draygon cannon header."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.Header, "Not a retail Draygon cannon header."),
        };

        slot.Family = new DraygonCannonPlmState(controlWord);
        if (slot.Header == PlmHeaderId.DraygonCannonFacingRightDestroyed)
        {
            // Setup $DF4C only moves the original argument to PLM_Variable and seeds three.
            // Its list begins at the damage instruction, which owns the terrain mutation.
            slot.RoomArgument = DraygonCannonDefinitions.HitsToDestroy;
            slot.StartAnimation(new DraygonCannonPlmAnimation(DraygonCannonPlmProgramDefinitions.DestroyProgram(orientation)));
            return;
        }

        slot.RoomArgument = 0;
        slot.StartAnimation(new DraygonCannonPlmAnimation(DraygonCannonPlmProgramDefinitions.StartProgram(orientation)));
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonDefinitions.LiveCannon);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonDefinitions.LiveCannonExtension);
    }

    /// <summary>Accepts pending missile hits, treating a Super Missile as enough damage to reach the destruction threshold.</summary>
    internal static void RunDraygonCannonPreInstruction(PlmSlot slot)
    {
        DraygonCannonPlmState state = slot.Family as DraygonCannonPlmState
            ?? throw new InvalidOperationException("A Draygon cannon's hit ran without its cannon state.");
        if (!state.HasPendingHit)
            return;

        SamusProjectileFamily family = new SamusProjectileTypeWord(checked((ushort)slot.LoopTimer)).Family;
        if (family == SamusProjectileFamily.SuperMissile)
            slot.RoomArgument = DraygonCannonDefinitions.SuperMissileHitCounterSeed;
        else if (family != SamusProjectileFamily.Missile)
            return;

        slot.LoopTimer = 0;
        state.HasPendingHit = false;
        slot.WakeAtLink();
    }

    /// <summary>
    /// <c>$84:8ACD</c> on the cannon's hit counter: counts a hit and reports whether it destroys the cannon. The
    /// destroying hit marks the counter spent and removes the missile wake.
    /// </summary>
    internal static bool CountHit(PlmSlot slot)
    {
        byte next = unchecked((byte)(slot.RoomArgument + 1));
        if (next < DraygonCannonDefinitions.HitsToDestroy)
        {
            slot.RoomArgument = next;
            return false;
        }
        slot.RoomArgument = ushort.MaxValue;
        slot.PreInstruction = PlmPreInstructions.Inert;
        return true;
    }

    /// <summary>Disables the matching bank-$A5 cannon firing word and changes both room blocks to destroyed terrain.</summary>
    /// <summary>Destroys the cannon: disables its bank-$A5 firing word and writes its destroyed terrain.</summary>
    internal void DamageDraygonCannon(
        RoomLevelData level,
        PlmSlot slot)
    {
        DraygonCannonPlmState state = (slot.Family as DraygonCannonPlmState)
            ?? throw new InvalidOperationException("Draygon cannon damage reached a slot without cannon state.");
        (_enemies ?? throw new InvalidOperationException(
            "Draygon cannon bytecode has no enemy-control-word writer.")).DisableDraygonCannon(
                state.ControlWord);
        WriteDraygonCannonTypeAndBts(
            level,
            slot.BlockIndex,
            DraygonCannonDefinitions.DestroyedCannon);
        WriteDraygonCannonTypeAndBts(
            level,
            checked(slot.BlockIndex + level.Size.X),
            DraygonCannonDefinitions.DestroyedCannon);
        state.Destroyed = true;
    }

    /// <summary>Changes a cannon block's collision type and behavior while preserving its visual tile bits.</summary>
    private static void WriteDraygonCannonTypeAndBts(
        RoomLevelData level,
        int blockIndex,
        DraygonCannonTerrain terrain)
    {
        ushort original = level.GetPlmCollisionBlockByIndex(blockIndex).PackedWord.Raw;
        level.SetPlmForegroundEntry(
            blockIndex,
            new RoomLevelWord(original).WithCollisionType(terrain.Type).Raw);
        level.SetPlmBehavior(blockIndex, terrain.Behavior);
    }

    /// <summary>Per-slot cannon enemy control-word identity, destruction state, and queued hit flag.</summary>
    internal sealed class DraygonCannonPlmState(
        DraygonCannonControlWord controlWord) : PlmFamilyState, IShotTriggeredPlm
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.DraygonCannon;

        /// <summary>The cannon whose firing Draygon stops when this physical cannon is destroyed, decoded from the room's PLM argument at setup.</summary>
        public DraygonCannonControlWord ControlWord { get; } = controlWord;
        /// <summary>Marks that this cannon's terrain and firing control have been disabled.</summary>
        public bool Destroyed { get; set; }
        /// <summary>Indicates a projectile collision awaiting missile-family filtering.</summary>
        public bool HasPendingHit { get; set; }

        /// <inheritdoc/>
        public void RecordShot() => HasPendingHit = true;
    }
}

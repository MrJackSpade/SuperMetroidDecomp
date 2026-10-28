using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Save, refill, map and recharge station PLMs: activation, lockout, collision notification and their draws.</summary>
[RoomLifetime]
internal sealed class StationPlms : IPlmFamilySource
{
    /// <summary>Registers the Station family's step.</summary>
    public void AddPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(PlmFamily.Station, (slot, context) =>
            TryStepStation(context.Level, slot) ? PlmStepOutcome.Consumed : PlmStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Applies the station and access-arm draw lists to the room tilemap.</summary>
    private readonly IPlmDrawing _drawing;
    /// <summary>Locates resident station PLMs when collision or frontend confirmation arrives.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Queues extension, retraction, and save-animation sounds.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Connects station activation to room state, PLM drawing, resident slots, and sound requests.</summary>
    internal StationPlms(SharedGameState? system, IRoomSamusAccess? samus, IPlmDrawing drawing, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _drawing = drawing;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Frames spent extending or retracting the station access arm.</summary>
    private const ushort StationAccessMovementFrames = 6;
    /// <summary>Native hold before a fully extended arm publishes station activation.</summary>
    private const ushort StationAccessExtendedHoldFrames = 96;
    /// <summary>Messages and frontend actions produced in the current PLM handler pass.</summary>
    private readonly List<StationActivationEvent> _stationActivationEvents = [];
    /// <summary>Blocks repeated save confirmation during the current room entry.</summary>
    internal bool SaveStationLockedOut { get; private set; }

    /// <summary>
    /// The saving sound <c>$84:AFF4</c> queued when the save was accepted, until the frontend publishes it. The
    /// acceptance runs at the message-close boundary, which completes no gameplay audio publication, so the sound
    /// cannot ride the PLM sound list: the next PLM pass would clear it unheard.
    /// </summary>
    private SoundRequest? _pendingSavingSound;

    /// <summary>Returns and clears the saving sound an accepted save queued, once.</summary>
    internal SoundRequest? ConsumeSavingSoundRequest()
    {
        SoundRequest? request = _pendingSavingSound;
        _pendingSavingSound = null;
        return request;
    }

    /// <summary>Frame start: drops the previous pass's activation events.</summary>
    internal void BeginFrame() => _stationActivationEvents.Clear();

    /// <summary>
    /// The map station's side of gameplay resume ($82:A2E3). Map access-list deletion does
    /// not restore the normal Samus handlers; leaving the automatically opened map does,
    /// through command $0C (<see cref="SuperMetroid.Core.Samus.SamusControl.ReleaseRefillStationOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (PlmSlot slot in _slotTable.Slots)
        {
            // Unpause can arrive before the final access-art hold expires. Keep that
            // remaining animation; only a station waiting for the map returns to idle.
            if (slot.Active && slot.Family is StationPlmState { Kind: StationKind.Map } station &&
                station.OperationPhase == StationOperationPhase.AwaitingMapUnpause)
                station.OperationPhase = StationOperationPhase.Idle;
        }
    }

    /// <summary>Station messages/actions emitted by the most recent PLM handler pass.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivationEvents =>
        _stationActivationEvents;

    /// <summary>
    /// Sets WRAM <c>$1E75</c>'s room-entry lockout used when loading directly onto a save
    /// station. Ordinary destination-room construction clears it by creating a fresh PLM
    /// owner; an SRAM load explicitly sets it after the station population is installed.
    /// </summary>
    public void LockSaveStationForCurrentRoomEntry() => SaveStationLockedOut = true;

    /// <summary>
    /// Resumes the sleeping save-station PLM after bank $85 returns its YES/NO result.
    /// The accepted route executes `$84:8CF1` and continues through `$AFF2-$AFFA` within
    /// the same PLM pass: it centres Samus, queues the saving sound and draws the first
    /// animation frame. The declined route jumps to `$B008`, which still installs the
    /// once-per-room-entry lockout. Returns whether the station is saving.
    /// </summary>
    public bool ResolveSaveStationConfirmation(StationActivationEvent activation, bool accepted, RoomLevelData level)
    {
        Ensure.NotNull(level);
        StationPlmState station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingConfirmation)
        {
            throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} returned " +
                $"confirmation while in {station.SavePhase}.");
        }

        if (!accepted)
        {
            station.SavePhase = SaveStationPhase.Idle;
            SaveStationLockedOut = true;
            return false;
        }

        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Confirmed save station has no live Samus owner.");
        // Rounds Samus to the nearest block boundary.
        samus.Position = samus.Position.WithWholeX(unchecked((ushort)(((samus.Position.X.Whole + 8) >> 4) << 4)));
        samus.FaceForward();
        samus.Control.LockHandlers();

        station.SavePhase = SaveStationPhase.Animating;
        station.AnimationFrame = 0;
        station.SaveAnimationLoopsRemaining =
            WrappingCountdown.Start(SaveStationAnimationDefinitions.SaveAnimationLoops);
        // `$84:AFF4` queues library-one sound $2E right after centring Samus.
        _pendingSavingSound = _sounds.CreateSoundRequest(Library1Sound.Saving, MaximumQueued: SoundQueueLimit.Max6);
        DrawSaveStationFrame(level, activation.BlockIndex, station);
        return true;
    }

    /// <summary>Completes `$84:B030` after message $18 has closed.</summary>
    public void CompleteSaveStation(StationActivationEvent activation)
    {
        StationPlmState station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingCompletionMessageClose)
        {
            throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} completed " +
                $"message $18 while in {station.SavePhase}.");
        }

        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Completed save station has no live Samus owner.");
        samus.Control.Unlock();
        station.SavePhase = SaveStationPhase.Idle;
        SaveStationLockedOut = true;
    }

    /// <summary>Requires exactly one resident save PLM matching a frontend confirmation event.</summary>
    private StationPlmState FindSaveStation(StationActivationEvent activation)
    {
        StationPlmState[] matches = _slotTable.Slots
            .Where(slot => slot.Active && slot.Argument is StationArgument station && station.Station == activation.SaveStationNumber)
            .Select(slot => slot.Family as StationPlmState)
            .OfType<StationPlmState>()
            .Where(station => station.Kind == StationKind.Save && station.AreaIndex == activation.AreaIndex)
            .ToArray();
        return matches.Length switch
        {
            1 => matches[0],
            0 => throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} is no longer resident."),
            _ => throw new InvalidDataException(
                $"Multiple resident save stations share {activation.AreaIndex}:{activation.StationIndex}."),
        };
    }

    /// <summary>Installs station collision blocks and animation state, or deactivates an elevator platform's collision bits.</summary>
    internal static bool TrySetupStationOrElevator(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        PlmSlot slot)
    {
        if (slot.Header is not (PlmHeaderId.ElevatorPlatform or PlmHeaderId.MapStation or
            PlmHeaderId.EnergyStation or PlmHeaderId.MissileStation or PlmHeaderId.SaveStation))
            return false;
        switch (slot.Header)
        {
            case PlmHeaderId.ElevatorPlatform:
                {
                    // Setup_DeactivatePLM clears collision bits 12..14 but deliberately retains
                    // bit 15 and the visual payload; the platform then runs its idle loop.
                    slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.ElevatorPlatform));
                    level.SetForegroundEntry(slot.BlockIndex,
                        level.GetCollisionBlockByIndex(slot.BlockIndex).PackedWord.WithPlmDeactivation().Raw);
                    return true;
                }

            case PlmHeaderId.MapStation:
                SetupStation(level, system, area, slot, StationKind.Map);
                return true;
            case PlmHeaderId.EnergyStation:
                SetupStation(level, system, area, slot, StationKind.Energy);
                return true;
            case PlmHeaderId.MissileStation:
                SetupStation(level, system, area, slot, StationKind.Missile);
                return true;
            case PlmHeaderId.SaveStation:
                SetupStation(level, system, area, slot, StationKind.Save);
                return true;
            case PlmHeaderId.None:
            case PlmHeaderId.Nothing:
            case PlmHeaderId.CollisionReactionClearCarry:
            case PlmHeaderId.RightwardsScrollExtension:
            case PlmHeaderId.LeftwardsScrollExtension:
            case PlmHeaderId.DownwardsScrollExtension:
            case PlmHeaderId.UpwardsScrollExtension:
            case PlmHeaderId.WreckedShipEntranceTreadmillFromWest:
            case PlmHeaderId.WreckedShipEntranceTreadmillFromEast:
            case PlmHeaderId.InsideReactionNothingFirst:
            case PlmHeaderId.InsideReactionNothingSecond:
            case PlmHeaderId.InsideReactionNothingThird:
            case PlmHeaderId.FillMotherBrainsWall:
            case PlmHeaderId.MotherBrainsRoomEscapeDoor:
            case PlmHeaderId.MotherBrainsBackgroundRow2:
            case PlmHeaderId.MotherBrainsBackgroundRow3:
            case PlmHeaderId.MotherBrainsBackgroundRow4:
            case PlmHeaderId.MotherBrainsBackgroundRow5:
            case PlmHeaderId.MotherBrainsBackgroundRow6:
            case PlmHeaderId.MotherBrainsBackgroundRow7:
            case PlmHeaderId.MotherBrainsBackgroundRow8:
            case PlmHeaderId.MotherBrainsBackgroundRow9:
            case PlmHeaderId.MotherBrainsBackgroundRowA:
            case PlmHeaderId.MotherBrainsBackgroundRowB:
            case PlmHeaderId.MotherBrainsBackgroundRowC:
            case PlmHeaderId.MotherBrainsBackgroundRowD:
            case PlmHeaderId.ClearMotherBrainCeilingBlock:
            case PlmHeaderId.ClearMotherBrainCeilingTube:
            case PlmHeaderId.ClearMotherBrainBottomMiddleSideTube:
            case PlmHeaderId.ClearMotherBrainBottomMiddleTubes:
            case PlmHeaderId.ClearMotherBrainBottomLeftTube:
            case PlmHeaderId.ClearMotherBrainBottomRightTube:
            case PlmHeaderId.InsideReactionBrinstarFloorPlant:
            case PlmHeaderId.InsideReactionBrinstarCeilingPlant:
            case PlmHeaderId.MapStationRightAccess:
            case PlmHeaderId.MapStationLeftAccess:
            case PlmHeaderId.EnergyStationRightAccess:
            case PlmHeaderId.EnergyStationLeftAccess:
            case PlmHeaderId.MissileStationRightAccess:
            case PlmHeaderId.MissileStationLeftAccess:
            case PlmHeaderId.NothingSecond:
            case PlmHeaderId.NothingThird:
            case PlmHeaderId.ScrollTriggerCollision:
            case PlmHeaderId.ScrollTrigger:
            case PlmHeaderId.UnusedSolidScrollPLM:
            case PlmHeaderId.InsideReactionCrateria80:
            case PlmHeaderId.InsideReactionQuicksandSurface:
            case PlmHeaderId.InsideReactionSubmergingQuicksand:
            case PlmHeaderId.InsideReactionSandFallsSlow:
            case PlmHeaderId.InsideReactionSandFallsFast:
            case PlmHeaderId.CollisionReactionQuicksandSurface:
            case PlmHeaderId.CollisionReactionSubmergingQuicksand:
            case PlmHeaderId.CollisionReactionSandFallsSlow:
            case PlmHeaderId.CollisionReactionSandFallsFast:
            case PlmHeaderId.ClearCrocomireBridge:
            case PlmHeaderId.CrumbleCrocomireBridgeBlock:
            case PlmHeaderId.ClearCrocomireBridgeBlock:
            case PlmHeaderId.ClearCrocomireInvisibleWall:
            case PlmHeaderId.CreateCrocomireInvisibleWall:
            case PlmHeaderId.UnusedDraw13BlankAirTiles:
            case PlmHeaderId.UnusedDraw13BlankSolidTiles:
            case PlmHeaderId.ClearBabyMetroidInvisibleWall:
            case PlmHeaderId.CreateBabyMetroidInvisibleWall:
            case PlmHeaderId.SaveStationTrigger:
            case PlmHeaderId.CrumbleAccessToTourianElevator:
            case PlmHeaderId.ClearAccessToTourianElevator:
            case PlmHeaderId.DrawPhantoonDoorDuringBossFight:
            case PlmHeaderId.RestorePhantoonDoorAfterBossFight:
            case PlmHeaderId.CrumbleSporeSpawnCeiling:
            case PlmHeaderId.ClearSporeSpawnCeiling:
            case PlmHeaderId.ClearBotwoonWall:
            case PlmHeaderId.CrumbleBotwoonWall:
            case PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1:
            case PlmHeaderId.CrumbleKraidCeilingIntoBackground1:
            case PlmHeaderId.CrumbleKraidPlatformVariant1:
            case PlmHeaderId.CrumbleKraidCeilingIntoBackground2:
            case PlmHeaderId.CrumbleKraidPlatformVariant2:
            case PlmHeaderId.CrumbleKraidCeilingIntoBackground3:
            case PlmHeaderId.ClearKraidCeiling:
            case PlmHeaderId.ClearKraidSpikes:
            case PlmHeaderId.CrumbleKraidSpikes:
            case PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres:
            case PlmHeaderId.SpeedBoosterEscape:
            case PlmHeaderId.ShaktoolsRoom:
            case PlmHeaderId.MaridiaElevatube:
            case PlmHeaderId.OldTourianEscapeShaftFakeWall:
            case PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft:
            case PlmHeaderId.GateBlock:
            case PlmHeaderId.ReactionCrittersEscapeBlock:
            case PlmHeaderId.CrittersEscapeBlock:
            case PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape:
            case PlmHeaderId.BombTorizoGreyDoor:
            case PlmHeaderId.WreckedShipAttic:
            case PlmHeaderId.CrateriaMainstreetEscapePassage:
            case PlmHeaderId.LeftGreenGateTrigger:
            case PlmHeaderId.RightGreenGateTrigger:
            case PlmHeaderId.LeftRedGateTrigger:
            case PlmHeaderId.RightRedGateTrigger:
            case PlmHeaderId.LeftBlueGateTrigger:
            case PlmHeaderId.RightBlueGateTrigger:
            case PlmHeaderId.LeftYellowGateTrigger:
            case PlmHeaderId.RightYellowGateTrigger:
            case PlmHeaderId.DownwardsOpenGate:
            case PlmHeaderId.DownwardGate:
            case PlmHeaderId.UpwardsOpenGate:
            case PlmHeaderId.UpwardsClosedGate:
            case PlmHeaderId.DownwardGateShotBlock:
            case PlmHeaderId.UpwardsGateShotblock:
            case PlmHeaderId.GenericShotTriggerForAPLM:
            case PlmHeaderId.GreyDoorFacingLeft:
            case PlmHeaderId.GreyDoorFacingRight:
            case PlmHeaderId.GreyDoorFacingUp:
            case PlmHeaderId.GreyDoorFacingDown:
            case PlmHeaderId.YellowDoorFacingLeft:
            case PlmHeaderId.YellowDoorFacingRight:
            case PlmHeaderId.YellowDoorFacingUp:
            case PlmHeaderId.YellowDoorFacingDown:
            case PlmHeaderId.GreenDoorFacingLeft:
            case PlmHeaderId.GreenDoorFacingRight:
            case PlmHeaderId.GreenDoorFacingUp:
            case PlmHeaderId.GreenDoorFacingDown:
            case PlmHeaderId.RedDoorFacingLeft:
            case PlmHeaderId.RedDoorFacingRight:
            case PlmHeaderId.RedDoorFacingUp:
            case PlmHeaderId.RedDoorFacingDown:
            case PlmHeaderId.BlueDoorFacingLeft:
            case PlmHeaderId.BlueDoorFacingRight:
            case PlmHeaderId.BlueDoorFacingUp:
            case PlmHeaderId.BlueDoorFacingDown:
            case PlmHeaderId.BlueDoorClosingFacingLeft:
            case PlmHeaderId.BlueDoorClosingFacingRight:
            case PlmHeaderId.BlueDoorClosingFacingUp:
            case PlmHeaderId.BlueDoorClosingFacingDown:
            case PlmHeaderId.MotherBrainEscapeRoomGate:
            case PlmHeaderId.MotherBrainEscapeRoomGateClosing:
            case PlmHeaderId.Plm1x1RespawningCrumbleBlock:
            case PlmHeaderId.Plm2x1RespawningCrumbleBlock:
            case PlmHeaderId.Plm1x2RespawningCrumbleBlock:
            case PlmHeaderId.Plm2x2RespawningCrumbleBlock:
            case PlmHeaderId.BombReactionSpeedBoostBlock:
            case PlmHeaderId.SpeedBlockBrinstarSlowRespawning:
            case PlmHeaderId.SpeedBlockBrinstarSlowPermanent:
            case PlmHeaderId.SpeedBlockRespawning:
            case PlmHeaderId.SpeedBlockDachoraRespawning:
            case PlmHeaderId.SpeedBlockPermanent:
            case PlmHeaderId.ContactCrumble1x1Respawning:
            case PlmHeaderId.ContactCrumble2x1Respawning:
            case PlmHeaderId.ContactCrumble1x2Respawning:
            case PlmHeaderId.ContactCrumble2x2Respawning:
            case PlmHeaderId.ContactCrumble1x1Permanent:
            case PlmHeaderId.ContactCrumble2x1Permanent:
            case PlmHeaderId.ContactCrumble1x2Permanent:
            case PlmHeaderId.ContactCrumble2x2Permanent:
            case PlmHeaderId.Reaction1x1RespawningShotBlock:
            case PlmHeaderId.Reaction2x1RespawningShotBlock:
            case PlmHeaderId.Reaction1x2RespawningShotBlock:
            case PlmHeaderId.Reaction2x2RespawningShotBlock:
            case PlmHeaderId.Reaction1x1ShotBlock:
            case PlmHeaderId.Reaction2x1ShotBlock:
            case PlmHeaderId.Reaction1x2ShotBlock:
            case PlmHeaderId.Reaction2x2ShotBlock:
            case PlmHeaderId.ReactionRespawningPowerBombBlock:
            case PlmHeaderId.ReactionPowerBombBlock:
            case PlmHeaderId.ReactionRespawningSuperMissileBlock:
            case PlmHeaderId.ReactionSuperMissileBlock:
            case PlmHeaderId.EnemyBreakableBlock:
            case PlmHeaderId.Collision1x1RespawningBombBlock:
            case PlmHeaderId.Collision2x1RespawningBombBlock:
            case PlmHeaderId.Collision1x2RespawningBombBlock:
            case PlmHeaderId.Collision2x2RespawningBombBlock:
            case PlmHeaderId.Collision1x1BombBlock:
            case PlmHeaderId.Collision2x1BombBlock:
            case PlmHeaderId.Collision1x2BombBlock:
            case PlmHeaderId.Collision2x2BombBlock:
            case PlmHeaderId.Reaction1x1RespawningBombBlock:
            case PlmHeaderId.Reaction2x1RespawningBombBlock:
            case PlmHeaderId.Reaction1x2RespawningBombBlock:
            case PlmHeaderId.Reaction2x2RespawningBombBlock:
            case PlmHeaderId.Reaction1x1BombBlock:
            case PlmHeaderId.Reaction2x1BombBlock:
            case PlmHeaderId.Reaction1x2BombBlock:
            case PlmHeaderId.Reaction2x2BombBlock:
            case PlmHeaderId.GrappledGrappleBlock:
            case PlmHeaderId.GrappledRespawningBreakableGrappleBlock:
            case PlmHeaderId.GrappledBreakableGrappleBlock:
            case PlmHeaderId.GrappledGenericSpikeBlock:
            case PlmHeaderId.GrappledDraygonsBrokenTurret:
            case PlmHeaderId.UnusedBlueBrinstarFaceBlock:
            case PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug:
            case PlmHeaderId.UnusedShotBlock:
            case PlmHeaderId.UnusedGrappleBlock:
            case PlmHeaderId.LowerNorfairChozoHand:
            case PlmHeaderId.CollisionLowerNorfairChozoHandCheck:
            case PlmHeaderId.MotherBrainGlass:
            case PlmHeaderId.BombTorizoHand:
            case PlmHeaderId.WreckedShipChozoHand:
            case PlmHeaderId.CollisionWreckedShipChozoHandCheck:
            case PlmHeaderId.ClearSlopeAccessForWreckedShipChozo:
            case PlmHeaderId.BlockSlopeAccessForWreckedShipChozo:
            case PlmHeaderId.NoobTube:
            case PlmHeaderId.SetMetroidsClearedStatesWhenRequired:
            case PlmHeaderId.EyeDoorEyeFacingRight:
            case PlmHeaderId.EyeDoorFacingRight:
            case PlmHeaderId.EyeDoorBottomFacingRight:
            case PlmHeaderId.EyeDoorEyeFacingLeft:
            case PlmHeaderId.EyeDoorFacingLeft:
            case PlmHeaderId.EyeDoorBottomFacingLeft:
            case PlmHeaderId.DraygonCannonFacingRight:
            case PlmHeaderId.DraygonCannonFacingRightDestroyed:
            case PlmHeaderId.DraygonCannonFacingLeft:
            case PlmHeaderId.DraygonCannonFacingLeftUnshielded:
            case PlmHeaderId.ItemCollisionDetection:
            case PlmHeaderId.ExposedEnergyTank:
            case PlmHeaderId.ExposedMissileTank:
            case PlmHeaderId.ExposedSuperMissileTank:
            case PlmHeaderId.ExposedPowerBombTank:
            case PlmHeaderId.ExposedBombs:
            case PlmHeaderId.ExposedChargeBeam:
            case PlmHeaderId.ExposedIceBeam:
            case PlmHeaderId.ExposedHiJumpBoots:
            case PlmHeaderId.ExposedSpeedBooster:
            case PlmHeaderId.ExposedWaveBeam:
            case PlmHeaderId.ExposedSpazerBeam:
            case PlmHeaderId.ExposedSpringBall:
            case PlmHeaderId.ExposedVariaSuit:
            case PlmHeaderId.ExposedGravitySuit:
            case PlmHeaderId.ExposedXrayScope:
            case PlmHeaderId.ExposedPlasmaBeam:
            case PlmHeaderId.ExposedGrappleBeam:
            case PlmHeaderId.ExposedSpaceJump:
            case PlmHeaderId.ExposedScrewAttack:
            case PlmHeaderId.ExposedMorphBall:
            case PlmHeaderId.ExposedReserveTank:
            case PlmHeaderId.ChozoEnergyTank:
            case PlmHeaderId.ChozoMissileTank:
            case PlmHeaderId.ChozoSuperMissileTank:
            case PlmHeaderId.ChozoPowerBombTank:
            case PlmHeaderId.ChozoBombs:
            case PlmHeaderId.ChozoChargeBeam:
            case PlmHeaderId.ChozoIceBeam:
            case PlmHeaderId.ChozoHiJumpBoots:
            case PlmHeaderId.ChozoSpeedBooster:
            case PlmHeaderId.ChozoWaveBeam:
            case PlmHeaderId.ChozoSpazerBeam:
            case PlmHeaderId.ChozoSpringBall:
            case PlmHeaderId.ChozoVariaSuit:
            case PlmHeaderId.ChozoGravitySuit:
            case PlmHeaderId.ChozoXrayScope:
            case PlmHeaderId.ChozoPlasmaBeam:
            case PlmHeaderId.ChozoGrappleBeam:
            case PlmHeaderId.ChozoSpaceJump:
            case PlmHeaderId.ChozoScrewAttack:
            case PlmHeaderId.ChozoMorphBall:
            case PlmHeaderId.ChozoReserveTank:
            case PlmHeaderId.ShotBlockEnergyTank:
            case PlmHeaderId.ShotBlockMissileTank:
            case PlmHeaderId.ShotBlockSuperMissileTank:
            case PlmHeaderId.ShotBlockPowerBombTank:
            case PlmHeaderId.ShotBlockBombs:
            case PlmHeaderId.ShotBlockChargeBeam:
            case PlmHeaderId.ShotBlockIceBeam:
            case PlmHeaderId.ShotBlockHiJumpBoots:
            case PlmHeaderId.ShotBlockSpeedBooster:
            case PlmHeaderId.ShotBlockWaveBeam:
            case PlmHeaderId.ShotBlockSpazerBeam:
            case PlmHeaderId.ShotBlockSpringBall:
            case PlmHeaderId.ShotBlockVariaSuit:
            case PlmHeaderId.ShotBlockGravitySuit:
            case PlmHeaderId.ShotBlockXrayScope:
            case PlmHeaderId.ShotBlockPlasmaBeam:
            case PlmHeaderId.ShotBlockGrappleBeam:
            case PlmHeaderId.ShotBlockSpaceJump:
            case PlmHeaderId.ShotBlockScrewAttack:
            case PlmHeaderId.ShotBlockMorphBall:
            case PlmHeaderId.ShotBlockReserveTank:
            default:
                throw new InvalidOperationException($"Undefined station header {slot.Header}.");
        }
    }

    /// <summary>Installs station and access BTS blocks and selects the current animation for saved map state.</summary>
    private static void SetupStation(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        PlmSlot slot,
        StationKind kind)
    {
        RoomLevelWord original = level.GetCollisionBlockByIndex(slot.BlockIndex).PackedWord;
        switch (kind)
        {
            case StationKind.Save:
                SetupSaveStation(level, area, slot, original);
                return;
            case StationKind.Map:
                SetupAccessStation(level, system, area, slot, original, StationAccessLayouts.Map);
                return;
            case StationKind.Energy:
                SetupAccessStation(level, system, area, slot, original, StationAccessLayouts.Energy);
                return;
            case StationKind.Missile:
                SetupAccessStation(level, system, area, slot, original, StationAccessLayouts.Missile);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationKind)} {kind}.");
        }
    }

    /// <summary>A save station: its trigger floor and its one save animation.</summary>
    private static void SetupSaveStation(RoomLevelData level, AreaId area, PlmSlot slot, RoomLevelWord original)
    {
        WriteStationBlock(
            level,
            slot.BlockIndex,
            original,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehaviorCodec.StationAccess(StationAccessBehavior.SaveFloor));
        slot.Family = new StationPlmState(
            StationKind.Save,
            area,
            StationAnimationList.SaveIdle,
            completedAnimationList: StationAnimationList.SaveIdle,
            animationFrameCount: 1);
    }

    /// <summary>
    /// A map, energy or missile station: its solid block, the access blocks either side, and its animations. Only the map
    /// station has a separate acquired animation, shown from the start once its area map is held.
    /// </summary>
    private static void SetupAccessStation(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        PlmSlot slot,
        RoomLevelWord original,
        StationAccessLayout layout)
    {
        WriteStationBlock(level, slot.BlockIndex, original, RoomCollisionType.SolidBlock, behavior: null);
        WriteAccessBlock(level, slot.BlockIndex + layout.RightOffset, layout.RightBehavior);
        WriteAccessBlock(level, slot.BlockIndex + layout.LeftOffset, layout.LeftBehavior);

        StationAnimationList normalAnimationList = layout.Animation;
        StationAnimationList completedAnimationList = layout.AcquiredAnimation ?? normalAnimationList;
        StationAnimationList initialAnimationList = layout.AcquiredAnimation is not null && system.HasAreaMap(area)
            ? completedAnimationList
            : normalAnimationList;
        slot.Family = new StationPlmState(
            layout.Kind,
            area,
            initialAnimationList,
            completedAnimationList,
            animationFrameCount: 3);
    }

    /// <summary>Marks an in-room access block with station-trigger collision and its directional BTS.</summary>
    private static void WriteAccessBlock(
        RoomLevelData level,
        int blockIndex,
        StationAccessBehavior behavior)
    {
        if ((uint)blockIndex >= (uint)level.ForegroundEntries.Length)
        {
            throw new InvalidDataException(
                $"Station access setup targets out-of-room block index {blockIndex}.");
        }
        WriteStationBlock(
            level,
            blockIndex,
            level.GetCollisionBlockByIndex(blockIndex).PackedWord,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehaviorCodec.StationAccess(behavior));
    }

    /// <summary>Changes a station block's collision nibble while preserving its visual word and optional BTS.</summary>
    private static void WriteStationBlock(
        RoomLevelData level,
        int blockIndex,
        RoomLevelWord original,
        RoomCollisionType collisionType,
        RoomBlockBehavior? behavior)
    {
        level.SetForegroundEntry(blockIndex, original.WithCollisionType(collisionType).Raw);
        if (behavior is { } bts)
            level.SetBehavior(blockIndex, bts);
    }

    /// <summary>Typed BTS overload used by room collision dispatch.</summary>
    public bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        SamusCollisionPose collisionPose,
        Axis axis,
        bool movingPositive,
        int roomWidthInBlocks)
        => TryNotifyStationCollision(
            accessBlockIndex,
            behavior,
            collisionPose,
            axis,
            movingPositive,
            roomWidthInBlocks,
            StationSetupGate.Checked);

    /// <summary>Checks the entry pose and direction, then arms a matching resident station's PLM sequence.</summary>
    internal bool TryNotifyStationCollision(
        int accessBlockIndex,
        RoomBlockBehavior behavior,
        SamusCollisionPose collisionPose,
        Axis axis,
        bool movingPositive,
        int roomWidthInBlocks,
        StationSetupGate setupGate)
    {
        if (!behavior.TryGetStationAccess(out StationAccessBehavior access))
            return false;

        int parentBlockIndex = access switch
        {
            StationAccessBehavior.MapRight => accessBlockIndex - 1,
            StationAccessBehavior.MapLeft => accessBlockIndex + 2,
            StationAccessBehavior.EnergyRight or StationAccessBehavior.MissileRight =>
                accessBlockIndex - 1,
            StationAccessBehavior.EnergyLeft or StationAccessBehavior.MissileLeft =>
                accessBlockIndex + 1,
            StationAccessBehavior.SaveFloor => accessBlockIndex,
            _ => throw new InvalidOperationException($"Undefined station access {access}."),
        };

        foreach (PlmSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != parentBlockIndex || slot.Family is not StationPlmState station)
                continue;

            bool setupAccepted = setupGate == StationSetupGate.Bypassed || access switch
            {
                // Right-side access is entered while moving left in pose $8A; left-side
                // access is the mirror in pose $89. These are the exact B1C8/B1F0 and
                // B26D-B300 setup predicates before cannon-height alignment.
                StationAccessBehavior.MapRight or
                    StationAccessBehavior.EnergyRight or
                    StationAccessBehavior.MissileRight =>
                    axis == Axis.Horizontal && !movingPositive &&
                    collisionPose == SamusCollisionPose.RanIntoWallLeft,
                StationAccessBehavior.MapLeft or
                    StationAccessBehavior.EnergyLeft or
                    StationAccessBehavior.MissileLeft =>
                    axis == Axis.Horizontal && movingPositive &&
                    collisionPose == SamusCollisionPose.RanIntoWallRight,
                // A foot can touch the floor trigger before the body's biased center
                // reaches its column. Native accepts only the latter, narrower range.
                StationAccessBehavior.SaveFloor => axis == Axis.Vertical && movingPositive &&
                    collisionPose == SamusCollisionPose.StandingNormal &&
                    IsSaveStationTriggerCentered(slot.BlockIndex, roomWidthInBlocks),
                _ => throw new InvalidOperationException($"Undefined station access {access}."),
            };
            if (setupAccepted && station.OperationPhase == StationOperationPhase.Idle &&
                (station.Kind != StationKind.Save ||
                    (station.SavePhase == SaveStationPhase.Idle && !SaveStationLockedOut)))
            {
                // Recharge setup rejects an already-full resource before command six
                // takes control. Rejecting it later leaves a locked Samus with no access
                // animation running to release her. Collision itself remains solid.
                if (station.Kind is StationKind.Energy or StationKind.Missile)
                {
                    IRoomSamus samus = _samus?.Current
                        ?? throw new InvalidOperationException("Recharge setup has no live Samus owner.");
                    if (station.Kind == StationKind.Energy
                        ? samus.Health == samus.MaxHealth
                        : samus.Missiles == samus.MaxMissiles)
                    {
                        return true;
                    }
                }
                station.Triggered = true;
                station.AccessBlockIndex = accessBlockIndex;
                station.AccessBehavior = access;

                // ActivateStationIfSamusArmCannonLinedUp runs command six in setup,
                // before the common PLM handler advances the resident access list. This
                // distinction is observable in G-Mode: the disabled handler cannot start
                // or finish the animation, but Samus is already movement-locked and the
                // station therefore softlocks. Save trigger $B590 only advances its
                // resident list and does not execute command six at this setup seam.
                if (station.Kind != StationKind.Save)
                {
                    IRoomSamus samus = _samus?.Current
                        ?? throw new InvalidOperationException(
                            $"{station.Kind} station setup has no live Samus owner.");
                    samus.Control.LockIntoRefillStation();
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>Applies the cartridge's wrapping coordinate test before queuing any save side effect.</summary>
    private bool IsSaveStationTriggerCentered(int blockIndex, int roomWidthInBlocks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Save-station collision requires the room's Samus owner.");
        int probeX = unchecked((ushort)(samus.Position.X.Whole - SaveStationTriggerGeometry.HorizontalProbeOffset));
        return (probeX >> SaveStationTriggerGeometry.BlockCoordinateShift) == blockIndex % roomWidthInBlocks;
    }

    /// <summary>Advances station draw timers, access-arm phases, and activation messages during the PLM pass.</summary>
    internal bool TryStepStation(RoomLevelData level, PlmSlot slot)
    {
        StationPlmState? station = slot.Family as StationPlmState;
        if (station is null)
            return false;

        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException(
                $"{station.Kind} station has no live Samus owner.");

        // The access list's first draw instruction installs its six-frame timer on the
        // triggering pass itself; the common PLM handler only decrements on later passes.
        bool operationStartedThisPass = false;
        if (station.Triggered)
        {
            station.Triggered = false;
            bool canActivate = station.Kind switch
            {
                StationKind.Map => !(_system ?? throw new InvalidOperationException(
                    "Map station has no shared game state owner.")).HasAreaMap(station.AreaIndex),
                StationKind.Energy => samus.Health != samus.MaxHealth,
                StationKind.Missile => samus.Missiles != samus.MaxMissiles,
                StationKind.Save => true,
                _ => throw new InvalidDataException($"Unknown station kind {station.Kind}."),
            };
            if (!canActivate && station.Kind is StationKind.Energy or StationKind.Missile)
            {
                // The access-list full-resource branch runs command one before deleting
                // itself. A refill between setup and this pass must release control too.
                samus.Control.Unlock();
                station.AccessBlockIndex = -1;
                station.AccessBehavior = null;
            }
            if (canActivate && station.Kind == StationKind.Save)
            {
                station.SavePhase = SaveStationPhase.AwaitingConfirmation;
                PublishStationActivation(station, slot, samus);
            }
            else if (canActivate)
            {
                // Access lists draw their first extension frame for six ticks, hold the
                // fully inserted arm for $60, then execute the activation opcode. Command
                // six locked Samus in setup; this pass does not lock her again, so one an
                // intervening unpause freed ($90:F2A2) stays free.
                station.OperationPhase = StationOperationPhase.Extending;
                station.OperationTimer = WrappingCountdown.Start(StationAccessMovementFrames);
                operationStartedThisPass = true;
                _sounds.Queue(Library2Sound.StationEngaged, MaximumQueued: SoundQueueLimit.Max6);
                DrawStationAccess(level, station, pose: StationAccessPose.Retracted);
            }
        }

        if (station.Kind == StationKind.Save && StepSaveStationAnimation(level, slot, station))
        {
            return true;
        }

        if (!operationStartedThisPass &&
            station.OperationPhase is not (StationOperationPhase.Idle or StationOperationPhase.AwaitingMapUnpause))
        {
            station.OperationTimer = station.OperationTimer.Decrement().Remaining;
            if (station.OperationTimer.IsZero)
            {
                switch (station.OperationPhase)
                {
                    case StationOperationPhase.Extending:
                        DrawStationAccess(level, station, pose: StationAccessPose.Extended);
                        station.OperationPhase = StationOperationPhase.Extended;
                        station.OperationTimer = WrappingCountdown.Start(StationAccessExtendedHoldFrames);
                        break;
                    case StationOperationPhase.Extended:
                        PublishStationActivation(station, slot, samus);
                        // $84:8CC6/$8CE7: the recharge activations run Samus command one
                        // right after their message box, before the access retracts. The
                        // map activation leaves input locked for pause teardown's command.
                        if (station.Kind != StationKind.Map)
                            samus.Control.Unlock();
                        station.OperationPhase = StationOperationPhase.PostActivationHold;
                        station.OperationTimer = WrappingCountdown.Start(StationAccessMovementFrames);
                        break;
                    case StationOperationPhase.PostActivationHold:
                        _sounds.Queue(Library2Sound.StationDisengaged, MaximumQueued: SoundQueueLimit.Max6);
                        station.OperationPhase = StationOperationPhase.Retracting;
                        station.OperationTimer = WrappingCountdown.Start(StationAccessMovementFrames);
                        break;
                    case StationOperationPhase.Retracting:
                        DrawStationAccess(level, station, pose: StationAccessPose.Retracted);
                        station.OperationPhase = StationOperationPhase.FinalRetractionHold;
                        station.OperationTimer = WrappingCountdown.Start(StationAccessMovementFrames);
                        break;
                    case StationOperationPhase.FinalRetractionHold:
                        station.OperationPhase = station.Kind == StationKind.Map && samus.Control.AlphaLocked
                            ? StationOperationPhase.AwaitingMapUnpause
                            : StationOperationPhase.Idle;
                        station.AccessBlockIndex = -1;
                        station.AccessBehavior = null;
                        // Every access list only deletes itself here. Recharges released
                        // input at activation; map pause teardown's command $0C owns its release.
                        break;
                    case StationOperationPhase.Idle:
                    case StationOperationPhase.AwaitingMapUnpause:
                    default:
                        throw new InvalidDataException(
                            $"Unknown station operation phase {station.OperationPhase}.");
                }
            }
        }

        // Save station $AFE8 draws once and then sleeps until its BTS-$4D trigger advances
        // it. Its confirmation/save coroutine is published above for the frontend owner.
        if (station.Kind == StationKind.Save && station.InitialDrawCompleted)
            return true;

        station.AnimationTimer = station.AnimationTimer.Decrement().Remaining;
        if (!station.AnimationTimer.IsZero)
            return true;

        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationPrograms.Resolve(
                station.AnimationList, station.AnimationFrame);
        WrappingCountdown timer = WrappingCountdown.FromRaw(frame.Duration);
        PlmDraw draw = frame.Draw;
        if (timer.IsNegative || timer.IsZero)
        {
            throw new InvalidDataException(
                $"{station.Kind} station animation {station.AnimationList} " +
                $"frame {station.AnimationFrame} does not begin with a timed draw.");
        }
        _drawing.Draw(level, slot.BlockIndex, draw);
        station.AnimationTimer = timer;
        station.InitialDrawCompleted = true;
        station.AnimationFrame = (station.AnimationFrame + 1) % station.AnimationFrameCount;
        return true;
    }

    /// <summary>Runs the accepted save's two-frame loop until the saved-game message can be published.</summary>
    private bool StepSaveStationAnimation(RoomLevelData level, PlmSlot slot, StationPlmState station)
    {
        if (station.SavePhase is SaveStationPhase.Idle or
            SaveStationPhase.AwaitingConfirmation)
        {
            return false;
        }
        if (station.SavePhase == SaveStationPhase.AwaitingCompletionMessageClose)
            return true;
        if (station.SavePhase != SaveStationPhase.Animating)
        {
            throw new InvalidDataException(
                $"Save station has unknown phase {station.SavePhase}.");
        }

        station.AnimationTimer = station.AnimationTimer.Decrement().Remaining;
        if (!station.AnimationTimer.IsZero)
            return true;

        // `$84:B002` runs when the second frame's four frames expire. After the last of
        // the 21 loops it falls through to `$84:B006`, the saved-game message, instead of
        // returning to the first frame. (The loop is counted at the second draw here.)
        if (station.AnimationFrame == 0 && station.SaveAnimationLoopsRemaining.IsZero)
        {
            station.SavePhase = SaveStationPhase.AwaitingCompletionMessageClose;
            _stationActivationEvents.Add(new StationActivationEvent(
                StationKind.Save,
                GameplayMessageIds.SaveCompleted,
                station.AreaIndex,
                StationNumber(slot),
                slot.BlockIndex));
            return true;
        }
        DrawSaveStationFrame(level, slot.BlockIndex, station);
        return true;
    }

    /// <summary>Draws the next of the two four-frame electricity frames (`$84:AFFA`/`$AFFE`).</summary>
    private void DrawSaveStationFrame(RoomLevelData level, int blockIndex, StationPlmState station)
    {
        StationAnimationList list = station.AnimationFrame == 0
            ? StationAnimationList.SaveFirstFrame
            : StationAnimationList.SaveSecondFrame;
        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationPrograms.Resolve(list, 0);
        if (frame.Duration != 4)
        {
            throw new InvalidDataException(
                $"Save-station animation list {list} has timer {frame.Duration}, expected 4.");
        }
        _drawing.Draw(level, blockIndex, frame.Draw);
        station.AnimationTimer = WrappingCountdown.FromRaw(frame.Duration);
        station.AnimationFrame ^= 1;
        if (station.AnimationFrame == 0)
            station.SaveAnimationLoopsRemaining = station.SaveAnimationLoopsRemaining.Decrement().Remaining;
    }

    /// <summary>Applies map or refill effects and publishes the station's frontend message.</summary>
    private void PublishStationActivation(
        StationPlmState station,
        PlmSlot slot,
        IRoomSamus samus)
    {
        GameplayMessageId message = station.Kind switch
        {
            StationKind.Map => GameplayMessageIds.MapDataAccessCompleted,
            StationKind.Energy => GameplayMessageIds.EnergyRechargeCompleted,
            StationKind.Missile => GameplayMessageIds.MissileRechargeCompleted,
            StationKind.Save => GameplayMessageIds.SaveConfirmation,
            _ => throw new InvalidDataException($"Unknown station kind {station.Kind}."),
        };

        switch (station.Kind)
        {
            case StationKind.Map:
                (_system ?? throw new InvalidOperationException(
                    "Map station has no shared game state owner."))
                    .SetAreaMapAcquired(station.AreaIndex);
                station.AnimationList = station.CompletedAnimationList;
                break;
            case StationKind.Energy:
                samus.Health = samus.MaxHealth;
                station.AnimationList = station.CompletedAnimationList;
                break;
            case StationKind.Missile:
                samus.Missiles = samus.MaxMissiles;
                station.AnimationList = station.CompletedAnimationList;
                break;
            case StationKind.Save:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationKind)} {station.Kind}.");
        }

        station.AnimationFrame = 0;
        station.AnimationTimer = WrappingCountdown.Start(1);
        _stationActivationEvents.Add(new StationActivationEvent(
            station.Kind,
            message,
            station.AreaIndex,
            (slot.Argument as StationArgument)?.Station,
            slot.BlockIndex));
    }

    /// <summary>The number a station was placed with, which its activation events carry.</summary>
    private static int StationNumber(PlmSlot slot) => (slot.Argument as StationArgument
        ?? throw new InvalidDataException($"Station at block {slot.BlockIndex} was placed without its number.")).Station;

    /// <summary>Draws the station access arm at its captured collision block in the short or extended pose.</summary>
    private void DrawStationAccess(RoomLevelData level, StationPlmState station, StationAccessPose pose)
    {
        StationAccessBehavior accessBehavior = station.AccessBehavior
            ?? throw new InvalidDataException("Station access has no BTS owner.");
        StationAccessPlmDefinition definition =
            StationAccessPlms.Resolve(accessBehavior);
        _drawing.Draw(level, station.AccessBlockIndex, PlmDraw.Of(pose == StationAccessPose.Extended ? definition.Extended : definition.Retracted));
    }

    /// <summary>Resident PLM state for station appearance, access-arm timing, and save confirmation.</summary>
    internal sealed class StationPlmState(
        StationKind kind,
        AreaId area,
        StationAnimationList animationList,
        StationAnimationList completedAnimationList,
        int animationFrameCount) : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.Station;

        /// <summary>Determines whether activation opens the map, refills a resource, or starts a save.</summary>
        public StationKind Kind { get; } = kind;
        /// <summary>Area whose map-acquired or save-station state is affected.</summary>
        public AreaId AreaIndex { get; } = area;
        /// <summary>Animation currently used by the resident station PLM.</summary>
        public StationAnimationList AnimationList { get; set; } = animationList;
        /// <summary>Animation selected after the station has completed its first activation.</summary>
        public StationAnimationList CompletedAnimationList { get; } = completedAnimationList;
        /// <summary>Number of timed draws in the current normal animation loop.</summary>
        public int AnimationFrameCount { get; } = animationFrameCount;
        /// <summary>Next frame index in the resident station's draw list.</summary>
        public int AnimationFrame { get; set; }
        /// <summary>Ticks remaining before the next station draw.</summary>
        public WrappingCountdown AnimationTimer { get; set; } = WrappingCountdown.Start(1);
        /// <summary>Keeps the save trigger asleep after its one initial draw.</summary>
        public bool InitialDrawCompleted { get; set; }
        /// <summary>Records accepted BTS collision for the next PLM handler pass.</summary>
        public bool Triggered { get; set; }
        /// <summary>Current extension, hold, retraction, or map-unpause phase.</summary>
        public StationOperationPhase OperationPhase { get; set; }
        /// <summary>Ticks remaining in the current access-arm phase.</summary>
        public WrappingCountdown OperationTimer { get; set; }
        /// <summary>Room block index where the activated access arm must be drawn.</summary>
        public int AccessBlockIndex { get; set; } = -1;
        /// <summary>Directional BTS type identifying the selected access draw list.</summary>
        public StationAccessBehavior? AccessBehavior { get; set; }
        /// <summary>Tracks frontend confirmation and save animation separately from arm motion.</summary>
        public SaveStationPhase SavePhase { get; set; }
        /// <summary>Remaining two-frame save animation loops before the completion message.</summary>
        public WrappingCountdown SaveAnimationLoopsRemaining { get; set; }
    }

    /// <summary>Phases of the non-save station's access-arm PLM list.</summary>
    internal enum StationOperationPhase
    {
        /// <summary>No access sequence is in progress.</summary>
        Idle,
        /// <summary>The first access draw is waiting to extend.</summary>
        Extending,
        /// <summary>The extended arm is holding before station activation.</summary>
        Extended,
        /// <summary>Station activation finished; the arm has not begun retraction.</summary>
        PostActivationHold,
        /// <summary>The arm is retracting after its sound request.</summary>
        Retracting,
        /// <summary>The short arm pose is holding before the access list ends.</summary>
        FinalRetractionHold,
        /// <summary>The access actor has finished; command <c>$0C</c> still owns Samus release.</summary>
        AwaitingMapUnpause,
    }
}

using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Save, refill, map and recharge station PLMs: activation, lockout, collision notification and their draws.</summary>
[RoomLifetime]
internal sealed class StationObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the Station family's step.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.Station, (slot, context) =>
            TryStepStation(context.Level, slot) ? RoomObjectStepOutcome.Consumed : RoomObjectStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Applies the station and access-arm draw lists to the room tilemap.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>Locates resident station PLMs when collision or frontend confirmation arrives.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Queues extension, retraction, and save-animation sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Connects station activation to room state, PLM drawing, resident slots, and sound requests.</summary>
    internal StationObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomObjectDrawing drawing, RoomObjectSlotTable slotTable, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _drawing = drawing;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Frames spent extending or retracting the station access arm.</summary>
    private const int StationAccessMovementFrames = 6;
    /// <summary>Hold before a fully extended arm publishes station activation.</summary>
    private const int StationAccessExtendedHoldFrames = 96;
    /// <summary>Messages and frontend actions produced in the current PLM handler pass.</summary>
    private readonly List<StationActivationEvent> _stationActivationEvents = [];
    /// <summary>Blocks repeated save confirmation during the current room entry.</summary>
    internal bool SaveStationLockedOut { get; private set; }

    /// <summary>
    /// The saving sound queued when the save was accepted, until the frontend publishes it. The
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
    /// The map station's side of gameplay resume. Map access-list deletion does
    /// not restore the normal Samus handlers; leaving the automatically opened map does,
    /// through the map-unpause release command (<see cref="SuperMetroid.Core.Samus.SamusControl.ReleaseRefillStationOnUnpause"/>).
    /// </summary>
    public void ResumeMapStationsAfterUnpause()
    {
        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            // Unpause can arrive before the final access-art hold expires. Keep that
            // remaining animation; only a station waiting for the map returns to idle.
            if (slot.Active && slot.Family is StationObjectState { Kind: StationKind.Map } station &&
                station.OperationPhase == StationOperationPhase.AwaitingMapUnpause)
                station.OperationPhase = StationOperationPhase.Idle;
        }
    }

    /// <summary>Station messages/actions emitted by the most recent PLM handler pass.</summary>
    public IReadOnlyList<StationActivationEvent> StationActivationEvents =>
        _stationActivationEvents;

    /// <summary>
    /// Sets the room-entry lockout used when loading directly onto a save
    /// station. Ordinary destination-room construction clears it by creating a fresh PLM
    /// owner; loading a saved game explicitly sets it after the station population is installed.
    /// </summary>
    public void LockSaveStationForCurrentRoomEntry() => SaveStationLockedOut = true;

    /// <summary>
    /// Resumes the sleeping save-station PLM after the save message returns its YES/NO result.
    /// The accepted route centres Samus, queues the saving sound and draws the first
    /// animation frame within the same PLM pass. The declined route still installs the
    /// once-per-room-entry lockout. Returns whether the station is saving.
    /// </summary>
    public bool ResolveSaveStationConfirmation(StationActivationEvent activation, bool accepted, RoomLevelData level)
    {
        Ensure.NotNull(level);
        StationObjectState station = FindSaveStation(activation);
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
        samus.Position = samus.Position.WithWrappedWholeX(((samus.Position.X.Whole + 8) >> 4) << 4);
        samus.FaceForward();
        samus.Control.LockHandlers();

        station.SavePhase = SaveStationPhase.Animating;
        station.AnimationFrame = 0;
        station.SaveAnimationLoopsRemaining = SaveStationAnimationDefinitions.SaveAnimationLoops;
        // The accepted route queues the saving sound right after centring Samus.
        _pendingSavingSound = _sounds.CreateSoundRequest(SamusSound.Saving, Priority: SoundRequestPriority.Normal);
        DrawSaveStationFrame(level, activation.BlockIndex, station);
        return true;
    }

    /// <summary>Completes the save after the saved-game message has closed.</summary>
    public void CompleteSaveStation(StationActivationEvent activation)
    {
        StationObjectState station = FindSaveStation(activation);
        if (station.SavePhase != SaveStationPhase.AwaitingCompletionMessageClose)
        {
            throw new InvalidOperationException(
                $"Save station {activation.AreaIndex}:{activation.StationIndex} completed " +
                $"save-completed message while in {station.SavePhase}.");
        }

        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Completed save station has no live Samus owner.");
        samus.Control.Unlock();
        station.SavePhase = SaveStationPhase.Idle;
        SaveStationLockedOut = true;
    }

    /// <summary>Requires exactly one resident save PLM matching a frontend confirmation event.</summary>
    private StationObjectState FindSaveStation(StationActivationEvent activation)
    {
        StationObjectState[] matches = _slotTable.Slots
            .Where(slot => slot.Active && slot.Argument is StationArgument station && station.Station == activation.SaveStationNumber)
            .Select(slot => slot.Family as StationObjectState)
            .OfType<StationObjectState>()
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
        RoomObjectSlot slot)
    {
        if (slot.Header is not (RoomObjectHeaderId.ElevatorPlatform or RoomObjectHeaderId.MapStation or
            RoomObjectHeaderId.EnergyStation or RoomObjectHeaderId.MissileStation or RoomObjectHeaderId.SaveStation))
            return false;
        switch (slot.Header)
        {
            case RoomObjectHeaderId.ElevatorPlatform:
                {
                    // The deactivating setup leaves a solid block solid and any other block air, keeping its
                    // appearance; the platform then runs its idle loop.
                    slot.StartAnimation(new ScriptedRoomObjectAnimation(ScriptedRoomObjectProgram.ElevatorPlatform));
                    level.SetTerrain(slot.BlockIndex,
                        level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain.Deactivated());
                    return true;
                }

            case RoomObjectHeaderId.MapStation:
                SetupStation(level, system, area, slot, StationKind.Map);
                return true;
            case RoomObjectHeaderId.EnergyStation:
                SetupStation(level, system, area, slot, StationKind.Energy);
                return true;
            case RoomObjectHeaderId.MissileStation:
                SetupStation(level, system, area, slot, StationKind.Missile);
                return true;
            case RoomObjectHeaderId.SaveStation:
                SetupStation(level, system, area, slot, StationKind.Save);
                return true;
            case RoomObjectHeaderId.None:
            case RoomObjectHeaderId.Nothing:
            case RoomObjectHeaderId.CollisionReactionClearCarry:
            case RoomObjectHeaderId.RightwardsScrollExtension:
            case RoomObjectHeaderId.LeftwardsScrollExtension:
            case RoomObjectHeaderId.DownwardsScrollExtension:
            case RoomObjectHeaderId.UpwardsScrollExtension:
            case RoomObjectHeaderId.WreckedShipEntranceTreadmillFromWest:
            case RoomObjectHeaderId.WreckedShipEntranceTreadmillFromEast:
            case RoomObjectHeaderId.InsideReactionNothingFirst:
            case RoomObjectHeaderId.InsideReactionNothingSecond:
            case RoomObjectHeaderId.InsideReactionNothingThird:
            case RoomObjectHeaderId.FillMotherBrainsWall:
            case RoomObjectHeaderId.MotherBrainsRoomEscapeDoor:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow2:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow3:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow4:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow5:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow6:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow7:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow8:
            case RoomObjectHeaderId.MotherBrainsBackgroundRow9:
            case RoomObjectHeaderId.MotherBrainsBackgroundRowA:
            case RoomObjectHeaderId.MotherBrainsBackgroundRowB:
            case RoomObjectHeaderId.MotherBrainsBackgroundRowC:
            case RoomObjectHeaderId.MotherBrainsBackgroundRowD:
            case RoomObjectHeaderId.ClearMotherBrainCeilingBlock:
            case RoomObjectHeaderId.ClearMotherBrainCeilingTube:
            case RoomObjectHeaderId.ClearMotherBrainBottomMiddleSideTube:
            case RoomObjectHeaderId.ClearMotherBrainBottomMiddleTubes:
            case RoomObjectHeaderId.ClearMotherBrainBottomLeftTube:
            case RoomObjectHeaderId.ClearMotherBrainBottomRightTube:
            case RoomObjectHeaderId.InsideReactionBrinstarFloorPlant:
            case RoomObjectHeaderId.InsideReactionBrinstarCeilingPlant:
            case RoomObjectHeaderId.MapStationRightAccess:
            case RoomObjectHeaderId.MapStationLeftAccess:
            case RoomObjectHeaderId.EnergyStationRightAccess:
            case RoomObjectHeaderId.EnergyStationLeftAccess:
            case RoomObjectHeaderId.MissileStationRightAccess:
            case RoomObjectHeaderId.MissileStationLeftAccess:
            case RoomObjectHeaderId.NothingSecond:
            case RoomObjectHeaderId.NothingThird:
            case RoomObjectHeaderId.ScrollTriggerCollision:
            case RoomObjectHeaderId.ScrollTrigger:
            case RoomObjectHeaderId.UnusedSolidScrollPLM:
            case RoomObjectHeaderId.InsideReactionCrateria80:
            case RoomObjectHeaderId.InsideReactionQuicksandSurface:
            case RoomObjectHeaderId.InsideReactionSubmergingQuicksand:
            case RoomObjectHeaderId.InsideReactionSandFallsSlow:
            case RoomObjectHeaderId.InsideReactionSandFallsFast:
            case RoomObjectHeaderId.CollisionReactionQuicksandSurface:
            case RoomObjectHeaderId.CollisionReactionSubmergingQuicksand:
            case RoomObjectHeaderId.CollisionReactionSandFallsSlow:
            case RoomObjectHeaderId.CollisionReactionSandFallsFast:
            case RoomObjectHeaderId.ClearCrocomireBridge:
            case RoomObjectHeaderId.CrumbleCrocomireBridgeBlock:
            case RoomObjectHeaderId.ClearCrocomireBridgeBlock:
            case RoomObjectHeaderId.ClearCrocomireInvisibleWall:
            case RoomObjectHeaderId.CreateCrocomireInvisibleWall:
            case RoomObjectHeaderId.UnusedDraw13BlankAirTiles:
            case RoomObjectHeaderId.UnusedDraw13BlankSolidTiles:
            case RoomObjectHeaderId.ClearBabyMetroidInvisibleWall:
            case RoomObjectHeaderId.CreateBabyMetroidInvisibleWall:
            case RoomObjectHeaderId.SaveStationTrigger:
            case RoomObjectHeaderId.CrumbleAccessToTourianElevator:
            case RoomObjectHeaderId.ClearAccessToTourianElevator:
            case RoomObjectHeaderId.DrawPhantoonDoorDuringBossFight:
            case RoomObjectHeaderId.RestorePhantoonDoorAfterBossFight:
            case RoomObjectHeaderId.CrumbleSporeSpawnCeiling:
            case RoomObjectHeaderId.ClearSporeSpawnCeiling:
            case RoomObjectHeaderId.ClearBotwoonWall:
            case RoomObjectHeaderId.CrumbleBotwoonWall:
            case RoomObjectHeaderId.UnusedSetKraidCeilingBlockToBackground1:
            case RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground1:
            case RoomObjectHeaderId.CrumbleKraidPlatformVariant1:
            case RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground2:
            case RoomObjectHeaderId.CrumbleKraidPlatformVariant2:
            case RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground3:
            case RoomObjectHeaderId.ClearKraidCeiling:
            case RoomObjectHeaderId.ClearKraidSpikes:
            case RoomObjectHeaderId.CrumbleKraidSpikes:
            case RoomObjectHeaderId.EnableSoundsIn20FramesF0FramesIfCeres:
            case RoomObjectHeaderId.SpeedBoosterEscape:
            case RoomObjectHeaderId.ShaktoolsRoom:
            case RoomObjectHeaderId.MaridiaElevatube:
            case RoomObjectHeaderId.OldTourianEscapeShaftFakeWall:
            case RoomObjectHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft:
            case RoomObjectHeaderId.GateBlock:
            case RoomObjectHeaderId.ReactionCrittersEscapeBlock:
            case RoomObjectHeaderId.CrittersEscapeBlock:
            case RoomObjectHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape:
            case RoomObjectHeaderId.BombTorizoGreyDoor:
            case RoomObjectHeaderId.WreckedShipAttic:
            case RoomObjectHeaderId.CrateriaMainstreetEscapePassage:
            case RoomObjectHeaderId.LeftGreenGateTrigger:
            case RoomObjectHeaderId.RightGreenGateTrigger:
            case RoomObjectHeaderId.LeftRedGateTrigger:
            case RoomObjectHeaderId.RightRedGateTrigger:
            case RoomObjectHeaderId.LeftBlueGateTrigger:
            case RoomObjectHeaderId.RightBlueGateTrigger:
            case RoomObjectHeaderId.LeftYellowGateTrigger:
            case RoomObjectHeaderId.RightYellowGateTrigger:
            case RoomObjectHeaderId.DownwardsOpenGate:
            case RoomObjectHeaderId.DownwardGate:
            case RoomObjectHeaderId.UpwardsOpenGate:
            case RoomObjectHeaderId.UpwardsClosedGate:
            case RoomObjectHeaderId.DownwardGateShotBlock:
            case RoomObjectHeaderId.UpwardsGateShotblock:
            case RoomObjectHeaderId.GenericShotTriggerForAPLM:
            case RoomObjectHeaderId.GreyDoorFacingLeft:
            case RoomObjectHeaderId.GreyDoorFacingRight:
            case RoomObjectHeaderId.GreyDoorFacingUp:
            case RoomObjectHeaderId.GreyDoorFacingDown:
            case RoomObjectHeaderId.YellowDoorFacingLeft:
            case RoomObjectHeaderId.YellowDoorFacingRight:
            case RoomObjectHeaderId.YellowDoorFacingUp:
            case RoomObjectHeaderId.YellowDoorFacingDown:
            case RoomObjectHeaderId.GreenDoorFacingLeft:
            case RoomObjectHeaderId.GreenDoorFacingRight:
            case RoomObjectHeaderId.GreenDoorFacingUp:
            case RoomObjectHeaderId.GreenDoorFacingDown:
            case RoomObjectHeaderId.RedDoorFacingLeft:
            case RoomObjectHeaderId.RedDoorFacingRight:
            case RoomObjectHeaderId.RedDoorFacingUp:
            case RoomObjectHeaderId.RedDoorFacingDown:
            case RoomObjectHeaderId.BlueDoorFacingLeft:
            case RoomObjectHeaderId.BlueDoorFacingRight:
            case RoomObjectHeaderId.BlueDoorFacingUp:
            case RoomObjectHeaderId.BlueDoorFacingDown:
            case RoomObjectHeaderId.BlueDoorClosingFacingLeft:
            case RoomObjectHeaderId.BlueDoorClosingFacingRight:
            case RoomObjectHeaderId.BlueDoorClosingFacingUp:
            case RoomObjectHeaderId.BlueDoorClosingFacingDown:
            case RoomObjectHeaderId.MotherBrainEscapeRoomGate:
            case RoomObjectHeaderId.MotherBrainEscapeRoomGateClosing:
            case RoomObjectHeaderId.RoomObject1x1RespawningCrumbleBlock:
            case RoomObjectHeaderId.RoomObject2x1RespawningCrumbleBlock:
            case RoomObjectHeaderId.RoomObject1x2RespawningCrumbleBlock:
            case RoomObjectHeaderId.RoomObject2x2RespawningCrumbleBlock:
            case RoomObjectHeaderId.BombReactionSpeedBoostBlock:
            case RoomObjectHeaderId.SpeedBlockBrinstarSlowRespawning:
            case RoomObjectHeaderId.SpeedBlockBrinstarSlowPermanent:
            case RoomObjectHeaderId.SpeedBlockRespawning:
            case RoomObjectHeaderId.SpeedBlockDachoraRespawning:
            case RoomObjectHeaderId.SpeedBlockPermanent:
            case RoomObjectHeaderId.ContactCrumble1x1Respawning:
            case RoomObjectHeaderId.ContactCrumble2x1Respawning:
            case RoomObjectHeaderId.ContactCrumble1x2Respawning:
            case RoomObjectHeaderId.ContactCrumble2x2Respawning:
            case RoomObjectHeaderId.ContactCrumble1x1Permanent:
            case RoomObjectHeaderId.ContactCrumble2x1Permanent:
            case RoomObjectHeaderId.ContactCrumble1x2Permanent:
            case RoomObjectHeaderId.ContactCrumble2x2Permanent:
            case RoomObjectHeaderId.Reaction1x1RespawningShotBlock:
            case RoomObjectHeaderId.Reaction2x1RespawningShotBlock:
            case RoomObjectHeaderId.Reaction1x2RespawningShotBlock:
            case RoomObjectHeaderId.Reaction2x2RespawningShotBlock:
            case RoomObjectHeaderId.Reaction1x1ShotBlock:
            case RoomObjectHeaderId.Reaction2x1ShotBlock:
            case RoomObjectHeaderId.Reaction1x2ShotBlock:
            case RoomObjectHeaderId.Reaction2x2ShotBlock:
            case RoomObjectHeaderId.ReactionRespawningPowerBombBlock:
            case RoomObjectHeaderId.ReactionPowerBombBlock:
            case RoomObjectHeaderId.ReactionRespawningSuperMissileBlock:
            case RoomObjectHeaderId.ReactionSuperMissileBlock:
            case RoomObjectHeaderId.EnemyBreakableBlock:
            case RoomObjectHeaderId.Collision1x1RespawningBombBlock:
            case RoomObjectHeaderId.Collision2x1RespawningBombBlock:
            case RoomObjectHeaderId.Collision1x2RespawningBombBlock:
            case RoomObjectHeaderId.Collision2x2RespawningBombBlock:
            case RoomObjectHeaderId.Collision1x1BombBlock:
            case RoomObjectHeaderId.Collision2x1BombBlock:
            case RoomObjectHeaderId.Collision1x2BombBlock:
            case RoomObjectHeaderId.Collision2x2BombBlock:
            case RoomObjectHeaderId.Reaction1x1RespawningBombBlock:
            case RoomObjectHeaderId.Reaction2x1RespawningBombBlock:
            case RoomObjectHeaderId.Reaction1x2RespawningBombBlock:
            case RoomObjectHeaderId.Reaction2x2RespawningBombBlock:
            case RoomObjectHeaderId.Reaction1x1BombBlock:
            case RoomObjectHeaderId.Reaction2x1BombBlock:
            case RoomObjectHeaderId.Reaction1x2BombBlock:
            case RoomObjectHeaderId.Reaction2x2BombBlock:
            case RoomObjectHeaderId.GrappledGrappleBlock:
            case RoomObjectHeaderId.GrappledRespawningBreakableGrappleBlock:
            case RoomObjectHeaderId.GrappledBreakableGrappleBlock:
            case RoomObjectHeaderId.GrappledGenericSpikeBlock:
            case RoomObjectHeaderId.GrappledDraygonsBrokenTurret:
            case RoomObjectHeaderId.UnusedBlueBrinstarFaceBlock:
            case RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug:
            case RoomObjectHeaderId.UnusedShotBlock:
            case RoomObjectHeaderId.UnusedGrappleBlock:
            case RoomObjectHeaderId.LowerNorfairChozoHand:
            case RoomObjectHeaderId.CollisionLowerNorfairChozoHandCheck:
            case RoomObjectHeaderId.MotherBrainGlass:
            case RoomObjectHeaderId.BombTorizoHand:
            case RoomObjectHeaderId.WreckedShipChozoHand:
            case RoomObjectHeaderId.CollisionWreckedShipChozoHandCheck:
            case RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo:
            case RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo:
            case RoomObjectHeaderId.NoobTube:
            case RoomObjectHeaderId.SetMetroidsClearedStatesWhenRequired:
            case RoomObjectHeaderId.EyeDoorEyeFacingRight:
            case RoomObjectHeaderId.EyeDoorFacingRight:
            case RoomObjectHeaderId.EyeDoorBottomFacingRight:
            case RoomObjectHeaderId.EyeDoorEyeFacingLeft:
            case RoomObjectHeaderId.EyeDoorFacingLeft:
            case RoomObjectHeaderId.EyeDoorBottomFacingLeft:
            case RoomObjectHeaderId.DraygonCannonFacingRight:
            case RoomObjectHeaderId.DraygonCannonFacingRightDestroyed:
            case RoomObjectHeaderId.DraygonCannonFacingLeft:
            case RoomObjectHeaderId.DraygonCannonFacingLeftUnshielded:
            case RoomObjectHeaderId.ItemCollisionDetection:
            case RoomObjectHeaderId.ExposedEnergyTank:
            case RoomObjectHeaderId.ExposedMissileTank:
            case RoomObjectHeaderId.ExposedSuperMissileTank:
            case RoomObjectHeaderId.ExposedPowerBombTank:
            case RoomObjectHeaderId.ExposedBombs:
            case RoomObjectHeaderId.ExposedChargeBeam:
            case RoomObjectHeaderId.ExposedIceBeam:
            case RoomObjectHeaderId.ExposedHiJumpBoots:
            case RoomObjectHeaderId.ExposedSpeedBooster:
            case RoomObjectHeaderId.ExposedWaveBeam:
            case RoomObjectHeaderId.ExposedSpazerBeam:
            case RoomObjectHeaderId.ExposedSpringBall:
            case RoomObjectHeaderId.ExposedVariaSuit:
            case RoomObjectHeaderId.ExposedGravitySuit:
            case RoomObjectHeaderId.ExposedXrayScope:
            case RoomObjectHeaderId.ExposedPlasmaBeam:
            case RoomObjectHeaderId.ExposedGrappleBeam:
            case RoomObjectHeaderId.ExposedSpaceJump:
            case RoomObjectHeaderId.ExposedScrewAttack:
            case RoomObjectHeaderId.ExposedMorphBall:
            case RoomObjectHeaderId.ExposedReserveTank:
            case RoomObjectHeaderId.ChozoEnergyTank:
            case RoomObjectHeaderId.ChozoMissileTank:
            case RoomObjectHeaderId.ChozoSuperMissileTank:
            case RoomObjectHeaderId.ChozoPowerBombTank:
            case RoomObjectHeaderId.ChozoBombs:
            case RoomObjectHeaderId.ChozoChargeBeam:
            case RoomObjectHeaderId.ChozoIceBeam:
            case RoomObjectHeaderId.ChozoHiJumpBoots:
            case RoomObjectHeaderId.ChozoSpeedBooster:
            case RoomObjectHeaderId.ChozoWaveBeam:
            case RoomObjectHeaderId.ChozoSpazerBeam:
            case RoomObjectHeaderId.ChozoSpringBall:
            case RoomObjectHeaderId.ChozoVariaSuit:
            case RoomObjectHeaderId.ChozoGravitySuit:
            case RoomObjectHeaderId.ChozoXrayScope:
            case RoomObjectHeaderId.ChozoPlasmaBeam:
            case RoomObjectHeaderId.ChozoGrappleBeam:
            case RoomObjectHeaderId.ChozoSpaceJump:
            case RoomObjectHeaderId.ChozoScrewAttack:
            case RoomObjectHeaderId.ChozoMorphBall:
            case RoomObjectHeaderId.ChozoReserveTank:
            case RoomObjectHeaderId.ShotBlockEnergyTank:
            case RoomObjectHeaderId.ShotBlockMissileTank:
            case RoomObjectHeaderId.ShotBlockSuperMissileTank:
            case RoomObjectHeaderId.ShotBlockPowerBombTank:
            case RoomObjectHeaderId.ShotBlockBombs:
            case RoomObjectHeaderId.ShotBlockChargeBeam:
            case RoomObjectHeaderId.ShotBlockIceBeam:
            case RoomObjectHeaderId.ShotBlockHiJumpBoots:
            case RoomObjectHeaderId.ShotBlockSpeedBooster:
            case RoomObjectHeaderId.ShotBlockWaveBeam:
            case RoomObjectHeaderId.ShotBlockSpazerBeam:
            case RoomObjectHeaderId.ShotBlockSpringBall:
            case RoomObjectHeaderId.ShotBlockVariaSuit:
            case RoomObjectHeaderId.ShotBlockGravitySuit:
            case RoomObjectHeaderId.ShotBlockXrayScope:
            case RoomObjectHeaderId.ShotBlockPlasmaBeam:
            case RoomObjectHeaderId.ShotBlockGrappleBeam:
            case RoomObjectHeaderId.ShotBlockSpaceJump:
            case RoomObjectHeaderId.ShotBlockScrewAttack:
            case RoomObjectHeaderId.ShotBlockMorphBall:
            case RoomObjectHeaderId.ShotBlockReserveTank:
            default:
                throw new InvalidOperationException($"Undefined station header {slot.Header}.");
        }
    }

    /// <summary>Installs station and access blocks and selects the current animation for saved map state.</summary>
    private static void SetupStation(
        RoomLevelData level,
        SharedGameState system,
        AreaId area,
        RoomObjectSlot slot,
        StationKind kind)
    {
        RoomTerrainBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex).Terrain;
        switch (kind)
        {
            case StationKind.Save:
                SetupSaveStation(level, area, slot, original);
                return;
            case StationKind.Map:
                SetupAccessStation(level, system, area, slot, original, StationAccessRuleDefinitions.Map);
                return;
            case StationKind.Energy:
                SetupAccessStation(level, system, area, slot, original, StationAccessRuleDefinitions.Energy);
                return;
            case StationKind.Missile:
                SetupAccessStation(level, system, area, slot, original, StationAccessRuleDefinitions.Missile);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(StationKind)} {kind}.");
        }
    }

    /// <summary>A save station: its trigger floor and its one save animation.</summary>
    private static void SetupSaveStation(RoomLevelData level, AreaId area, RoomObjectSlot slot, RoomTerrainBlock original)
    {
        WriteStationBlock(
            level,
            slot.BlockIndex,
            original,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(StationAccessBehavior.SaveFloor)));
        slot.Family = new StationObjectState(
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
        RoomObjectSlot slot,
        RoomTerrainBlock original,
        StationAccessRule rule)
    {
        WriteStationBlock(level, slot.BlockIndex, original, RoomCollisionType.SolidBlock, behavior: null);
        WriteAccessBlock(level, slot.BlockIndex + rule.RightOffset, rule.RightBehavior);
        WriteAccessBlock(level, slot.BlockIndex + rule.LeftOffset, rule.LeftBehavior);

        StationAnimationList normalAnimationList = rule.Animation;
        StationAnimationList completedAnimationList = rule.AcquiredAnimation ?? normalAnimationList;
        StationAnimationList initialAnimationList = rule.AcquiredAnimation is not null && system.HasAreaMap(area)
            ? completedAnimationList
            : normalAnimationList;
        slot.Family = new StationObjectState(
            rule.Kind,
            area,
            initialAnimationList,
            completedAnimationList,
            animationFrameCount: 3);
    }

    /// <summary>Marks an in-room access block with station-trigger collision and its directional behaviour.</summary>
    private static void WriteAccessBlock(
        RoomLevelData level,
        int blockIndex,
        StationAccessBehavior behavior)
    {
        if (!level.IsLogicalBlockIndex(blockIndex))
        {
            throw new InvalidDataException(
                $"Station access setup targets out-of-room block index {blockIndex}.");
        }
        WriteStationBlock(
            level,
            blockIndex,
            level.GetCollisionBlockByIndex(blockIndex).Terrain,
            RoomCollisionType.SpecialBlock,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.StationAccesses.ReactionFor(behavior)));
    }

    /// <summary>Changes a station block's collision kind while preserving its appearance, and sets its optional behaviour.</summary>
    private static void WriteStationBlock(
        RoomLevelData level,
        int blockIndex,
        RoomTerrainBlock original,
        RoomCollisionType collisionType,
        RoomBlockBehavior? behavior)
    {
        level.SetTerrain(blockIndex, original.WithCollision(collisionType));
        if (behavior is { } written)
            level.SetBehavior(blockIndex, written);
    }

    /// <summary>The station collision notice room collision dispatch sends with the block's behaviour.</summary>
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

        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != parentBlockIndex || slot.Family is not StationObjectState station)
                continue;

            bool setupAccepted = setupGate == StationSetupGate.Bypassed || access switch
            {
                // Right-side access is entered while moving left into the wall in the ran-into-wall-left pose;
                // left-side access is the mirror in the ran-into-wall-right pose. These are the stations' setup tests
                // before cannon-height alignment.
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
                // reaches its column. Accepts only the latter, narrower range.
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
                // station therefore softlocks. The save trigger only advances its
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

    /// <summary>Applies the game's wrapping coordinate test before queuing any save side effect.</summary>
    private bool IsSaveStationTriggerCentered(int blockIndex, int roomWidthInBlocks)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roomWidthInBlocks);
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Save-station collision requires the room's Samus owner.");
        int probeX = CoordinateAxis.Wrap(samus.Position.X.Whole - SaveStationTriggerGeometry.HorizontalProbeOffset);
        return (probeX >> SaveStationTriggerGeometry.BlockCoordinateShift) == blockIndex % roomWidthInBlocks;
    }

    /// <summary>Advances station draw timers, access-arm phases, and activation messages during the PLM pass.</summary>
    internal bool TryStepStation(RoomLevelData level, RoomObjectSlot slot)
    {
        StationObjectState? station = slot.Family as StationObjectState;
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
                // fully inserted arm for 96 ticks, then activate the station. Command
                // six locked Samus in setup; this pass does not lock her again, so one an
                // intervening unpause freed stays free.
                station.OperationPhase = StationOperationPhase.Extending;
                station.OperationTimer = StationAccessMovementFrames;
                operationStartedThisPass = true;
                _sounds.Queue(EnemySound.StationEngaged, Priority: SoundRequestPriority.Normal);
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
            if (--station.OperationTimer == 0)
            {
                switch (station.OperationPhase)
                {
                    case StationOperationPhase.Extending:
                        DrawStationAccess(level, station, pose: StationAccessPose.Extended);
                        station.OperationPhase = StationOperationPhase.Extended;
                        station.OperationTimer = StationAccessExtendedHoldFrames;
                        break;
                    case StationOperationPhase.Extended:
                        PublishStationActivation(station, slot, samus);
                        // The recharge activations run Samus command one
                        // right after their message box, before the access retracts. The
                        // map activation leaves input locked for pause teardown's command.
                        if (station.Kind != StationKind.Map)
                            samus.Control.Unlock();
                        station.OperationPhase = StationOperationPhase.PostActivationHold;
                        station.OperationTimer = StationAccessMovementFrames;
                        break;
                    case StationOperationPhase.PostActivationHold:
                        _sounds.Queue(EnemySound.StationDisengaged, Priority: SoundRequestPriority.Normal);
                        station.OperationPhase = StationOperationPhase.Retracting;
                        station.OperationTimer = StationAccessMovementFrames;
                        break;
                    case StationOperationPhase.Retracting:
                        DrawStationAccess(level, station, pose: StationAccessPose.Retracted);
                        station.OperationPhase = StationOperationPhase.FinalRetractionHold;
                        station.OperationTimer = StationAccessMovementFrames;
                        break;
                    case StationOperationPhase.FinalRetractionHold:
                        station.OperationPhase = station.Kind == StationKind.Map && samus.Control.AlphaLocked
                            ? StationOperationPhase.AwaitingMapUnpause
                            : StationOperationPhase.Idle;
                        station.AccessBlockIndex = -1;
                        station.AccessBehavior = null;
                        // Every access list only deletes itself here. Recharges released
                        // input at activation; map pause teardown's release command owns its release.
                        break;
                    case StationOperationPhase.Idle:
                    case StationOperationPhase.AwaitingMapUnpause:
                    default:
                        throw new InvalidDataException(
                            $"Unknown station operation phase {station.OperationPhase}.");
                }
            }
        }

        // The save station draws once and then sleeps until its save-floor trigger advances
        // it. Its confirmation/save coroutine is published above for the frontend owner.
        if (station.Kind == StationKind.Save && station.InitialDrawCompleted)
            return true;

        if (--station.AnimationTimer != 0)
            return true;

        StationAnimationProgramDefinitions.Frame frame =
            StationAnimationPrograms.Resolve(
                station.AnimationList, station.AnimationFrame);
        int timer = frame.Duration;
        RoomObjectDraw draw = frame.Draw;
        if (timer is <= 0 or > short.MaxValue)
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
    private bool StepSaveStationAnimation(RoomLevelData level, RoomObjectSlot slot, StationObjectState station)
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

        if (--station.AnimationTimer != 0)
            return true;

        // The loop step runs when the second frame's four frames expire. After the last of
        // the 21 loops it falls through to the saved-game message, instead of
        // returning to the first frame. (The loop is counted at the second draw here.)
        if (station.AnimationFrame == 0 && station.SaveAnimationLoopsRemaining == 0)
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

    /// <summary>Draws the next of the two four-frame electricity frames.</summary>
    private void DrawSaveStationFrame(RoomLevelData level, int blockIndex, StationObjectState station)
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
        station.AnimationTimer = frame.Duration;
        station.AnimationFrame ^= 1;
        if (station.AnimationFrame == 0)
            station.SaveAnimationLoopsRemaining--;
    }

    /// <summary>Applies map or refill effects and publishes the station's frontend message.</summary>
    private void PublishStationActivation(
        StationObjectState station,
        RoomObjectSlot slot,
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
        station.AnimationTimer = 1;
        _stationActivationEvents.Add(new StationActivationEvent(
            station.Kind,
            message,
            station.AreaIndex,
            (slot.Argument as StationArgument)?.Station,
            slot.BlockIndex));
    }

    /// <summary>The number a station was placed with, which its activation events carry.</summary>
    private static int StationNumber(RoomObjectSlot slot) => (slot.Argument as StationArgument
        ?? throw new InvalidDataException($"Station at block {slot.BlockIndex} was placed without its number.")).Station;

    /// <summary>Draws the station access arm at its captured collision block in the short or extended pose.</summary>
    private void DrawStationAccess(RoomLevelData level, StationObjectState station, StationAccessPose pose)
    {
        StationAccessBehavior accessBehavior = station.AccessBehavior
            ?? throw new InvalidDataException("Station access has no behaviour owner.");
        StationAccessObjectDefinition definition =
            StationAccessObjects.Resolve(accessBehavior);
        _drawing.Draw(level, station.AccessBlockIndex, RoomObjectDraw.Of(pose == StationAccessPose.Extended ? definition.Extended : definition.Retracted));
    }

    /// <summary>Resident PLM state for station appearance, access-arm timing, and save confirmation.</summary>
    internal sealed class StationObjectState(
        StationKind kind,
        AreaId area,
        StationAnimationList animationList,
        StationAnimationList completedAnimationList,
        int animationFrameCount) : RoomObjectFamilyState
    {
        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.Station;

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
        public int AnimationTimer { get; set; } = 1;
        /// <summary>Keeps the save trigger asleep after its one initial draw.</summary>
        public bool InitialDrawCompleted { get; set; }
        /// <summary>Records accepted behaviour collision for the next PLM handler pass.</summary>
        public bool Triggered { get; set; }
        /// <summary>Current extension, hold, retraction, or map-unpause phase.</summary>
        public StationOperationPhase OperationPhase { get; set; }
        /// <summary>Ticks remaining in the current access-arm phase.</summary>
        public int OperationTimer { get; set; }
        /// <summary>Room block index where the activated access arm must be drawn.</summary>
        public int AccessBlockIndex { get; set; } = -1;
        /// <summary>Directional behaviour type identifying the selected access draw list.</summary>
        public StationAccessBehavior? AccessBehavior { get; set; }
        /// <summary>Tracks frontend confirmation and save animation separately from arm motion.</summary>
        public SaveStationPhase SavePhase { get; set; }
        /// <summary>Remaining two-frame save animation loops before the completion message.</summary>
        public int SaveAnimationLoopsRemaining { get; set; }
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
        /// <summary>The access actor has finished; the map-unpause release command still owns Samus release.</summary>
        AwaitingMapUnpause,
    }
}

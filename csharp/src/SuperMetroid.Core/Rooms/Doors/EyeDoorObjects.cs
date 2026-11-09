using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Eye-door PLMs (Tourian and Kraid lair): eye projectile spawning, hit reaction and conversion to a blue door.</summary>
[RoomLifetime]
internal sealed class EyeDoorObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the EyeDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.EyeDoor, static (_, _) => RoomObjectStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds eye-door PLM handling to room services and sound requests.</summary>
    internal EyeDoorObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomObjectEnemyEffects? enemies, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>Recognizes the eye, middle, and bottom headers for either door orientation.</summary>
    internal static bool IsEyeDoorHeader(RoomObjectHeaderId header) => header is
        RoomObjectHeaderId.EyeDoorEyeFacingRight or
        RoomObjectHeaderId.EyeDoorFacingRight or
        RoomObjectHeaderId.EyeDoorBottomFacingRight or
        RoomObjectHeaderId.EyeDoorEyeFacingLeft or
        RoomObjectHeaderId.EyeDoorFacingLeft or
        RoomObjectHeaderId.EyeDoorBottomFacingLeft;

    /// <summary>Initializes a door component and its closed collision blocks unless its persistent bit is set.</summary>
    internal void SetupEyeDoorSlot(
        RoomLevelData level,
        RoomObjectSlot slot)
    {
        (EyeDoorComponent component, EyeDoorOrientation orientation) = slot.Header switch
        {
            RoomObjectHeaderId.EyeDoorEyeFacingRight =>
                new EyeDoorPart(EyeDoorComponent.Eye, EyeDoorOrientation.Right),
            RoomObjectHeaderId.EyeDoorFacingRight =>
                new EyeDoorPart(EyeDoorComponent.Door, EyeDoorOrientation.Right),
            RoomObjectHeaderId.EyeDoorBottomFacingRight =>
                new EyeDoorPart(EyeDoorComponent.Bottom, EyeDoorOrientation.Right),
            RoomObjectHeaderId.EyeDoorEyeFacingLeft =>
                new EyeDoorPart(EyeDoorComponent.Eye, EyeDoorOrientation.Left),
            RoomObjectHeaderId.EyeDoorFacingLeft =>
                new EyeDoorPart(EyeDoorComponent.Door, EyeDoorOrientation.Left),
            RoomObjectHeaderId.EyeDoorBottomFacingLeft =>
                new EyeDoorPart(EyeDoorComponent.Bottom, EyeDoorOrientation.Left),
            RoomObjectHeaderId.None or
                RoomObjectHeaderId.Nothing or
                RoomObjectHeaderId.CollisionReactionClearCarry or
                RoomObjectHeaderId.RightwardsScrollExtension or
                RoomObjectHeaderId.LeftwardsScrollExtension or
                RoomObjectHeaderId.DownwardsScrollExtension or
                RoomObjectHeaderId.UpwardsScrollExtension or
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
                RoomObjectHeaderId.ShotBlockReserveTank => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.Header, "Not an eye-door PLM header."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.Header, "Not an eye-door PLM header."),
        };
        slot.Family = new EyeDoorObjectState(component);
        slot.StartAnimation(new EyeDoorObjectAnimation(orientation, component,
            component == EyeDoorComponent.Eye ? EyeDoorObjectProgram.EyeStart : EyeDoorObjectProgram.ComponentStart));

        SharedGameState system = _system ??
            throw new InvalidOperationException("Eye-door setup has no persistence owner.");
        if (slot.Argument is DoorBitArgument { Persists: true } openedEye &&
            system.HasOpenedDoor(openedEye.Door))
        {
            // Setup routines deliberately leave already-open terrain untouched. Their first
            // bytecode instruction takes the persistent branch and either deletes this
            // passive component or constructs the ordinary blue-door cap from the eye.
            return;
        }

        if (component == EyeDoorComponent.Eye)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.BlockIndex, EyeDoorDefinitions.EyeCollision,
                EyeDoorDefinitions.EyeBehavior);
            WriteExtensionRows(level, slot.BlockIndex, EyeDoorDefinitions.EyeExtensionRows);
            return;
        }

        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, slot.BlockIndex, EyeDoorDefinitions.ClosedComponentCollision,
            EyeDoorDefinitions.ClosedComponentBehavior);
    }

    /// <summary>Wakes opened components or consumes a pending missile hit before instruction dispatch.</summary>
    internal void RunEyeDoorFrameBehavior(RoomObjectSlot slot, EyeDoorFrameBehavior frameBehavior)
    {
        EyeDoorObjectState eyeDoor = slot.Family as EyeDoorObjectState
            ?? throw new InvalidOperationException("An eye door's wake ran without its component state.");
        SharedGameState system = _system ??
            throw new InvalidOperationException("A live eye door has no persistence owner.");
        switch (frameBehavior)
        {
            case EyeDoorFrameBehavior.WakeWhenDoorBitSet:
                if (slot.Argument is DoorBitArgument { Persists: true } wakingEye &&
                    system.HasOpenedDoor(wakingEye.Door))
                {
                    slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
                    slot.WakeAtLink();
                }
                return;

            case EyeDoorFrameBehavior.MissileHit:
                if (!eyeDoor.HasPendingHit)
                    return;

                SamusProjectileFamily? family = slot.ProjectileHit?.Family;
                slot.ProjectileHit = null;
                eyeDoor.HasPendingHit = false;
                if (family == SamusProjectileFamily.SuperMissile)
                {
                    // The Super Missile frame behaviour seeds the counter so the next increment meets the threshold.
                    eyeDoor.HitCounter = EyeDoorObjectProgramDefinitions.SuperMissileHitCounter;
                }
                else if (family != SamusProjectileFamily.Missile)
                {
                    _sounds.Queue(
                        EyeDoorDefinitions.RejectedShotSound,
                        Priority: SoundRequestPriority.Normal);
                    return;
                }

                slot.WakeAtLink();
                return;

            default:
                throw new InvalidOperationException($"Undefined EyeDoorFrameBehavior {frameBehavior}.");
        }
    }

    /// <summary>Whether Samus stands within <paramref name="reach"/> blocks across and up or down of the PLM.</summary>
    internal bool IsSamusNear(RoomLevelData level, RoomObjectSlot slot, Extent16<RoomBlocks> reach)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException("Eye-door proximity requires Samus.");
        int roomObjectX = slot.BlockIndex % level.Size.X;
        int roomObjectY = slot.BlockIndex / level.Size.X;
        int deltaX = Math.Abs((samus.Position.X.Whole >> 4) - roomObjectX);
        int deltaY = Math.Abs((samus.Position.Y.Whole >> 4) - roomObjectY);
        return deltaX <= reach.X && deltaY <= reach.Y;
    }

    /// <summary>The eye facing <paramref name="facing"/> fires its projectile and queues its sound.</summary>
    internal void Shoot(RoomObjectSlot slot, EyeDoorOrientation facing)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Projectile, facing);
        _sounds.Queue(
            EyeDoorDefinitions.ProjectileSound,
            Priority: SoundRequestPriority.Normal);
    }

    /// <summary>The eye facing <paramref name="facing"/> sweats.</summary>
    internal void SpawnSweat(RoomObjectSlot slot, EyeDoorOrientation facing) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Sweat, facing);

    /// <summary>Two randomly placed smoke puffs.</summary>
    internal void SpawnTwoSmoke(RoomObjectSlot slot, EyeDoorOrientation facing)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.ScatteredSmoke, facing);
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.ScatteredSmoke, facing);
    }

    /// <summary>One centred smoke puff.</summary>
    internal void SpawnSmoke(RoomObjectSlot slot, EyeDoorOrientation facing) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.CenteredSmoke, facing);

    /// <summary>Counts a hit on the eye; true when it opens the door.</summary>
    internal static bool CountEyeHit(RoomObjectSlot slot, DoorObjects doors)
    {
        EyeDoorObjectState eye = (slot.Family as EyeDoorObjectState)
            ?? throw new InvalidOperationException("An eye's hit reached a slot without its eye-door state.");
        eye.HitCounter++;
        if (eye.HitCounter < EyeDoorObjectProgramDefinitions.HitThreshold)
            return false;
        doors.MarkDoorOpened(slot);
        return true;
    }

    /// <summary>
    /// Moves the eye PLM up one block and writes the blue cap facing
    /// <paramref name="orientation"/> with its extension rows.
    /// </summary>
    internal static void ConvertEyeToBlueDoor(RoomLevelData level, RoomObjectSlot slot, EyeDoorOrientation orientation)
    {
        RoomBlockBehavior facing = orientation switch
        {
            EyeDoorOrientation.Left => RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)),
            EyeDoorOrientation.Right => RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Right)),
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        slot.BlockIndex = checked(slot.BlockIndex - level.Size.X);
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
            level,
            slot.BlockIndex,
            RoomCollisionType.ShootableBlock,
            facing);
        WriteExtensionRows(level, slot.BlockIndex, EyeDoorDefinitions.BlueDoorExtensionCount);
    }

    /// <summary>
    /// Writes <paramref name="rows"/> vertical extension blocks below <paramref name="originIndex"/>, each pointing back
    /// up to the origin block.
    /// </summary>
    private static void WriteExtensionRows(RoomLevelData level, int originIndex, int rows)
    {
        for (int row = 1; row <= rows; row++)
        {
            RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(
                level,
                checked(originIndex + row * level.Size.X),
                RoomCollisionType.VerticalExtension,
                RoomBlockBehavior.Extending(-row));
        }
    }

    /// <summary>Sends an eye-door projectile request with the eye's facing, source block and room argument to enemy services.</summary>
    private void SpawnEyeDoorProjectile(
        RoomObjectSlot slot,
        EyeDoorProjectileDefinition definition,
        EyeDoorOrientation facing)
    {
        IEyeDoorProjectileSpawner enemies = _enemies ??
            throw new InvalidOperationException("Eye-door program has no enemy-projectile spawner.");
        enemies.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(
            definition,
            facing,
            slot.BlockIndex,
            (slot.Argument as DoorBitArgument)?.Door));
    }

    /// <summary>Per-component hit state retained across eye-door instruction frames.</summary>
    internal sealed class EyeDoorObjectState(EyeDoorComponent component) : RoomObjectFamilyState, IShotTriggeredObject
    {
        /// <summary>Only the eye takes the shot; the middle and bottom components ignore it.</summary>
        public void RecordShot()
        {
            if (Component == EyeDoorComponent.Eye)
                HasPendingHit = true;
        }

        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.EyeDoor;

        /// <summary>Identifies whether this slot is the eye, middle, or bottom component.</summary>
        public EyeDoorComponent Component { get; } = component;
        /// <summary>Missile-hit count used by the door's threshold step. It only counts up, by one per hit or to 120 on a Super Missile hit, and the door opens once it reaches its threshold, so it stays far below the 255 the original byte counter could hold.</summary>
        public int HitCounter { get; set; }
        /// <summary>Whether a projectile collision awaits the eye's missile pre-instruction.</summary>
        public bool HasPendingHit { get; set; }
    }
}

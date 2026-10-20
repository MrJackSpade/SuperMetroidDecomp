using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>Eye-door PLMs (Tourian and Kraid lair): eye projectile spawning, hit reaction and conversion to a blue door.</summary>
[RoomLifetime]
internal sealed class EyeDoorPlms : IPlmFamilySource
{
    /// <summary>Registers the EyeDoor family, which has no step of its own: its slots take the shared path.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.EyeDoor, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>Supplies Samus, persistence bits, and enemy projectile spawning to eye-door bytecode.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Queues rejected-shot and launched-projectile sounds.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds eye-door PLM handling to room services and sound requests.</summary>
    internal EyeDoorPlms(PlmRoomBindings bindings, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _sounds = sounds;
    }

    /// <summary>Recognizes the eye, middle, and bottom headers for either door orientation.</summary>
    internal static bool IsEyeDoorHeader(PlmHeaderId header) => header is
        PlmHeaderId.EyeDoorEyeFacingRight or
        PlmHeaderId.EyeDoorFacingRight or
        PlmHeaderId.EyeDoorBottomFacingRight or
        PlmHeaderId.EyeDoorEyeFacingLeft or
        PlmHeaderId.EyeDoorFacingLeft or
        PlmHeaderId.EyeDoorBottomFacingLeft;

    /// <summary>Initializes a door component and its closed collision blocks unless its persistent bit is set.</summary>
    internal void SetupEyeDoorSlot(
        RoomLevelData level,
        PlmSlot slot)
    {
        (EyeDoorComponent component, EyeDoorOrientation orientation) = slot.HeaderPointer switch
        {
            PlmHeaderId.EyeDoorEyeFacingRight =>
                (EyeDoorComponent.Eye, EyeDoorOrientation.Right),
            PlmHeaderId.EyeDoorFacingRight =>
                (EyeDoorComponent.Door, EyeDoorOrientation.Right),
            PlmHeaderId.EyeDoorBottomFacingRight =>
                (EyeDoorComponent.Bottom, EyeDoorOrientation.Right),
            PlmHeaderId.EyeDoorEyeFacingLeft =>
                (EyeDoorComponent.Eye, EyeDoorOrientation.Left),
            PlmHeaderId.EyeDoorFacingLeft =>
                (EyeDoorComponent.Door, EyeDoorOrientation.Left),
            PlmHeaderId.EyeDoorBottomFacingLeft =>
                (EyeDoorComponent.Bottom, EyeDoorOrientation.Left),
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
                nameof(slot), slot.HeaderPointer, "Not an eye-door PLM header."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(slot), slot.HeaderPointer, "Not an eye-door PLM header."),
        };
        slot.Family = new EyeDoorPlmState(component);
        slot.StartAnimation(new EyeDoorPlmAnimation(orientation, component,
            component == EyeDoorComponent.Eye ? EyeDoorPlmProgram.EyeStart : EyeDoorPlmProgram.ComponentStart));

        Bank80SystemState system = _bindings.System ??
            throw new InvalidOperationException("Eye-door setup has no persistence owner.");
        if (unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasOpenedDoorBit(slot.RoomArgument))
        {
            // Setup routines deliberately leave already-open terrain untouched. Their first
            // bytecode instruction takes the persistent branch and either deletes this
            // passive component or constructs the ordinary blue-door cap from the eye.
            return;
        }

        if (component == EyeDoorComponent.Eye)
        {
            RoomPlmSystem.WritePlmCollisionTypeAndBts(level, slot.BlockIndex, EyeDoorPlmRomData.EyeCollisionWord);
            RoomPlmSystem.WritePlmCollisionTypeAndBts(
                level,
                checked(slot.BlockIndex + level.Size.X),
                EyeDoorPlmRomData.EyeExtensionWord);
            return;
        }

        RoomPlmSystem.WritePlmCollisionTypeAndBts(level, slot.BlockIndex, EyeDoorPlmRomData.ClosedComponentWord);
    }

    /// <summary>Wakes opened components or consumes a pending missile hit before instruction dispatch.</summary>
    internal void RunEyeDoorPreInstruction(PlmSlot slot, EyeDoorPreInstruction preInstruction)
    {
        EyeDoorPlmState eyeDoor = slot.Family as EyeDoorPlmState
            ?? throw new InvalidOperationException("An eye door's wake ran without its component state.");
        Bank80SystemState system = _bindings.System ??
            throw new InvalidOperationException("A live eye door has no persistence owner.");
        switch (preInstruction)
        {
            case EyeDoorPreInstruction.WakeWhenDoorBitSet:
                if (unchecked((short)slot.RoomArgument) >= 0 &&
                    system.HasOpenedDoorBit(slot.RoomArgument))
                {
                    slot.PreInstruction = PlmPreInstructions.Inert;
                    slot.WakeAtLink();
                }
                return;

            case EyeDoorPreInstruction.MissileHit:
                if (!eyeDoor.HasPendingHit)
                    return;

                SamusProjectileFamily family = new SamusProjectileTypeWord(checked((ushort)slot.LoopTimer)).Family;
                slot.LoopTimer = 0;
                eyeDoor.HasPendingHit = false;
                if (family == SamusProjectileFamily.SuperMissile)
                {
                    // $BD50 seeds the counter so the next increment meets the threshold.
                    eyeDoor.HitCounter = EyeDoorPlmProgramDefinitions.SuperMissileHitCounter;
                }
                else if (family != SamusProjectileFamily.Missile)
                {
                    _sounds.Queue(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            EyeDoorPlmRomData.RejectedShotSound),
                        MaximumQueued: 6);
                    return;
                }

                slot.WakeAtLink();
                return;

            default:
                throw new InvalidOperationException($"Undefined EyeDoorPreInstruction {preInstruction}.");
        }
    }

    /// <summary><c>$84:8D41</c>: whether Samus stands within <paramref name="reach"/> blocks across and up or down of the PLM.</summary>
    internal bool IsSamusNear(RoomLevelData level, PlmSlot slot, Extent16<RoomBlocks> reach)
    {
        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException("Eye-door proximity requires Samus.");
        int plmX = slot.BlockIndex % level.Size.X;
        int plmY = slot.BlockIndex / level.Size.X;
        int deltaX = Math.Abs((samus.Position.X.Whole >> 4) - plmX);
        int deltaY = Math.Abs((samus.Position.Y.Whole >> 4) - plmY);
        return deltaX <= reach.X && deltaY <= reach.Y;
    }

    /// <summary><c>$84:D77A</c>: fires an eye-door projectile and queues its sound.</summary>
    internal void Shoot(PlmSlot slot, ushort parameter)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Projectile, parameter);
        _sounds.Queue(
            SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, EyeDoorPlmRomData.ProjectileSound),
            MaximumQueued: 6);
    }

    /// <summary><c>$84:D790</c>: the eye sweats.</summary>
    internal void SpawnSweat(PlmSlot slot, ushort parameter) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Sweat, parameter);

    /// <summary><c>$84:D79F</c>: two randomly placed smoke puffs.</summary>
    internal void SpawnTwoSmoke(PlmSlot slot)
    {
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Smoke, EyeDoorPlmRomData.RandomizedSmokeParameter);
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Smoke, EyeDoorPlmRomData.RandomizedSmokeParameter);
    }

    /// <summary><c>$84:D7B6</c>: one centred smoke puff.</summary>
    internal void SpawnSmoke(PlmSlot slot) =>
        SpawnEyeDoorProjectile(slot, EyeDoorProjectileDefinition.Smoke, EyeDoorPlmRomData.CenteredSmokeParameter);

    /// <summary><c>$84:8A91</c>: counts a hit on the eye; true when it opens the door.</summary>
    internal static bool CountEyeHit(PlmSlot slot, DoorPlms doors)
    {
        EyeDoorPlmState eye = (slot.Family as EyeDoorPlmState)
            ?? throw new InvalidOperationException("An eye's hit reached a slot without its eye-door state.");
        eye.HitCounter++;
        if (eye.HitCounter < EyeDoorPlmProgramDefinitions.HitThreshold)
            return false;
        doors.MarkDoorOpened(slot);
        return true;
    }

    /// <summary>
    /// <c>$84:D7C3</c> (right) / <c>$84:D7DA</c> (left): moves the eye PLM up one block and writes the blue cap facing
    /// <paramref name="orientation"/> with its extension rows.
    /// </summary>
    internal static void ConvertEyeToBlueDoor(RoomLevelData level, PlmSlot slot, EyeDoorOrientation orientation)
    {
        RoomBlockBehavior facing = orientation switch
        {
            EyeDoorOrientation.Left => RoomBlockBehaviorValues.BlueDoorFacingLeft,
            EyeDoorOrientation.Right => RoomBlockBehaviorValues.BlueDoorFacingRight,
            _ => throw new InvalidOperationException($"Undefined {nameof(EyeDoorOrientation)} {orientation}."),
        };
        slot.BlockIndex = checked(slot.BlockIndex - level.Size.X);
        RoomPlmSystem.WritePlmCollisionTypeAndBts(
            level,
            slot.BlockIndex,
            RoomCollisionType.ShootableBlock,
            facing.Value);
        for (int row = 1; row <= EyeDoorPlmRomData.BlueDoorExtensionCount; row++)
        {
            RoomPlmSystem.WritePlmCollisionTypeAndBts(
                level,
                checked(slot.BlockIndex + row * level.Size.X),
                RoomCollisionType.VerticalExtension,
                unchecked((byte)-row));
        }
    }

    /// <summary>Sends an eye-door projectile request with the source block and room argument to enemy services.</summary>
    private void SpawnEyeDoorProjectile(
        PlmSlot slot,
        EyeDoorProjectileDefinition definitionPointer,
        ushort parameter)
    {
        IEyeDoorProjectileSpawner enemies = _bindings.Enemies ??
            throw new InvalidOperationException("Eye-door bytecode has no bank-$86 spawn owner.");
        enemies.SpawnEyeDoorProjectile(new EyeDoorProjectileRequest(
            definitionPointer,
            parameter,
            slot.BlockIndex,
            slot.RoomArgument));
    }

    /// <summary>Per-component hit state retained across eye-door instruction frames.</summary>
    internal sealed class EyeDoorPlmState(EyeDoorComponent component) : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.EyeDoor;

        /// <summary>Identifies whether this slot is the eye, middle, or bottom component.</summary>
        public EyeDoorComponent Component { get; } = component;
        /// <summary>Missile-hit count used by the door's threshold instruction. It only counts up, by one per hit or to 120 on a Super Missile hit, and the door opens once it reaches its threshold, so it stays far below the 255 the cartridge's byte could hold.</summary>
        public int HitCounter { get; set; }
        /// <summary>Whether a projectile collision awaits the eye's missile pre-instruction.</summary>
        public bool HasPendingHit { get; set; }
    }
}

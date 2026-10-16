using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Enemies.PipeBugs;
using System.Buffers.Binary;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.NativeSnapshotImportMotherBrainVerification;
using static SuperMetroid.Verification.Harness.NativeSnapshotImportPauseVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Harness;
using SuperMetroid.Verification.Samus.Projectiles;

/// <summary>
/// Constructs a replay's initial production state from native WRAM and SRAM while
/// rejecting unsupported live state instead of repairing later replay divergence.
/// </summary>
internal static class NativeSnapshotImportVerification
{
    /// <summary>
    /// Builds a production game from one native WRAM snapshot. This is the single initial-state
    /// import a snapshot-start movie replay permits; nothing native is fed back afterwards.
    /// Unsupported live state is refused rather than approximated.
    /// </summary>
    /// <param name="memory">The 128 KiB native WRAM image at the movie's first input boundary.</param>
    /// <param name="saveRam">The movie's starting 8 KiB cartridge SRAM.</param>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static (CartridgeImportAddressSpace Bus, SuperMetroidRuntime Runtime, SamusState Samus, SuperMetroidGame Game)
        ImportNativeSnapshot(byte[] memory, byte[] saveRam)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SaveRamImage.Load(bus, saveRam);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Options.MoonwalkEnabled = W(NativeSnapshotMemory.MoonwalkOption) != 0;
        runtime.System.LoadCollectedItemBytes(memory.AsSpan(NativeSnapshotMemory.CollectedItemBits, Bank80SystemState.ItemBitByteCount));
        runtime.System.LoadBossBytes(memory.AsSpan(NativeSnapshotMemory.BossBits, Bank80SystemState.AreaCount));
        runtime.System.LoadEventBytes(memory.AsSpan(NativeSnapshotMemory.Events, Bank80SystemState.EventByteCount));
        runtime.System.LoadOpenedDoorBytes(memory.AsSpan(NativeSnapshotMemory.OpenedDoors, Bank80SystemState.DoorBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(NativeSnapshotMemory.Room), new Position16<RoomPixels>(W(NativeSnapshotMemory.CameraX), W(NativeSnapshotMemory.CameraY)));
        ScrollBoundaryCamera camera = runtime.Room.LoadedCamera;
        // The room load wrote only the integer layer-one words; import the native fractions,
        // ideal position and speed words as whole camera values.
        camera.Position = camera.Position.WithFractionsOf(FixedPosition<RoomPixels>.FromWords(
                0, W(NativeSnapshotMemory.CameraXFraction), 0, W(NativeSnapshotMemory.CameraYFraction)));
        camera.IdealPosition = new Position16<RoomPixels>(W(NativeSnapshotMemory.IdealCameraX), W(NativeSnapshotMemory.IdealCameraY));
        camera.Speed = FixedVector<RoomPixels>.FromWords(
                W(NativeSnapshotMemory.CameraSpeedX), W(NativeSnapshotMemory.CameraSpeedXFraction),
                W(NativeSnapshotMemory.CameraSpeedY), W(NativeSnapshotMemory.CameraSpeedYFraction));
        runtime.Room.LoadedCamera.FinishSamusScrolling(FixedPosition<RoomPixels>.FromWords(
            W(NativeSnapshotMemory.PreviousSamusX), W(NativeSnapshotMemory.PreviousSamusXFraction),
            W(NativeSnapshotMemory.PreviousSamusY), W(NativeSnapshotMemory.PreviousSamusYFraction)));
        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.Room.LevelData ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load room collision data.");
        for (int index = 0; index < level.Size.X * level.Size.Y; index++)
        {
            level.SetForegroundEntry(index, W(NativeSnapshotMemory.Level + index * sizeof(ushort)));
            level.SetBehavior(index, memory[NativeSnapshotMemory.Bts + index]);
        }

        runtime.Cgram.LoadBytes(memory.AsSpan(NativeSnapshotMemory.PaletteBuffer, CgramPalettes.ByteCount));
        var initialPaletteSlots = runtime.RoomPaletteFx.slots;
        for (int slot = 0; slot < initialPaletteSlots.Length; slot++)
        {
            var paletteSlot = initialPaletteSlots[slot];
            paletteSlot.Id = W(NativeSnapshotMemory.PaletteFxId + slot * 2);
            paletteSlot.ColorByteIndex = W(NativeSnapshotMemory.PaletteFxColor + slot * 2);
            paletteSlot.PreInstruction = W(NativeSnapshotMemory.PaletteFxPreInstruction + slot * 2);
            // A live object resumes its typed program where the recorded native pointer stands.
            paletteSlot.Cursor = paletteSlot.Id == 0
                ? default
                : SuperMetroid.Verification.Fx.PaletteFx.PaletteFxProgramsVerification.CursorAt(
                    SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W(NativeSnapshotMemory.PaletteFxInstruction + slot * 2));
            paletteSlot.InstructionTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.PaletteFxInstructionTimer + slot * 2));
            paletteSlot.Timer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.PaletteFxTimer + slot * 2));
        }
        runtime.RoomPaletteFx.samusInHeatPaletteIndex = W(NativeSnapshotMemory.HeatPalettePhase);
        runtime.RoomPaletteFx.previousSamusInHeatPaletteIndex = W(NativeSnapshotMemory.PreviousHeatPalettePhase);
        ushort[] initialHud = runtime.Hud._tiles;
        for (int index = 0; index < initialHud.Length; index++)
            initialHud[index] = W(NativeSnapshotMemory.HudTilemap + index * 2);
        runtime.Hud._previousSelectedItem = W(NativeSnapshotMemory.PreviousHudSelection);
        byte[] initialScrolls = runtime.Room.LoadedCamera.Scrolls._cells;
        memory.AsSpan(NativeSnapshotMemory.ScrollStorage, RoomScrollGrid.StorageByteCount).CopyTo(initialScrolls);
        WorkRamByteTable scrollZones = new RoomScrollMemory(bus).Zones;
        for (int index = 0; index < initialScrolls.Length; index++)
            scrollZones[index] = initialScrolls[index];
        runtime.Hud.MinimapDisabled = W(NativeSnapshotMemory.MinimapDisabled) != 0;
        byte[] initialMaps = memory.AsSpan(NativeSnapshotMemory.SavedExploredMaps,
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea).ToArray();
        memory.AsSpan(NativeSnapshotMemory.LiveExploredMap, Bank80SystemState.ExploredMapBytesPerArea)
            .CopyTo(initialMaps.AsSpan(W(NativeSnapshotMemory.CurrentArea) * Bank80SystemState.ExploredMapBytesPerArea));
        runtime.System.LoadExploredMapBytes(initialMaps);
        runtime.System.LoadUsedSaveStationBytes(memory.AsSpan(NativeSnapshotMemory.SaveElevatorMarkers, Bank80SystemState.UsedSaveStationByteCount));
        runtime.System.LoadMapStationBytes(memory.AsSpan(NativeSnapshotMemory.MapStationMarkers, Bank80SystemState.MapStationByteCount));

        SamusState samus = runtime.Player.Samus ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load Samus.");
        samus.InputLocked = false;
        samus.Presentation.TopSpritemapIndex = W(NativeSnapshotMemory.SamusTopSpritemap);
        samus.Presentation.BottomSpritemapIndex = W(NativeSnapshotMemory.SamusBottomSpritemap);
        samus.Presentation.SpritemapPosition = new Position16<ScreenPixels>(W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY));
        samus.ArmCannon.Frame = W(NativeSnapshotMemory.CannonFrame);
        samus.ArmCannon.ToggleFlag = W(NativeSnapshotMemory.CannonToggle);
        samus.ArmCannon.DrawingMode = W(NativeSnapshotMemory.CannonDrawingMode);
        samus.ArmCannon.OpenFlag = memory[NativeSnapshotMemory.CannonFlags];
        samus.ArmCannon.CloseFlag = memory[NativeSnapshotMemory.CannonFlags + 1];
        new ProjectileInheritanceRecordsAccess(bus).Load(memory.AsSpan(NativeSnapshotMemory.ProjectileInheritancePrefix,
            NativeSnapshotMemory.SamusSlopeAdjusted - NativeSnapshotMemory.ProjectileInheritancePrefix));
        samus.Kinematics.PositionAdjustedBySlope = W(NativeSnapshotMemory.SamusSlopeAdjusted) != 0;
        for (int direction = 0; direction < 4; direction++)
            samus.Kinematics.RecordSolidEnemyCollision((SamusCollisionDirection)direction,
                W(NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2));
        samus.HorizontalSpeed.ActiveSpeedTableBaseAddress = W(NativeSnapshotMemory.HorizontalSpeedTable);
        samus.HorizontalSpeed.DecelerationMultiplier = memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier];
        samus.HorizontalSpeed.EchoSoundFlag = W(NativeSnapshotMemory.SpeedEchoSoundLatch);
        samus.HorizontalSpeed.TotalSpeed = W(NativeSnapshotMemory.TotalHorizontalSpeed);
        samus.HorizontalSpeed.TotalSubspeed = W(NativeSnapshotMemory.TotalHorizontalSubspeed);
        runtime.Projectiles.SlotTable.ProjectileCounter = W(NativeSnapshotMemory.ProjectileCount);
        runtime.Projectiles.Charge.PreviousBeamChargeCounter = W(NativeSnapshotMemory.PreviousCharge);
        runtime.Projectiles.Firing.ProjectileInvincibilityTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.ProjectileInteractionImmunity));
        runtime.Projectiles.Charge.ChargedShotGlowTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.ChargedShotGlow));
        runtime.Projectiles.Charge.SamusChargePaletteIndex = W(NativeSnapshotMemory.ChargePaletteIndex);
        runtime.BombProjectiles.BombCounter = W(NativeSnapshotMemory.BombCount);
        samus.BombSpreadChargeTimeoutCounter = W(NativeSnapshotMemory.BombSpreadChargeTimeout);
        samus.PoseTransitionShotDirection = W(NativeSnapshotMemory.PoseShotDirection);
        samus.HyperBeam = W(NativeSnapshotMemory.HyperBeam);
        samus.ResumeChargingBeamSoundFlag = W(NativeSnapshotMemory.ResumeChargeSound);
        samus.InputSnapshot.PreviousDrawHeldInput = (SnesButton)W(NativeSnapshotMemory.SamusFilteredHeld);
        samus.InputSnapshot.PreviousDrawNewInput = (SnesButton)W(NativeSnapshotMemory.SamusFilteredNew);
        samus.InputSnapshot.AutoJumpTimer = W(NativeSnapshotMemory.SamusAutoJumpTimer);
        samus.PreviousHealthForHurtCheck = W(NativeSnapshotMemory.SamusPreviousHealthForFlash);
        AssertTrue(W(NativeSnapshotMemory.SamusInputHandler) is
            NativeSnapshotMemory.SamusNormalInputHandler or NativeSnapshotMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        samus.InputSnapshot.AutoJumpInputPending = W(NativeSnapshotMemory.SamusInputHandler) == NativeSnapshotMemory.SamusAutoJumpInputHandler;
        samus.EquippedItems = W(NativeSnapshotMemory.Items);
        samus.EquippedBeams = W(NativeSnapshotMemory.Beams);
        samus.Health = W(NativeSnapshotMemory.Health);
        samus.MaxHealth = W(NativeSnapshotMemory.MaxHealth);
        samus.InvincibilityTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.InvincibilityTimer));
        samus.KnockbackTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.KnockbackTimer));
        samus.KnockbackDirection = W(NativeSnapshotMemory.KnockbackDirection);
        samus.KnockbackXDirection = W(NativeSnapshotMemory.KnockbackXDirection);
        samus.HurtFlashCounter = W(NativeSnapshotMemory.HurtFlashCounter);
        samus.SubunitHealth = W(NativeSnapshotMemory.SubunitHealth);
        samus.SelectedHudItem = W(NativeSnapshotMemory.SelectedHudItem);
        samus.ArmCannon._previousSelectedHudItem = samus.SelectedHudItem;
        samus.AutoCancelHudItemIndex = W(NativeSnapshotMemory.AutoCancelHudItemIndex);
        samus.ReserveTankMode = W(NativeSnapshotMemory.ReserveMode);
        samus.MaxReserveEnergy = W(NativeSnapshotMemory.MaxReserve);
        samus.ReserveEnergy = W(NativeSnapshotMemory.Reserve);
        samus.Pose = (SamusPoseId)W(NativeSnapshotMemory.Pose);
        samus.Position = samus.Position.WithWhole(new(W(NativeSnapshotMemory.X), W(NativeSnapshotMemory.Y)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(W(NativeSnapshotMemory.XFraction)).WithFractionY(W(NativeSnapshotMemory.YFraction));
        samus.PoseMetadata.RefreshCollisionRadii(bus);
        samus.Presentation.InitializeAnimation(bus);
        samus.Presentation.SetAnimationFrameFromSpecialHandler(W(NativeSnapshotMemory.Animation), W(NativeSnapshotMemory.AnimationTimer));
        samus.PoseHistory.PreviousPose = (SamusPoseId)W(NativeSnapshotMemory.PreviousPose);
        samus.PoseHistory.PreviousDirectionAndMovement = W(NativeSnapshotMemory.PreviousDirection);
        samus.PoseHistory.LastDifferentPose = (SamusPoseId)W(NativeSnapshotMemory.LastDifferentPose);
        samus.PoseHistory.LastDifferentDirectionAndMovement = W(NativeSnapshotMemory.LastDifferentDirection);
        samus.HorizontalSpeed.BaseSpeed = W(NativeSnapshotMemory.BaseSpeed);
        samus.HorizontalSpeed.BaseSubspeed = W(NativeSnapshotMemory.BaseFraction);
        samus.HorizontalSpeed.ExtraRunSpeed = W(NativeSnapshotMemory.ExtraSpeed);
        samus.HorizontalSpeed.ExtraRunSubspeed = W(NativeSnapshotMemory.ExtraFraction);
        samus.HorizontalSpeed.AccelerationMode = W(NativeSnapshotMemory.AccelerationMode);
        samus.HorizontalSpeed.HasRunningMomentum = W(NativeSnapshotMemory.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = W(NativeSnapshotMemory.BoostCounter);
        samus.Kinematics.YSpeed = W(NativeSnapshotMemory.VerticalSpeed);
        samus.Kinematics.YSubspeed = W(NativeSnapshotMemory.VerticalFraction);
        samus.Kinematics.YDirection = ClosedNativeWords.Decode<SamusVerticalDirection>(
            W(NativeSnapshotMemory.VerticalDirection), "Samus vertical direction");
        samus.Kinematics.Radius = new(W(NativeSnapshotMemory.SamusXRadius), W(NativeSnapshotMemory.SamusYRadius));
        samus.Kinematics.YAcceleration = W(NativeSnapshotMemory.Gravity);
        samus.Kinematics.YSubacceleration = W(NativeSnapshotMemory.GravityFraction);
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWords(W(NativeSnapshotMemory.ExtraXDisplacement), W(NativeSnapshotMemory.ExtraXDisplacementFraction), W(NativeSnapshotMemory.ExtraYDisplacement), W(NativeSnapshotMemory.ExtraYDisplacementFraction));
        samus.Kinematics.HorizontalSlopeCollisionEnable = W(NativeSnapshotMemory.SlopeCollisionEnable);
        samus.HorizontalSpeed.SpeedDivisor = W(NativeSnapshotMemory.SpeedDivisor);
        samus.HorizontalSpeed.ContactDamageIndex = ClosedNativeWords.Decode<SamusContactDamageIndex>(
            W(NativeSnapshotMemory.ContactDamageIndex), "Samus contact damage index");
        samus.MorphBallBounceState = ClosedNativeWords.Decode<SamusBallBounceState>(
            W(NativeSnapshotMemory.MorphBallBounceState), "Samus morph-ball bounce state");
        samus.BombJumpDirection = W(NativeSnapshotMemory.BombJumpDirection);

        foreach (var trail in runtime.Projectiles.TrailSlots)
        {
            trail.Left.InstructionTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.TrailLeftInstructionTimer + trail.NativeByteIndex));
            trail.Right.InstructionTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.TrailRightInstructionTimer + trail.NativeByteIndex));
            trail.Left.InstructionPointer = W(NativeSnapshotMemory.TrailLeftInstructionPointer + trail.NativeByteIndex);
            trail.Right.InstructionPointer = W(NativeSnapshotMemory.TrailRightInstructionPointer + trail.NativeByteIndex);
            trail.Left.TileNumberAttributes = W(NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex);
            trail.Right.TileNumberAttributes = W(NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex);
            trail.Left.Position = new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailLeftXPosition + trail.NativeByteIndex), W(NativeSnapshotMemory.TrailLeftYPosition + trail.NativeByteIndex));
            trail.Right.Position = new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailRightXPosition + trail.NativeByteIndex), W(NativeSnapshotMemory.TrailRightYPosition + trail.NativeByteIndex));
        }

        for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
        {
            ushort packed = W(NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
            samus.LiquidPhysics.AtmosphericEffects.SetSlot(index, SnesLittleEndian.HighByte(packed), (byte)packed,
                W(NativeSnapshotMemory.AtmosphericTimer + index * 2),
                new(W(NativeSnapshotMemory.AtmosphericX + index * 2), W(NativeSnapshotMemory.AtmosphericY + index * 2)));
        }
        samus.AnimationFrameBuffer = W(NativeSnapshotMemory.AnimationFrameBuffer);
        samus.LiquidPhysics.LiquidPhysicsType = ClosedNativeWords.Decode<SamusLiquidMedium>(W(NativeSnapshotMemory.LiquidPhysicsType), "liquid physics type");
        samus.LiquidPhysics.PeriodicSubDamage = W(NativeSnapshotMemory.PeriodicSubDamage);
        samus.LiquidPhysics.PeriodicDamage = W(NativeSnapshotMemory.PeriodicDamage);

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialPlms = runtime.Plms.SlotTable._slots;
        for (int index = 0; index < initialPlms.Length; index++)
        {
            AssertTrue(W(NativeSnapshotMemory.PlmHeaders + index * 2) == 0, "native initial PLM pool is empty");
            initialPlms[index].Active = false;
        }
        foreach (var projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            ImportNativeEnemyProjectile(projectile, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W);
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(NativeSnapshotMemory.Random));
        runtime.Enemies.FrameContext._randomEnemyCounter = W(NativeSnapshotMemory.MainEnemyRoutineCount);
        // The liquid's BG3 HDMA object has already run its first pass, so its callback is
        // installed. A live object owns the per-frame liquid motion; once Mother Brain's
        // $A9:8C0C has cleared the channel flags, the room's liquid objects are deleted.
        bool lavaAcidObjectLive = Enumerable.Range(0, NativeSnapshotMemory.HdmaObjectCount).Any(slot =>
            W(NativeSnapshotMemory.HdmaObjectChannelBitflags + slot * 2) != 0 &&
            W(NativeSnapshotMemory.HdmaObjectPreInstructions + slot * 2) == NativeSnapshotMemory.AcidHdmaCallback);
        AssertTrue(runtime.RoomLayer3Fx.Type is RoomFxType.Lava or RoomFxType.Acid,
            "snapshot import models the lava/acid HDMA objects of its room");
        runtime.RoomLayer3Fx.lavaAcidBg3PreInstructionInstalled = true;
        runtime.RoomLayer3Fx.liquidHdmaObjectsDeleted = !lavaAcidObjectLive;
        runtime.RoomLayer3Fx.tidePhase = W(NativeSnapshotMemory.TidePhase);
        runtime.RoomLayer3Fx.tideFixedOffset = unchecked((int)(SnesSixteenSixteen.Compose(W(NativeSnapshotMemory.TideOffset), W(NativeSnapshotMemory.TideOffsetFraction))));
        runtime.RoomLayer3Fx.baseYSubposition = W(NativeSnapshotMemory.LiquidBaseFraction);
        runtime.RoomLayer3Fx.CurrentYPosition = W(NativeSnapshotMemory.AcidSurface);
        runtime.RoomLayer3Fx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        var pipeBugPointers = new PipeBugNativeInstructionPointers(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus));
        // EnemyData +$0E onward; +$00 is the identity, +$02..+$08 the 16.16 position and
        // +$0A/+$0C the radii, each imported as one typed value below.
        for (int index = 0; index < runtime.Enemies.SlotTable.Slots.Count; index++)
        {
            var slot = runtime.Enemies.SlotTable.Slots[index];
            int address = NativeSnapshotMemory.EnemyBase + index * 64;
            if (W(address) != 0)
                AssertTrue((ushort)slot.EnemyDefinitionPointer == W(address), "initial enemy species agrees with room population");
            slot.Bind((EnemyDefinitionId)W(address), slot.Definition);
            slot.Position = FixedPosition<RoomPixels>.FromWords(W(address + 2), W(address + 4), W(address + 6), W(address + 8));
            slot.Radius = new(W(address + 10), W(address + 12));
            slot.Properties = W(address + 14);
            slot.ExtraProperties = W(address + 16);
            slot.AiStates = (EnemyAiStates)W(address + 18);
            slot.Health = W(address + 20);
            slot.SpritemapPointer = W(address + 22);
            slot.Timer = NativeCountdown.FromRaw(W(address + 24));
            slot.CurrentInstruction = W(address + 26);
            slot.InstructionTimer = NativeCountdown.FromRaw(W(address + 28));
            slot.PaletteIndex = W(address + 30);
            slot.VramTilesIndex = W(address + 32);
            slot.Layer = W(address + 34);
            slot.FlashTimer = NativeCountdown.FromRaw(W(address + 36));
            slot.FrozenTimer = NativeCountdown.FromRaw(W(address + 38));
            slot.InvincibilityTimer = NativeCountdown.FromRaw(W(address + 40));
            slot.ShakeTimer = NativeCountdown.FromRaw(W(address + 42));
            slot.FrameCounter = W(address + 44);
            slot.VariableA = W(address + 48);
            slot.VariableB = W(address + 50);
            slot.VariableC = W(address + 52);
            slot.VariableD = W(address + 54);
            slot.VariableE = W(address + 56);
            slot.VariableF = W(address + 58);
            // A Pipe Bug's animation runs as a typed program (#641): resume it where the recorded pointer stands.
            if (runtime.Enemies.PipeBugStates[index] is { } pipeBug)
                pipeBug.Program = pipeBugPointers.CursorAt(slot.CurrentInstruction);
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnPosition = new(slot.VariableB, slot.VariableC);
                pipe.RespawnDelay = NativeCountdown.FromRaw(slot.VariableD);
                pipe.AnimationState = (PipeBugAnimationSelector)slot.VariableE;
                pipe.EmergenceTopY = W(NativeSnapshotMemory.EnemyExtra + index * 64);
                pipe.InstalledAnimationState = (PipeBugAnimationSelector)W(NativeSnapshotMemory.EnemyExtraPreviousAnimation + index * 64);
            }
        }
        if (runtime.Enemies.MotherBrainEncounter.MotherBrain is not null)
            ImportNativeMotherBrain(runtime, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W);
        runtime.Nmi.NmiFrameCounter = W(NativeSnapshotMemory.NmiCounter);
        runtime.Nmi.NmiFrameCounter8 = memory[NativeSnapshotMemory.NmiCounterByte];
        samus.CollectedItems = W(NativeSnapshotMemory.CollectedItems); samus.CollectedBeams = W(NativeSnapshotMemory.CollectedBeams);
        samus.Missiles = W(NativeSnapshotMemory.Missiles); samus.MaxMissiles = W(NativeSnapshotMemory.MaxMissiles);
        samus.SuperMissiles = W(NativeSnapshotMemory.SuperMissiles); samus.MaxSuperMissiles = W(NativeSnapshotMemory.MaxSuperMissiles);
        samus.PowerBombs = W(NativeSnapshotMemory.PowerBombs); samus.MaxPowerBombs = W(NativeSnapshotMemory.MaxPowerBombs);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch((SnesButton)W(NativeSnapshotMemory.HeldInput));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        game.Session.runtime = runtime;
        game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
        game.GameState = (SuperMetroidGameState)W(NativeSnapshotMemory.GameState);
        if (game.GameState == SuperMetroidGameState.PausedB)
            ImportNativePauseMenu(game, runtime, bus, W, memory);
        return (bus, runtime, samus, game);
    }

    /// <summary>
    /// Enemy-projectile kinds whose complete state is the native slot arrays: no family keeps
    /// port-only words (afterburn chains, death-drop sources) that the arrays cannot supply.
    /// </summary>
    private static readonly HashSet<RoomEnemyProjectileKind> NativeSlotOnlyEnemyProjectileKinds =
    [
        RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging,
        RoomEnemyProjectileKind.MotherBrainPurpleBreathBig,
        RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall,
    ];

    /// <summary>
    /// Imports one enemy-projectile slot. A live slot is initialized from its definition, as
    /// <c>SpawnEprojInner</c> does, then takes every per-slot word the native arrays hold; the
    /// definition-derived radii and properties must already agree with them. Free slots keep
    /// their stale position and velocity words, which later spawns can read.
    /// </summary>
    private static void ImportNativeEnemyProjectile(RoomEnemyProjectileSlot projectile, IImportCartridgeSource cartridge, Func<int, ushort> W)
    {
        int index = projectile.SlotIndex * 2;
        ushort id = W(NativeSnapshotMemory.EnemyProjectileId + index);
        if (id == 0)
        {
            // Room loading spawns the room's projectiles afresh; a slot the snapshot holds
            // empty (for example Mother Brain's destroyed turrets) must be empty here too.
            projectile.Clear();
        }
        else
        {
            var kind = (RoomEnemyProjectileKind)id;
            AssertTrue(NativeSlotOnlyEnemyProjectileKinds.Contains(kind),
                $"initial native enemy projectile {projectile.SlotIndex} kind ${id:X4} has a verified slot-only import");
            EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
                projectile, kind, W(NativeSnapshotMemory.EnemyProjectileGraphics + index));
            projectile.PreInstruction = W(NativeSnapshotMemory.EnemyProjectilePreInstruction + index);
            ushort instruction = W(NativeSnapshotMemory.EnemyProjectileInstruction + index);
            // A typed family resumes at the step the recorded native pointer stands for.
            if (projectile.Animation is { } typed)
                AssertEqual(1, NativeEnemyProjectileFamilies.All.Count(family => family.TryResumeAt(cartridge, typed, instruction)),
                    $"initial enemy projectile {projectile.SlotIndex} resumes its typed program");
            else
                projectile.InstructionPointer = instruction;
            projectile.InstructionTimer = NativeCountdown.FromRaw(W(NativeSnapshotMemory.EnemyProjectileInstructionTimer + index));
            projectile.GeneralTimer = W(NativeSnapshotMemory.EnemyProjectileTimer + index);
            projectile.Position = projectile.Position
                .WithFractionX(W(NativeSnapshotMemory.EnemyProjectileXFraction + index))
                .WithFractionY(W(NativeSnapshotMemory.EnemyProjectileYFraction + index));
            projectile.Variable0 = W(NativeSnapshotMemory.EnemyProjectileVariableE + index);
            projectile.Variable1 = W(NativeSnapshotMemory.EnemyProjectileVariableF + index);
            projectile.CollidedProjectileType = W(NativeSnapshotMemory.EnemyProjectileVariableG + index);
            // The current frame record is [timer][operand]; the list pointer already follows it.
            EnemyProjectileEngine.SetEnemyProjectileVisualOperand(
                projectile, unchecked((ushort)(instruction - 2)));
            AssertEqual(W(NativeSnapshotMemory.EnemyProjectileRadius + index), SnesLittleEndian.Word(unchecked((byte)projectile.Radius.X), unchecked((byte)projectile.Radius.Y)),
                $"initial enemy projectile {projectile.SlotIndex} radii");
            ushort properties = projectile.Damage;
            if (projectile.DrawPriority == EnemyProjectileDrawPriority.High) properties |= NativeSnapshotMemory.EnemyProjectileHighDraw;
            if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
            if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
            if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
            AssertEqual(W(NativeSnapshotMemory.EnemyProjectileProperties + index), properties,
                $"initial enemy projectile {projectile.SlotIndex} properties");
        }
        projectile.Position = projectile.Position.WithWhole(new(
            W(NativeSnapshotMemory.EnemyProjectileX + index), W(NativeSnapshotMemory.EnemyProjectileY + index)));
        projectile.Velocity = new(
            new SnesSignedEightEight(W(NativeSnapshotMemory.EnemyProjectileXVelocity + index)),
            new SnesSignedEightEight(W(NativeSnapshotMemory.EnemyProjectileYVelocity + index)));
    }
}

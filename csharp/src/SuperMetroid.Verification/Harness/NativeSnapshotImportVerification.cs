using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Enemies.PipeBugs;
using System.Buffers.Binary;
using SuperMetroid.Core.Frontend;
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
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.Verification.Harness;

using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx.PaletteFx;

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
    /// <param name="hostServices">The hardware-timing policies, or <see langword="null"/> for the lag-free defaults.</param>
    internal static ImportNativeSnapshotResult
        ImportNativeSnapshot(byte[] memory, byte[] saveRam, SuperMetroidGameHostServices? hostServices = null)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        SaveSlotStore saves = SaveRamCodec.Decode(saveRam, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps);
        var random = new SharedRandom();
        var runtime = CreateRetailRuntimeFixture(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Options.MoonwalkEnabled = W(NativeSnapshotMemory.MoonwalkOption) != 0;
        runtime.System.LoadCollectedItemBytes(memory.AsSpan(NativeSnapshotMemory.CollectedItemBits, SharedGameState.ItemBitByteCount));
        runtime.System.LoadBossBytes(memory.AsSpan(NativeSnapshotMemory.BossBits, SharedGameState.AreaCount));
        runtime.System.LoadEventBytes(memory.AsSpan(NativeSnapshotMemory.Events, SharedGameState.EventByteCount));
        runtime.System.LoadOpenedDoorBytes(memory.AsSpan(NativeSnapshotMemory.OpenedDoors, SharedGameState.DoorBitByteCount));
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.Get(W(NativeSnapshotMemory.Room)), new Position16<RoomPixels>(W(NativeSnapshotMemory.CameraX), W(NativeSnapshotMemory.CameraY)));
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

        runtime.Colours.AsPaletteColors().LoadBytes(memory.AsSpan(NativeSnapshotMemory.PaletteBuffer, CgramPalettes.ByteCount));
        var initialPaletteSlots = runtime.RoomPaletteFx.slots;
        for (int slot = 0; slot < initialPaletteSlots.Length; slot++)
        {
            var paletteSlot = initialPaletteSlots[slot];
            paletteSlot.Id = W(NativeSnapshotMemory.PaletteFxId + slot * 2);
            // The native colour index names the colours the object animates: of the colour groups its programs name, the one
            // starting there.
            paletteSlot.Target = SuperMetroid.Core.Fx.PaletteFx.PaletteFxColoursPositions.FromCartridge(paletteSlot.Object,
                RoomPaletteFxSystemConstants.PaletteFxColorIndex(W(NativeSnapshotMemory.PaletteFxColor + slot * 2), paletteSlot.Id));
            // The pre-instruction values are arbitrary (#1415): a live object's routine word translates through the native view.
            paletteSlot.PreInstruction = paletteSlot.Id == 0
                ? default
                : SuperMetroid.Core.Fx.FxNativeWords.PaletteFxPreInstructions.Decode(
                    W(NativeSnapshotMemory.PaletteFxPreInstruction + slot * 2), "palette-FX pre-instruction");
            // A live object resumes its typed program where the recorded native pointer stands.
            paletteSlot.Cursor = paletteSlot.Id == 0
                ? default
                : SuperMetroid.Verification.Fx.PaletteFx.PaletteFxProgramsVerification.CursorAt(
                    SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W(NativeSnapshotMemory.PaletteFxInstruction + slot * 2));
            paletteSlot.InstructionTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.PaletteFxInstructionTimer + slot * 2));
            paletteSlot.Timer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.PaletteFxTimer + slot * 2));
        }
        runtime.PaletteFxSession.SamusInHeatPaletteIndex = W(NativeSnapshotMemory.HeatPalettePhase);
        runtime.PaletteFxSession.PreviousSamusInHeatPaletteIndex = W(NativeSnapshotMemory.PreviousHeatPalettePhase);
        BackgroundTile[] initialHud = runtime.Hud.Tiles;
        for (int index = 0; index < initialHud.Length; index++)
            initialHud[index] = BackgroundTileWordBytes.Tile(W(NativeSnapshotMemory.HudTilemap + index * 2));
        runtime.Hud.PreviousSelectedItem = HudItems.Decode(W(NativeSnapshotMemory.PreviousHudSelection));
        runtime.Room.LoadedCamera.Scrolls.Storage.Load(memory.AsSpan(NativeSnapshotMemory.ScrollStorage, RoomScrollGrid.StorageByteCount));
        runtime.Hud.MinimapDisabled = W(NativeSnapshotMemory.MinimapDisabled) != 0;
        byte[] initialMaps = memory.AsSpan(NativeSnapshotMemory.SavedExploredMaps,
            SharedGameState.ExploredMapAreaCount * SharedGameState.ExploredMapBytesPerArea).ToArray();
        memory.AsSpan(NativeSnapshotMemory.LiveExploredMap, SharedGameState.ExploredMapBytesPerArea)
            .CopyTo(initialMaps.AsSpan(W(NativeSnapshotMemory.CurrentArea) * SharedGameState.ExploredMapBytesPerArea));
        runtime.System.LoadExploredMapBytes(initialMaps);
        runtime.System.LoadUsedSaveStationBytes(memory.AsSpan(NativeSnapshotMemory.SaveElevatorMarkers, SharedGameState.UsedSaveStationByteCount));
        runtime.System.LoadMapStationBytes(memory.AsSpan(NativeSnapshotMemory.MapStationMarkers, SharedGameState.MapStationByteCount));

        SamusState samus = runtime.Player.Samus ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load Samus.");
        samus.Control.Unlock();
        // The recorded indices name the pictures the body last drew; they are imported as the pose-half frames that show them.
        InstalledSamusSpritemaps spritemaps = samus.Presentation.BodyFrames.Artwork?.Spritemaps ?? throw new InvalidDataException(
            "The native Ridley checkpoint requires installed Samus spritemaps.");
        samus.Presentation.TopSpritemap = SamusSpritemapNative.Frame(spritemaps, (SamusPoseId)W(NativeSnapshotMemory.Pose), SamusBodyHalf.Top, W(NativeSnapshotMemory.SamusTopSpritemap));
        samus.Presentation.BottomSpritemap = SamusSpritemapNative.Frame(spritemaps, (SamusPoseId)W(NativeSnapshotMemory.Pose), SamusBodyHalf.Bottom, W(NativeSnapshotMemory.SamusBottomSpritemap));
        samus.Presentation.SpritemapPosition = new Position16<ScreenPixels>(W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY));
        samus.ArmCannon.Frame = W(NativeSnapshotMemory.CannonFrame);
        samus.ArmCannon.ToggleFlag = W(NativeSnapshotMemory.CannonToggle);
        samus.ArmCannon.DrawingMode = W(NativeSnapshotMemory.CannonDrawingMode);
        samus.ArmCannon.OpenFlag = memory[NativeSnapshotMemory.CannonFlags];
        samus.ArmCannon.CloseFlag = memory[NativeSnapshotMemory.CannonFlags + 1];
        new ProjectileInheritanceRecordsAccess(samus.ProjectileInheritance).Load(memory.AsSpan(NativeSnapshotMemory.ProjectileInheritancePrefix,
            NativeSnapshotMemory.SamusSlopeAdjusted - NativeSnapshotMemory.ProjectileInheritancePrefix));
        samus.Kinematics.PositionAdjustedBySlope = W(NativeSnapshotMemory.SamusSlopeAdjusted) != 0;
        for (int direction = 0; direction < 4; direction++)
            samus.Kinematics.RecordSolidEnemyCollision((SamusCollisionDirection)direction,
                W(NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2));
        samus.HorizontalSpeed.ActiveSpeedTableBaseAddress = W(NativeSnapshotMemory.HorizontalSpeedTable);
        samus.HorizontalSpeed.DecelerationMultiplier = memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier];
        samus.HorizontalSpeed.EchoSoundFlag = W(NativeSnapshotMemory.SpeedEchoSoundLatch);
        samus.HorizontalSpeed.TotalSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.TotalHorizontalSpeed), W(NativeSnapshotMemory.TotalHorizontalSubspeed));
        runtime.Projectiles.SlotTable.ProjectileCounter = W(NativeSnapshotMemory.ProjectileCount);
        runtime.Projectiles.Charge.PreviousBeamChargeCounter = W(NativeSnapshotMemory.PreviousCharge);
        runtime.Projectiles.Firing.ProjectileInvincibilityTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.ProjectileInteractionImmunity));
        runtime.Projectiles.Charge.ChargedShotGlowTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.ChargedShotGlow));
        int chargePaletteByteOffset = W(NativeSnapshotMemory.ChargePaletteIndex);
        runtime.Projectiles.Charge.ChargePaletteStep = (chargePaletteByteOffset & 1) == 0
            ? chargePaletteByteOffset / sizeof(ushort)
            : throw new InvalidDataException($"Native charge palette byte offset {chargePaletteByteOffset} names no step.");
        runtime.BombProjectiles.BombCounter = W(NativeSnapshotMemory.BombCount);
        samus.BombSpreadChargeTimeoutCounter = W(NativeSnapshotMemory.BombSpreadChargeTimeout);
        samus.PoseTransitionShotDirection = W(NativeSnapshotMemory.PoseShotDirection);
        samus.HyperBeamEnabled = SamusSnapshotWords.DecodeHyperBeam(W(NativeSnapshotMemory.HyperBeam));
        samus.ResumeChargingBeamSound = SamusSnapshotWords.DecodeResumeChargingBeamSound(W(NativeSnapshotMemory.ResumeChargeSound));
        samus.InputSnapshot.PreviousDrawHeldInput = (ControllerButton)W(NativeSnapshotMemory.SamusFilteredHeld);
        samus.InputSnapshot.PreviousDrawNewInput = (ControllerButton)W(NativeSnapshotMemory.SamusFilteredNew);
        samus.InputSnapshot.AutoJumpTimer = W(NativeSnapshotMemory.SamusAutoJumpTimer);
        samus.PreviousHealthForHurtCheck = SamusQuantity.Decode(W(NativeSnapshotMemory.SamusPreviousHealthForFlash));
        AssertTrue(W(NativeSnapshotMemory.SamusInputHandler) is
            NativeSnapshotMemory.SamusNormalInputHandler or NativeSnapshotMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        samus.InputSnapshot.AutoJumpInputPending = W(NativeSnapshotMemory.SamusInputHandler) == NativeSnapshotMemory.SamusAutoJumpInputHandler;
        samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(W(NativeSnapshotMemory.Items));
        samus.EquippedBeams = (SamusBeamFlags)(W(NativeSnapshotMemory.Beams));
        samus.Health = SamusQuantity.Decode(W(NativeSnapshotMemory.Health));
        samus.MaxHealth = SamusQuantity.Decode(W(NativeSnapshotMemory.MaxHealth));
        samus.InvincibilityTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.InvincibilityTimer));
        samus.KnockbackTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.KnockbackTimer));
        samus.KnockbackDirection = (SamusKnockbackDirection)W(NativeSnapshotMemory.KnockbackDirection);
        samus.KnockbackXDirection = (KnockbackHorizontalDirection)W(NativeSnapshotMemory.KnockbackXDirection);
        samus.HurtFlashCounter = W(NativeSnapshotMemory.HurtFlashCounter);
        samus.SubunitHealth = W(NativeSnapshotMemory.SubunitHealth);
        samus.SelectedHudItem = HudItems.Decode(W(NativeSnapshotMemory.SelectedHudItem));
        samus.ArmCannon.PreviousSelectedHudItem = samus.SelectedHudItem;
        samus.AutoCancelHudItemIndex = HudItems.Decode(W(NativeSnapshotMemory.AutoCancelHudItemIndex));
        samus.ReserveTankMode = ReserveTankModes.Decode(W(NativeSnapshotMemory.ReserveMode));
        samus.MaxReserveEnergy = SamusQuantity.Decode(W(NativeSnapshotMemory.MaxReserve));
        samus.ReserveEnergy = SamusQuantity.Decode(W(NativeSnapshotMemory.Reserve));
        samus.Pose = (SamusPoseId)W(NativeSnapshotMemory.Pose);
        samus.Position = samus.Position.WithWhole(new(W(NativeSnapshotMemory.X), W(NativeSnapshotMemory.Y)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(W(NativeSnapshotMemory.XFraction)).WithFractionY(W(NativeSnapshotMemory.YFraction));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, W(NativeSnapshotMemory.Animation), W(NativeSnapshotMemory.AnimationTimer));
        samus.PoseHistory.PreviousPose = (SamusPoseId)W(NativeSnapshotMemory.PreviousPose);
        samus.PoseHistory.PreviousDirectionAndMovement = W(NativeSnapshotMemory.PreviousDirection);
        samus.PoseHistory.LastDifferentPose = (SamusPoseId)W(NativeSnapshotMemory.LastDifferentPose);
        samus.PoseHistory.LastDifferentDirectionAndMovement = W(NativeSnapshotMemory.LastDifferentDirection);
        samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.BaseSpeed), W(NativeSnapshotMemory.BaseFraction));
        samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.ExtraSpeed), W(NativeSnapshotMemory.ExtraFraction));
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationModeImport.FromNative(W(NativeSnapshotMemory.AccelerationMode));
        samus.HorizontalSpeed.HasRunningMomentum = W(NativeSnapshotMemory.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = new SamusSpeedBoostCounter(W(NativeSnapshotMemory.BoostCounter));
        samus.Kinematics.YSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.VerticalSpeed), W(NativeSnapshotMemory.VerticalFraction));
        samus.Kinematics.YDirection = ClosedEnumWordDecoding.Decode<SamusVerticalDirection>(
            W(NativeSnapshotMemory.VerticalDirection), "Samus vertical direction");
        samus.Kinematics.Radius = new(W(NativeSnapshotMemory.SamusXRadius), W(NativeSnapshotMemory.SamusYRadius));
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.Gravity), W(NativeSnapshotMemory.GravityFraction));
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromWords(W(NativeSnapshotMemory.ExtraXDisplacement), W(NativeSnapshotMemory.ExtraXDisplacementFraction), W(NativeSnapshotMemory.ExtraYDisplacement), W(NativeSnapshotMemory.ExtraYDisplacementFraction));
        samus.Kinematics.HorizontalSlopeCollisionEnable = W(NativeSnapshotMemory.SlopeCollisionEnable);
        samus.HorizontalSpeed.SpeedDivisor = W(NativeSnapshotMemory.SpeedDivisor);
        samus.HorizontalSpeed.ContactDamageIndex = ClosedEnumWordDecoding.Decode<SamusContactDamageIndex>(
            W(NativeSnapshotMemory.ContactDamageIndex), "Samus contact damage index");
        samus.MorphBallBounceState = ClosedEnumWordDecoding.Decode<SamusBallBounceState>(
            W(NativeSnapshotMemory.MorphBallBounceState), "Samus morph-ball bounce state");
        samus.BombJumpDirection = W(NativeSnapshotMemory.BombJumpDirection);

        foreach (var trail in runtime.Projectiles.TrailSlots)
        {
            trail.Left.InstructionTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.TrailLeftInstructionTimer + trail.SlotTableOffset));
            trail.Right.InstructionTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.TrailRightInstructionTimer + trail.SlotTableOffset));
            trail.Left.InstructionPointer = W(NativeSnapshotMemory.TrailLeftInstructionPointer + trail.SlotTableOffset);
            trail.Right.InstructionPointer = W(NativeSnapshotMemory.TrailRightInstructionPointer + trail.SlotTableOffset);
            trail.Left.ShownPose = ProjectileTrailNative.ShownPose(W(NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.SlotTableOffset));
            trail.Right.ShownPose = ProjectileTrailNative.ShownPose(W(NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.SlotTableOffset));
            trail.Left.Position = new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailLeftXPosition + trail.SlotTableOffset), W(NativeSnapshotMemory.TrailLeftYPosition + trail.SlotTableOffset));
            trail.Right.Position = new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailRightXPosition + trail.SlotTableOffset), W(NativeSnapshotMemory.TrailRightYPosition + trail.SlotTableOffset));
        }

        for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
        {
            ushort packed = W(NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
            samus.LiquidPhysics.AtmosphericEffects.SetSlot(index, (SamusAtmosphericEffectType)LittleEndianBytes.HighByte(packed), (byte)packed,
                W(NativeSnapshotMemory.AtmosphericTimer + index * 2),
                new(W(NativeSnapshotMemory.AtmosphericX + index * 2), W(NativeSnapshotMemory.AtmosphericY + index * 2)));
        }
        samus.AnimationFrameBuffer = W(NativeSnapshotMemory.AnimationFrameBuffer);
        samus.LiquidPhysics.LiquidPhysicsType = ClosedEnumWordDecoding.Decode<SamusLiquidMedium>(W(NativeSnapshotMemory.LiquidPhysicsType), "liquid physics type");
        samus.LiquidPhysics.PeriodicSubDamage = W(NativeSnapshotMemory.PeriodicSubDamage);
        samus.LiquidPhysics.PeriodicDamage = W(NativeSnapshotMemory.PeriodicDamage);

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialPlms = runtime.Plms.SlotTable.Slots;
        for (int index = 0; index < initialPlms.Count; index++)
        {
            AssertTrue(W(NativeSnapshotMemory.PlmHeaders + index * 2) == 0, "native initial PLM pool is empty");
            initialPlms[index].Delete();
        }
        foreach (var projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            ImportNativeEnemyProjectile(projectile, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W,
                runtime.Enemies.GraphicsSet);
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(NativeSnapshotMemory.Random));
        runtime.Enemies.FrameContext.SeedRandomEnemyCounter(W(NativeSnapshotMemory.MainEnemyRoutineCount));
        // The liquid's BG3 HDMA object has already run its first pass, so its callback is
        // installed. A live object owns the per-frame liquid motion; once Mother Brain's
        // $A9:8C0C has cleared the channel flags, the room's liquid objects are deleted.
        bool lavaAcidObjectLive = Enumerable.Range(0, NativeSnapshotMemory.HdmaObjectCount).Any(slot =>
            W(NativeSnapshotMemory.HdmaObjectChannelBitflags + slot * 2) != 0 &&
            W(NativeSnapshotMemory.HdmaObjectPreInstructions + slot * 2) == NativeSnapshotMemory.AcidHdmaCallback);
        AssertTrue(runtime.RoomLayer3Fx.Type is RoomFxType.Lava or RoomFxType.Acid,
            "snapshot import models the lava/acid HDMA objects of its room");
        runtime.RoomLayer3Fx.LavaAcidBg3PreInstructionInstalled = true;
        runtime.RoomLayer3Fx.LiquidMotionStopped = !lavaAcidObjectLive;
        runtime.RoomLayer3Fx.TidePhase = W(NativeSnapshotMemory.TidePhase);
        runtime.RoomLayer3Fx.TideFixedOffset = unchecked((int)(SixteenSixteen.Compose(W(NativeSnapshotMemory.TideOffset), W(NativeSnapshotMemory.TideOffsetFraction))));
        runtime.RoomLayer3Fx.BaseY = runtime.RoomLayer3Fx.BaseY.WithFraction(W(NativeSnapshotMemory.LiquidBaseFraction));
        runtime.RoomLayer3Fx.Surface = new(0, W(NativeSnapshotMemory.AcidSurface));
        samus.LiquidPhysics.ConfigureFor(runtime.RoomLayer3Fx);
        var pipeBugPointers = new PipeBugNativeInstructionPointers(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus));
        var kraidHeadCursors = new SuperMetroid.Verification.Bosses.Kraid.KraidHeadNativeCursors(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus));
        // EnemyData +$0E onward; +$00 is the identity, +$02..+$08 the 16.16 position and
        // +$0A/+$0C the radii, each imported as one typed value below.
        for (int index = 0; index < runtime.Enemies.SlotTable.Slots.Count; index++)
        {
            var slot = runtime.Enemies.SlotTable.Slots[index];
            int address = NativeSnapshotMemory.EnemyBase + index * 64;
            if (W(address) != 0)
                AssertTrue(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(slot.DefinitionId) == W(address), "initial enemy species agrees with room population");
            slot.Bind(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionSlotWords.FromSlotWord(W(address)), slot.Definition);
            slot.Position = FixedPosition<RoomPixels>.FromWords(W(address + 2), W(address + 4), W(address + 6), W(address + 8));
            slot.Radius = new(W(address + 10), W(address + 12));
            slot.Properties = EnemyPropertyWord.FromWord(W(address + 14));
            slot.ExtraProperties = EnemyExtraPropertyWord.FromWord(W(address + 16));
            slot.AiStates = (EnemyAiStates)W(address + 18);
            slot.Health = W(address + 20);
            // A free native slot keeps its last occupant's word in that occupant's bank; a free port slot shows no frame.
            slot.SpritemapPointer = W(address) == 0 ? (ushort)0 : W(address + 22);
            slot.Timer = WrappingCountdown.FromRaw(W(address + 24));
            slot.CurrentInstruction = W(address + 26);
            slot.InstructionTimer = WrappingCountdown.FromRaw(W(address + 28));
            // The native palette word names a row; the colouring it denotes is decoded under the room's loaded set.
            slot.SetPaletteRow(runtime.Enemies, NativeSpriteGraphicsWords.PaletteRow(W(address + 30)));
            slot.TileIndexBase = W(address + 32);
            slot.Layer = W(address + 34);
            slot.FlashTimer = WrappingCountdown.FromRaw(W(address + 36));
            slot.FrozenTimer = WrappingCountdown.FromRaw(W(address + 38));
            slot.InvincibilityTimer = WrappingCountdown.FromRaw(W(address + 40));
            slot.ShakeTimer = WrappingCountdown.FromRaw(W(address + 42));
            slot.FrameCounter = W(address + 44);
            var words = new NativeEnemyRecordWords(
                W(address + 48), W(address + 50), W(address + 52), W(address + 54), W(address + 56), W(address + 58));
            // A free native slot's family words are its last occupant's, which no family state of this slot holds (#1435).
            if (W(address) != 0)
                NativeEnemyFamilyStateImport.Import(runtime.Enemies, slot, words);
            // Kraid's head program is typed and kept in the encounter state; the recorded body word is its native cursor,
            // which only this import translates. Before the first attack the word names no head record and the head stays unstarted.
            if (slot.DefinitionId == EnemyDefinitionId.Kraid && runtime.Enemies.KraidBehavior.Access.Kraid is { } kraid)
                kraid.HeadProgram = kraidHeadCursors.TryCursorAt(words.B);
            // A Pipe Bug's animation runs as a typed program (#641): resume it where the recorded pointer stands.
            if (runtime.Enemies.PipeBugStates[index] is { } pipeBug)
                pipeBug.Program = pipeBugPointers.CursorAt(checked((ushort)slot.CurrentInstruction));
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnPosition = new(words.B, words.C);
                pipe.RespawnDelay = WrappingCountdown.FromRaw(words.D);
                pipe.AnimationState = (PipeBugAnimationSelector)words.E;
                pipe.EmergenceTop = pipe.SpawnPosition.WithY(W(NativeSnapshotMemory.EnemyExtra + index * 64));
                pipe.InstalledAnimationState = (PipeBugAnimationSelector)W(NativeSnapshotMemory.EnemyExtraPreviousAnimation + index * 64);
            }
        }
        if (runtime.Enemies.MotherBrainEncounter.MotherBrain is not null)
            ImportNativeMotherBrain(runtime, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W);
        runtime.Clock.Import(memory[NativeSnapshotMemory.NmiCounterByte], W(NativeSnapshotMemory.NmiCounter));
        samus.CollectedItems = SamusEquipmentWordCodec.FromEquipmentWord(W(NativeSnapshotMemory.CollectedItems)); samus.CollectedBeams = (SamusBeamFlags)(W(NativeSnapshotMemory.CollectedBeams));
        samus.Missiles = SamusQuantity.Decode(W(NativeSnapshotMemory.Missiles)); samus.MaxMissiles = SamusQuantity.Decode(W(NativeSnapshotMemory.MaxMissiles));
        samus.SuperMissiles = SamusQuantity.Decode(W(NativeSnapshotMemory.SuperMissiles)); samus.MaxSuperMissiles = SamusQuantity.Decode(W(NativeSnapshotMemory.MaxSuperMissiles));
        samus.PowerBombs = SamusQuantity.Decode(W(NativeSnapshotMemory.PowerBombs)); samus.MaxPowerBombs = SamusQuantity.Decode(W(NativeSnapshotMemory.MaxPowerBombs));
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch((ControllerButton)W(NativeSnapshotMemory.HeldInput));
        var game = CreateRetailGameFixture(saves, renderGameplayFrames: false, hostServices: hostServices, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.GameState = (SuperMetroidGameState)W(NativeSnapshotMemory.GameState);
        if (game.GameState == SuperMetroidGameState.PausedB)
            ImportNativePauseMenu(game, runtime, W, memory);
        return new ImportNativeSnapshotResult(bus, runtime, samus, game);
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
    private static void ImportNativeEnemyProjectile(RoomEnemyProjectileSlot projectile, IImportCartridgeSource cartridge, Func<int, ushort> W,
        EnemyGraphicsSet graphicsSet)
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
            RoomEnemyProjectileKind kind = EnemyProjectileNativeWords.Kinds.Decode(id, "enemy projectile definition");
            AssertTrue(NativeSlotOnlyEnemyProjectileKinds.Contains(kind),
                $"initial native enemy projectile {projectile.SlotIndex} kind ${id:X4} has a verified slot-only import");
            // The snapshot's graphics word packs the projectile's tile base and palette row as an attribute word does; the
            // row is decoded into the colouring it denotes under the room's loaded set.
            var graphics = new SnesObjAttributeWord(W(NativeSnapshotMemory.EnemyProjectileGraphics + index));
            FamilyProjectileStarts.Initialize(
                projectile, kind, new SpriteGraphics(graphics.TileNumber, graphicsSet.ColouringInRow(graphics.PaletteIndex)));
            projectile.PreInstructionWord = W(NativeSnapshotMemory.EnemyProjectilePreInstruction + index);
            ushort instruction = W(NativeSnapshotMemory.NativeEnemyProjectileInstruction + index);
            // The typed family resumes at the step the recorded native pointer stands for.
            EnemyProjectileAnimation typed = projectile.Animation ?? throw new InvalidOperationException(
                $"Initial enemy projectile {projectile.SlotIndex} kind {kind} starts no typed program.");
            AssertEqual(1, NativeEnemyProjectileFamilies.All.Count(family => family.TryResumeAt(cartridge, typed, instruction)),
                $"initial enemy projectile {projectile.SlotIndex} resumes its typed program");
            projectile.InstructionTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.EnemyProjectileInstructionTimer + index));
            projectile.GeneralTimer = W(NativeSnapshotMemory.EnemyProjectileTimer + index);
            projectile.Position = projectile.Position
                .WithFractionX(W(NativeSnapshotMemory.EnemyProjectileXFraction + index))
                .WithFractionY(W(NativeSnapshotMemory.EnemyProjectileYFraction + index));
            projectile.Variable0 = W(NativeSnapshotMemory.EnemyProjectileVariableE + index);
            projectile.Variable1 = W(NativeSnapshotMemory.EnemyProjectileVariableF + index);
            projectile.CollidedProjectileType = W(NativeSnapshotMemory.EnemyProjectileVariableG + index);
            // The current frame record is [timer][operand]; the list pointer already follows it.
            EnemyProjectileNativeImportAccess.ShowNativeOperand(
                projectile, unchecked((ushort)(instruction - 2)));
            AssertEqual(W(NativeSnapshotMemory.EnemyProjectileRadius + index), LittleEndianBytes.Word(unchecked((byte)projectile.Radius.X), unchecked((byte)projectile.Radius.Y)),
                $"initial enemy projectile {projectile.SlotIndex} radii");
            ushort properties = checked((ushort)projectile.Damage);
            if (projectile.Depth == EnemyProjectileDepth.InFrontOfSamus) properties |= NativeSnapshotMemory.EnemyProjectileInFrontOfSamus;
            if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
            if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
            if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
            AssertEqual(W(NativeSnapshotMemory.EnemyProjectileProperties + index), properties,
                $"initial enemy projectile {projectile.SlotIndex} properties");
        }
        projectile.Position = projectile.Position.WithWhole(new(
            W(NativeSnapshotMemory.EnemyProjectileX + index), W(NativeSnapshotMemory.EnemyProjectileY + index)));
        projectile.Velocity = new(
            new SignedEightEight(W(NativeSnapshotMemory.EnemyProjectileXVelocity + index)),
            new SignedEightEight(W(NativeSnapshotMemory.EnemyProjectileYVelocity + index)));
    }
}

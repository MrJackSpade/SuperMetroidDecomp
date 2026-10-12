using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static NativeSnapshotImportMotherBrainVerification;
using static NativeSnapshotImportPauseVerification;
using static RetailGameFixtureVerification;
using static RetailRuntimeFixtureVerification;

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
        ScrollBoundaryCamera camera = runtime.Room.Camera!;
        // The room load wrote only the integer layer-one words; import the native fractions,
        // ideal position and speed words as whole camera values.
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.Position))!.SetValue(camera,
            camera.Position.WithFractionsOf(FixedPosition<RoomPixels>.FromWords(
                0, W(NativeSnapshotMemory.CameraXFraction), 0, W(NativeSnapshotMemory.CameraYFraction))));
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.IdealPosition))!.SetValue(camera,
            new Position16<RoomPixels>(W(NativeSnapshotMemory.IdealCameraX), W(NativeSnapshotMemory.IdealCameraY)));
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.Speed))!.SetValue(camera,
            FixedVector<RoomPixels>.FromWords(
                W(NativeSnapshotMemory.CameraSpeedX), W(NativeSnapshotMemory.CameraSpeedXFraction),
                W(NativeSnapshotMemory.CameraSpeedY), W(NativeSnapshotMemory.CameraSpeedYFraction)));
        runtime.Room.Camera!.FinishSamusScrolling(FixedPosition<RoomPixels>.FromWords(
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
        var initialPaletteSlots = (Array)typeof(RoomPaletteFxSystem)
            .GetField("slots", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.RoomPaletteFx)!;
        for (int slot = 0; slot < initialPaletteSlots.Length; slot++)
        {
            object paletteSlot = initialPaletteSlots.GetValue(slot)!;
            foreach (var (property, address) in new[]
            {
                ("Id", NativeSnapshotMemory.PaletteFxId), ("ColorByteIndex", NativeSnapshotMemory.PaletteFxColor),
                ("PreInstruction", NativeSnapshotMemory.PaletteFxPreInstruction), ("InstructionPointer", NativeSnapshotMemory.PaletteFxInstruction),
                ("InstructionTimer", NativeSnapshotMemory.PaletteFxInstructionTimer), ("Timer", NativeSnapshotMemory.PaletteFxTimer),
            }) paletteSlot.GetType().GetProperty(property)!.SetValue(paletteSlot, W(address + slot * 2));
        }
        foreach (var (field, address) in new[]
        {
            ("samusInHeatPaletteIndex", NativeSnapshotMemory.HeatPalettePhase),
            ("previousSamusInHeatPaletteIndex", NativeSnapshotMemory.PreviousHeatPalettePhase),
        }) typeof(RoomPaletteFxSystem).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(runtime.RoomPaletteFx, W(address));
        ushort[] initialHud = (ushort[])typeof(HudState)
            .GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Hud)!;
        for (int index = 0; index < initialHud.Length; index++)
            initialHud[index] = W(NativeSnapshotMemory.HudTilemap + index * 2);
        var previousHudSelection = typeof(HudState)
            .GetField("_previousSelectedItem", BindingFlags.Instance | BindingFlags.NonPublic)!;
        previousHudSelection.SetValue(runtime.Hud, W(NativeSnapshotMemory.PreviousHudSelection));
        byte[] initialScrolls = (byte[])typeof(RoomScrollGrid)
            .GetField("_cells", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Room.Camera.Scrolls)!;
        memory.AsSpan(NativeSnapshotMemory.ScrollStorage, RoomScrollGrid.StorageByteCount).CopyTo(initialScrolls);
        WorkRamByteTable scrollZones = new RoomScrollMemory(bus).Zones;
        for (int index = 0; index < initialScrolls.Length; index++)
            scrollZones[index] = initialScrolls[index];
        typeof(HudState).GetProperty(nameof(HudState.MinimapDisabled))!
            .SetValue(runtime.Hud, W(NativeSnapshotMemory.MinimapDisabled) != 0);
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
        foreach (var (property, address) in new[]
        {
            (nameof(SamusPresentation.TopSpritemapIndex), NativeSnapshotMemory.SamusTopSpritemap),
            (nameof(SamusPresentation.BottomSpritemapIndex), NativeSnapshotMemory.SamusBottomSpritemap),
        }) typeof(SamusPresentation).GetProperty(property)!.SetValue(samus.Presentation, W(address));
        typeof(SamusPresentation).GetProperty(nameof(SamusPresentation.SpritemapPosition))!.SetValue(samus.Presentation,
            new Position16<ScreenPixels>(W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY)));
        foreach (var (property, address) in new[]
        {
            (nameof(SamusArmCannonState.Frame), NativeSnapshotMemory.CannonFrame),
            (nameof(SamusArmCannonState.ToggleFlag), NativeSnapshotMemory.CannonToggle),
            (nameof(SamusArmCannonState.DrawingMode), NativeSnapshotMemory.CannonDrawingMode),
        }) typeof(SamusArmCannonState).GetProperty(property)!.SetValue(samus.ArmCannon, W(address));
        typeof(SamusArmCannonState).GetProperty(nameof(SamusArmCannonState.OpenFlag))!
            .SetValue(samus.ArmCannon, memory[NativeSnapshotMemory.CannonFlags]);
        typeof(SamusArmCannonState).GetProperty(nameof(SamusArmCannonState.CloseFlag))!
            .SetValue(samus.ArmCannon, memory[NativeSnapshotMemory.CannonFlags + 1]);
        new ProjectileInheritanceRecordsAccess(bus).Load(memory.AsSpan(NativeSnapshotMemory.ProjectileInheritancePrefix,
            NativeSnapshotMemory.SamusSlopeAdjusted - NativeSnapshotMemory.ProjectileInheritancePrefix));
        samus.Kinematics.PositionAdjustedBySlope = W(NativeSnapshotMemory.SamusSlopeAdjusted) != 0;
        for (int direction = 0; direction < 4; direction++)
            samus.Kinematics.RecordSolidEnemyCollision((SamusCollisionDirection)direction,
                W(NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2));
        typeof(SamusHorizontalSpeedState).GetProperty(nameof(samus.HorizontalSpeed.ActiveSpeedTableBaseAddress))!
            .SetValue(samus.HorizontalSpeed, W(NativeSnapshotMemory.HorizontalSpeedTable));
        samus.HorizontalSpeed.DecelerationMultiplier = memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier];
        samus.HorizontalSpeed.EchoSoundFlag = W(NativeSnapshotMemory.SpeedEchoSoundLatch);
        typeof(SamusHorizontalSpeedState).GetProperty(nameof(samus.HorizontalSpeed.TotalSpeed))!
            .SetValue(samus.HorizontalSpeed, W(NativeSnapshotMemory.TotalHorizontalSpeed));
        typeof(SamusHorizontalSpeedState).GetProperty(nameof(samus.HorizontalSpeed.TotalSubspeed))!
            .SetValue(samus.HorizontalSpeed, W(NativeSnapshotMemory.TotalHorizontalSubspeed));
        typeof(ProjectileSlotTable).GetProperty("ProjectileCounter")!.SetValue(runtime.Projectiles.SlotTable, W(NativeSnapshotMemory.ProjectileCount));
        typeof(BeamChargeState).GetProperty("PreviousBeamChargeCounter")!.SetValue(runtime.Projectiles.Charge, W(NativeSnapshotMemory.PreviousCharge));
        typeof(BeamFiring).GetProperty("ProjectileInvincibilityTimer")!.SetValue(runtime.Projectiles.Firing, W(NativeSnapshotMemory.ProjectileInteractionImmunity));
        typeof(BeamChargeState).GetProperty("ChargedShotGlowTimer")!.SetValue(runtime.Projectiles.Charge, W(NativeSnapshotMemory.ChargedShotGlow));
        typeof(BeamChargeState).GetProperty("SamusChargePaletteIndex")!.SetValue(runtime.Projectiles.Charge, W(NativeSnapshotMemory.ChargePaletteIndex));
        runtime.BombProjectiles.GetType().GetProperty("BombCounter")!.SetValue(runtime.BombProjectiles, W(NativeSnapshotMemory.BombCount));
        samus.GetType().GetProperty("BombSpreadChargeTimeoutCounter")!.SetValue(samus, W(NativeSnapshotMemory.BombSpreadChargeTimeout));
        samus.GetType().GetProperty("PoseTransitionShotDirection")!.SetValue(samus, W(NativeSnapshotMemory.PoseShotDirection));
        samus.GetType().GetProperty("HyperBeam")!.SetValue(samus, W(NativeSnapshotMemory.HyperBeam));
        samus.GetType().GetProperty("ResumeChargingBeamSoundFlag")!.SetValue(samus, W(NativeSnapshotMemory.ResumeChargeSound));
        typeof(SamusInputSnapshot).GetProperty("PreviousDrawHeldInput")!.SetValue(samus.InputSnapshot, W(NativeSnapshotMemory.SamusFilteredHeld));
        typeof(SamusInputSnapshot).GetProperty("PreviousDrawNewInput")!.SetValue(samus.InputSnapshot, W(NativeSnapshotMemory.SamusFilteredNew));
        typeof(SamusInputSnapshot).GetProperty("AutoJumpTimer")!.SetValue(samus.InputSnapshot, W(NativeSnapshotMemory.SamusAutoJumpTimer));
        typeof(SamusState).GetProperty("PreviousHealthForHurtCheck")!.SetValue(samus, W(NativeSnapshotMemory.SamusPreviousHealthForFlash));
        AssertTrue(W(NativeSnapshotMemory.SamusInputHandler) is
            NativeSnapshotMemory.SamusNormalInputHandler or NativeSnapshotMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        typeof(SamusInputSnapshot).GetProperty(nameof(SamusInputSnapshot.AutoJumpInputPending))!.SetValue(samus.InputSnapshot,
            W(NativeSnapshotMemory.SamusInputHandler) == NativeSnapshotMemory.SamusAutoJumpInputHandler);
        samus.EquippedItems = W(NativeSnapshotMemory.Items);
        samus.EquippedBeams = W(NativeSnapshotMemory.Beams);
        samus.Health = W(NativeSnapshotMemory.Health);
        samus.MaxHealth = W(NativeSnapshotMemory.MaxHealth);
        samus.InvincibilityTimer = W(NativeSnapshotMemory.InvincibilityTimer);
        samus.KnockbackTimer = W(NativeSnapshotMemory.KnockbackTimer);
        samus.KnockbackDirection = W(NativeSnapshotMemory.KnockbackDirection);
        samus.KnockbackXDirection = W(NativeSnapshotMemory.KnockbackXDirection);
        samus.HurtFlashCounter = W(NativeSnapshotMemory.HurtFlashCounter);
        samus.SubunitHealth = W(NativeSnapshotMemory.SubunitHealth);
        samus.SelectedHudItem = W(NativeSnapshotMemory.SelectedHudItem);
        typeof(SamusArmCannonState).GetField("_previousSelectedHudItem", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(samus.ArmCannon, samus.SelectedHudItem);
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
            typeof(SamusProjectileTrailSide).GetProperty("InstructionTimer")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftInstructionTimer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionTimer")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightInstructionTimer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionPointer")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftInstructionPointer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionPointer")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightInstructionPointer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("TileNumberAttributes")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("TileNumberAttributes")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty(nameof(SamusProjectileTrailSide.Position))!.SetValue(trail.Left, new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailLeftXPosition + trail.NativeByteIndex), W(NativeSnapshotMemory.TrailLeftYPosition + trail.NativeByteIndex)));
            typeof(SamusProjectileTrailSide).GetProperty(nameof(SamusProjectileTrailSide.Position))!.SetValue(trail.Right, new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailRightXPosition + trail.NativeByteIndex), W(NativeSnapshotMemory.TrailRightYPosition + trail.NativeByteIndex)));
        }

        for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
        {
            ushort packed = W(NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
            samus.LiquidPhysics.AtmosphericEffects.SetSlot(index, SnesLittleEndian.HighByte(packed), (byte)packed,
                W(NativeSnapshotMemory.AtmosphericTimer + index * 2),
                new(W(NativeSnapshotMemory.AtmosphericX + index * 2), W(NativeSnapshotMemory.AtmosphericY + index * 2)));
        }
        typeof(SamusState).GetProperty(nameof(samus.AnimationFrameBuffer))!.SetValue(samus, W(NativeSnapshotMemory.AnimationFrameBuffer));
        typeof(SamusLiquidPhysicsState).GetProperty("LiquidPhysicsType")!.SetValue(samus.LiquidPhysics,
            ClosedNativeWords.Decode<SamusLiquidMedium>(W(NativeSnapshotMemory.LiquidPhysicsType), "liquid physics type"));
        typeof(SamusLiquidPhysicsState).GetProperty("PeriodicSubDamage")!.SetValue(samus.LiquidPhysics, W(NativeSnapshotMemory.PeriodicSubDamage));
        typeof(SamusLiquidPhysicsState).GetProperty("PeriodicDamage")!.SetValue(samus.LiquidPhysics, W(NativeSnapshotMemory.PeriodicDamage));

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        Array initialPlms = runtime.Plms.SlotTable._slots;
        for (int index = 0; index < initialPlms.Length; index++)
        {
            AssertTrue(W(NativeSnapshotMemory.PlmHeaders + index * 2) == 0, "native initial PLM pool is empty");
            object slot = initialPlms.GetValue(index)!;
            slot.GetType().GetProperty("Active")!.SetValue(slot, false);
        }
        foreach (var projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            ImportNativeEnemyProjectile(projectile, W);
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
        typeof(RoomLayer3FxState).GetField("lavaAcidBg3PreInstructionInstalled", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, true);
        typeof(RoomLayer3FxState).GetField("liquidHdmaObjectsDeleted", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, !lavaAcidObjectLive);
        typeof(RoomLayer3FxState).GetField("tidePhase", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, W(NativeSnapshotMemory.TidePhase));
        typeof(RoomLayer3FxState).GetField("tideFixedOffset", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx,
            unchecked((int)(SnesSixteenSixteen.Compose(W(NativeSnapshotMemory.TideOffset), W(NativeSnapshotMemory.TideOffsetFraction)))));
        typeof(RoomLayer3FxState).GetField("baseYSubposition", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, W(NativeSnapshotMemory.LiquidBaseFraction));
        typeof(RoomLayer3FxState).GetProperty(nameof(RoomLayer3FxState.CurrentYPosition))!.SetValue(runtime.RoomLayer3Fx, W(NativeSnapshotMemory.AcidSurface));
        runtime.RoomLayer3Fx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        // EnemyData +$0E onward; +$00 is the identity, +$02..+$08 the 16.16 position and
        // +$0A/+$0C the radii, each imported as one typed value below.
        string[] slotWords = ["Properties", "ExtraProperties", "AiHandlerBits", "Health", "SpritemapPointer", "Timer", "CurrentInstruction", "InstructionTimer", "PaletteIndex", "VramTilesIndex", "Layer", "FlashTimer", "FrozenTimer", "InvincibilityTimer", "ShakeTimer", "FrameCounter"];
        for (int index = 0; index < runtime.Enemies.SlotTable.Slots.Count; index++)
        {
            var slot = runtime.Enemies.SlotTable.Slots[index];
            int address = NativeSnapshotMemory.EnemyBase + index * 64;
            if (W(address) != 0)
                AssertTrue((ushort)slot.EnemyDefinitionPointer == W(address), "initial enemy species agrees with room population");
            typeof(RoomEnemySlot).GetProperty(nameof(RoomEnemySlot.EnemyDefinitionPointer))!.SetValue(slot, W(address));
            slot.Position = FixedPosition<RoomPixels>.FromWords(W(address + 2), W(address + 4), W(address + 6), W(address + 8));
            slot.Radius = new(W(address + 10), W(address + 12));
            for (int word = 0; word < slotWords.Length; word++)
                typeof(RoomEnemySlot).GetProperty(slotWords[word])!.SetValue(slot, W(address + 14 + word * 2));
            for (int word = 0; word < 6; word++)
                typeof(RoomEnemySlot).GetProperty("Variable" + (char)('A' + word))!.SetValue(slot, W(address + 48 + word * 2));
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnPosition = new(slot.VariableB, slot.VariableC);
                pipe.DelayOrCounter = slot.VariableD;
                pipe.AnimationState = (PipeBugAnimationSelector)slot.VariableE;
                pipe.EmergenceTopY = W(NativeSnapshotMemory.EnemyExtra + index * 64);
                pipe.InstalledAnimationState = (PipeBugAnimationSelector)W(NativeSnapshotMemory.EnemyExtraPreviousAnimation + index * 64);
            }
        }
        if (runtime.Enemies.MotherBrainEncounter.MotherBrain is not null)
            ImportNativeMotherBrain(runtime, W);
        runtime.Nmi.NmiFrameCounter = W(NativeSnapshotMemory.NmiCounter);
        runtime.Nmi.NmiFrameCounter8 = memory[NativeSnapshotMemory.NmiCounterByte];
        samus.CollectedItems = W(NativeSnapshotMemory.CollectedItems); samus.CollectedBeams = W(NativeSnapshotMemory.CollectedBeams);
        samus.Missiles = W(NativeSnapshotMemory.Missiles); samus.MaxMissiles = W(NativeSnapshotMemory.MaxMissiles);
        samus.SuperMissiles = W(NativeSnapshotMemory.SuperMissiles); samus.MaxSuperMissiles = W(NativeSnapshotMemory.MaxSuperMissiles);
        samus.PowerBombs = W(NativeSnapshotMemory.PowerBombs); samus.MaxPowerBombs = W(NativeSnapshotMemory.MaxPowerBombs);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch(W(NativeSnapshotMemory.HeldInput));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime.Room.ActiveRoom!.State.Pointer);
        typeof(SuperMetroidGame).GetProperty(nameof(game.GameState))!.SetValue(game, (SuperMetroidGameState)W(NativeSnapshotMemory.GameState));
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
    private static void ImportNativeEnemyProjectile(RoomEnemyProjectileSlot projectile, Func<int, ushort> W)
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
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.PreInstruction), W(NativeSnapshotMemory.EnemyProjectilePreInstruction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.InstructionPointer), W(NativeSnapshotMemory.EnemyProjectileInstruction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.InstructionTimer), W(NativeSnapshotMemory.EnemyProjectileInstructionTimer + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.GeneralTimer), W(NativeSnapshotMemory.EnemyProjectileTimer + index));
            projectile.Position = projectile.Position
                .WithFractionX(W(NativeSnapshotMemory.EnemyProjectileXFraction + index))
                .WithFractionY(W(NativeSnapshotMemory.EnemyProjectileYFraction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.Variable0), W(NativeSnapshotMemory.EnemyProjectileVariableE + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.Variable1), W(NativeSnapshotMemory.EnemyProjectileVariableF + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.CollidedProjectileType), W(NativeSnapshotMemory.EnemyProjectileVariableG + index));
            // The current frame record is [timer][operand]; the list pointer already follows it.
            EnemyProjectileEngine.SetEnemyProjectileVisualOperand(
                projectile, unchecked((ushort)(projectile.InstructionPointer - 2)));
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

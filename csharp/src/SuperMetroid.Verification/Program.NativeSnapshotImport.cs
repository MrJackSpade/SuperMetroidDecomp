using System.Buffers.Binary;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Builds a production game from one native WRAM snapshot. This is the single initial-state
    /// import a snapshot-start movie replay permits; nothing native is fed back afterwards.
    /// Unsupported live state is refused rather than approximated.
    /// </summary>
    /// <param name="memory">The 128 KiB native WRAM image at the movie's first input boundary.</param>
    /// <param name="saveRam">The movie's starting 8 KiB cartridge SRAM.</param>
    private static (CartridgeImportAddressSpace Bus, SuperMetroidRuntime Runtime, SamusState Samus, SuperMetroidGame Game)
        ImportNativeSnapshot(byte[] memory, byte[] saveRam)
    {
        ushort W(int address) => BinaryPrimitives.ReadUInt16LittleEndian(memory.AsSpan(address));
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        saveRam.CopyTo(bus.SaveRam);
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom(); runtime.InitializeCeresStartSamus();
        runtime.MoonwalkEnabled = W(NativeSnapshotMemory.MoonwalkOption) != 0;
        runtime.System.LoadCollectedItemBytes(memory.AsSpan(NativeSnapshotMemory.CollectedItemBits, Bank80SystemState.ItemBitByteCount));
        runtime.System.LoadBossBytes(memory.AsSpan(NativeSnapshotMemory.BossBits, Bank80SystemState.AreaCount));
        runtime.System.LoadEventBytes(memory.AsSpan(NativeSnapshotMemory.Events, Bank80SystemState.EventByteCount));
        runtime.System.LoadOpenedDoorBytes(memory.AsSpan(NativeSnapshotMemory.OpenedDoors, Bank80SystemState.DoorBitByteCount));
        runtime.LoadCartridgeRoomForDebug(W(NativeSnapshotMemory.Room), W(NativeSnapshotMemory.CameraX), W(NativeSnapshotMemory.CameraY));
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.XSubposition))!.SetValue(runtime.Camera, W(NativeSnapshotMemory.CameraXFraction));
        typeof(ScrollBoundaryCamera).GetProperty(nameof(ScrollBoundaryCamera.YSubposition))!.SetValue(runtime.Camera, W(NativeSnapshotMemory.CameraYFraction));
        foreach (var (property, address) in new[]
        {
            (nameof(ScrollBoundaryCamera.IdealXPosition), NativeSnapshotMemory.IdealCameraX),
            (nameof(ScrollBoundaryCamera.IdealYPosition), NativeSnapshotMemory.IdealCameraY),
            (nameof(ScrollBoundaryCamera.CameraXSpeed), NativeSnapshotMemory.CameraSpeedX),
            (nameof(ScrollBoundaryCamera.CameraXSubspeed), NativeSnapshotMemory.CameraSpeedXFraction),
            (nameof(ScrollBoundaryCamera.CameraYSpeed), NativeSnapshotMemory.CameraSpeedY),
            (nameof(ScrollBoundaryCamera.CameraYSubspeed), NativeSnapshotMemory.CameraSpeedYFraction),
        }) typeof(ScrollBoundaryCamera).GetProperty(property)!.SetValue(runtime.Camera, W(address));
        runtime.Camera!.FinishSamusScrolling(new SamusCameraPoint(
            W(NativeSnapshotMemory.PreviousSamusX), W(NativeSnapshotMemory.PreviousSamusXFraction),
            W(NativeSnapshotMemory.PreviousSamusY), W(NativeSnapshotMemory.PreviousSamusYFraction)));
        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.LevelData ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load room collision data.");
        for (int index = 0; index < level.WidthInBlocks * level.HeightInBlocks; index++)
        {
            level.SetForegroundEntry(index, W(NativeSnapshotMemory.Level + index * sizeof(ushort)));
            level.SetBehavior(index, memory[NativeSnapshotMemory.Bts + index]);
        }

        runtime.Cgram.LoadBytes(memory.AsSpan(NativeSnapshotMemory.PaletteBuffer, SnesCgram.ByteCount));
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
            .GetField("_cells", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Camera.Scrolls)!;
        memory.AsSpan(NativeSnapshotMemory.ScrollStorage, RoomScrollGrid.StorageByteCount).CopyTo(initialScrolls);
        for (int index = 0; index < initialScrolls.Length; index++)
            bus.WriteByte(RoomScrollGrid.WorkRamAddress + index, initialScrolls[index]);
        typeof(HudState).GetProperty(nameof(HudState.MinimapDisabled))!
            .SetValue(runtime.Hud, W(NativeSnapshotMemory.MinimapDisabled) != 0);
        byte[] initialMaps = memory.AsSpan(NativeSnapshotMemory.SavedExploredMaps,
            Bank80SystemState.ExploredMapAreaCount * Bank80SystemState.ExploredMapBytesPerArea).ToArray();
        memory.AsSpan(NativeSnapshotMemory.LiveExploredMap, Bank80SystemState.ExploredMapBytesPerArea)
            .CopyTo(initialMaps.AsSpan(W(NativeSnapshotMemory.CurrentArea) * Bank80SystemState.ExploredMapBytesPerArea));
        runtime.System.LoadExploredMapBytes(initialMaps);
        runtime.System.LoadUsedSaveStationBytes(memory.AsSpan(NativeSnapshotMemory.SaveElevatorMarkers, Bank80SystemState.UsedSaveStationByteCount));
        runtime.System.LoadMapStationBytes(memory.AsSpan(NativeSnapshotMemory.MapStationMarkers, Bank80SystemState.MapStationByteCount));

        SamusState samus = runtime.Samus ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load Samus.");
        samus.InputLocked = false;
        foreach (var (property, address) in new[]
        {
            (nameof(SamusState.TopSpritemapIndex), NativeSnapshotMemory.SamusTopSpritemap),
            (nameof(SamusState.BottomSpritemapIndex), NativeSnapshotMemory.SamusBottomSpritemap),
            (nameof(SamusState.SpritemapXPosition), NativeSnapshotMemory.SamusSpriteX),
            (nameof(SamusState.SpritemapYPosition), NativeSnapshotMemory.SamusSpriteY),
        }) typeof(SamusState).GetProperty(property)!.SetValue(samus, W(address));
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
        for (int address = NativeSnapshotMemory.ProjectileInheritancePrefix;
             address < NativeSnapshotMemory.SamusSlopeAdjusted; address++)
            bus.WriteByte(address, memory[address]);
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
        runtime.Projectiles.GetType().GetProperty("ProjectileCounter")!.SetValue(runtime.Projectiles, W(NativeSnapshotMemory.ProjectileCount));
        runtime.Projectiles.GetType().GetProperty("PreviousBeamChargeCounter")!.SetValue(runtime.Projectiles, W(NativeSnapshotMemory.PreviousCharge));
        runtime.Projectiles.GetType().GetProperty("ProjectileInvincibilityTimer")!.SetValue(runtime.Projectiles, W(NativeSnapshotMemory.ProjectileInteractionImmunity));
        runtime.Projectiles.GetType().GetProperty("ChargedShotGlowTimer")!.SetValue(runtime.Projectiles, W(NativeSnapshotMemory.ChargedShotGlow));
        runtime.Projectiles.GetType().GetProperty("SamusChargePaletteIndex")!.SetValue(runtime.Projectiles, W(NativeSnapshotMemory.ChargePaletteIndex));
        runtime.BombProjectiles.GetType().GetProperty("BombCounter")!.SetValue(runtime.BombProjectiles, W(NativeSnapshotMemory.BombCount));
        samus.GetType().GetProperty("BombSpreadChargeTimeoutCounter")!.SetValue(samus, W(NativeSnapshotMemory.BombSpreadChargeTimeout));
        samus.GetType().GetProperty("PoseTransitionShotDirection")!.SetValue(samus, W(NativeSnapshotMemory.PoseShotDirection));
        samus.GetType().GetProperty("HyperBeam")!.SetValue(samus, W(NativeSnapshotMemory.HyperBeam));
        samus.GetType().GetProperty("ResumeChargingBeamSoundFlag")!.SetValue(samus, W(NativeSnapshotMemory.ResumeChargeSound));
        typeof(SamusState).GetProperty("PreviousDrawHeldInput")!.SetValue(samus, W(NativeSnapshotMemory.SamusFilteredHeld));
        typeof(SamusState).GetProperty("PreviousDrawNewInput")!.SetValue(samus, W(NativeSnapshotMemory.SamusFilteredNew));
        typeof(SamusState).GetProperty("AutoJumpTimer")!.SetValue(samus, W(NativeSnapshotMemory.SamusAutoJumpTimer));
        typeof(SamusState).GetProperty("PreviousHealthForHurtCheck")!.SetValue(samus, W(NativeSnapshotMemory.SamusPreviousHealthForFlash));
        AssertTrue(W(NativeSnapshotMemory.SamusInputHandler) is
            NativeSnapshotMemory.SamusNormalInputHandler or NativeSnapshotMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        typeof(SamusState).GetProperty(nameof(samus.AutoJumpInputPending))!.SetValue(samus,
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
        samus.Pose = (byte)W(NativeSnapshotMemory.Pose);
        samus.XPosition = W(NativeSnapshotMemory.X);
        samus.YPosition = W(NativeSnapshotMemory.Y);
        samus.Kinematics.XSubposition = W(NativeSnapshotMemory.XFraction);
        samus.Kinematics.YSubposition = W(NativeSnapshotMemory.YFraction);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.SetAnimationFrameFromSpecialHandler(W(NativeSnapshotMemory.Animation), W(NativeSnapshotMemory.AnimationTimer));
        samus.PoseHistory.PreviousPose = W(NativeSnapshotMemory.PreviousPose);
        samus.PoseHistory.PreviousDirectionAndMovement = W(NativeSnapshotMemory.PreviousDirection);
        samus.PoseHistory.LastDifferentPose = W(NativeSnapshotMemory.LastDifferentPose);
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
        samus.Kinematics.YDirection = W(NativeSnapshotMemory.VerticalDirection);
        samus.Kinematics.XRadius = W(NativeSnapshotMemory.SamusXRadius);
        samus.Kinematics.YRadius = W(NativeSnapshotMemory.SamusYRadius);
        samus.Kinematics.YAcceleration = W(NativeSnapshotMemory.Gravity);
        samus.Kinematics.YSubacceleration = W(NativeSnapshotMemory.GravityFraction);
        samus.Kinematics.ExtraXDisplacement = W(NativeSnapshotMemory.ExtraXDisplacement);
        samus.Kinematics.ExtraXSubdisplacement = W(NativeSnapshotMemory.ExtraXDisplacementFraction);
        samus.Kinematics.ExtraYDisplacement = W(NativeSnapshotMemory.ExtraYDisplacement);
        samus.Kinematics.ExtraYSubdisplacement = W(NativeSnapshotMemory.ExtraYDisplacementFraction);
        samus.Kinematics.HorizontalSlopeCollisionEnable = W(NativeSnapshotMemory.SlopeCollisionEnable);
        samus.HorizontalSpeed.SpeedDivisor = W(NativeSnapshotMemory.SpeedDivisor);
        samus.HorizontalSpeed.ContactDamageIndex = W(NativeSnapshotMemory.ContactDamageIndex);
        samus.MorphBallBounceState = W(NativeSnapshotMemory.MorphBallBounceState);
        samus.BombJumpDirection = W(NativeSnapshotMemory.BombJumpDirection);


        foreach (var trail in runtime.Projectiles.TrailSlots)
        {
            typeof(SamusProjectileTrailSide).GetProperty("InstructionTimer")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftInstructionTimer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionTimer")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightInstructionTimer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionPointer")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftInstructionPointer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("InstructionPointer")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightInstructionPointer + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("TileNumberAttributes")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("TileNumberAttributes")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("XPosition")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftXPosition + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("XPosition")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightXPosition + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("YPosition")!.SetValue(trail.Left, W(NativeSnapshotMemory.TrailLeftYPosition + trail.NativeByteIndex));
            typeof(SamusProjectileTrailSide).GetProperty("YPosition")!.SetValue(trail.Right, W(NativeSnapshotMemory.TrailRightYPosition + trail.NativeByteIndex));
        }

        for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
        {
            ushort packed = W(NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
            samus.LiquidPhysics.AtmosphericEffects.SetSlot(index, (byte)(packed >> 8), (byte)packed,
                W(NativeSnapshotMemory.AtmosphericTimer + index * 2),
                W(NativeSnapshotMemory.AtmosphericX + index * 2), W(NativeSnapshotMemory.AtmosphericY + index * 2));
        }
        typeof(SamusState).GetProperty(nameof(samus.AnimationFrameBuffer))!.SetValue(samus, W(NativeSnapshotMemory.AnimationFrameBuffer));
        typeof(SamusLiquidPhysicsState).GetProperty("LiquidPhysicsType")!.SetValue(samus.LiquidPhysics, W(NativeSnapshotMemory.LiquidPhysicsType));
        typeof(SamusLiquidPhysicsState).GetProperty("PeriodicSubDamage")!.SetValue(samus.LiquidPhysics, W(NativeSnapshotMemory.PeriodicSubDamage));
        typeof(SamusLiquidPhysicsState).GetProperty("PeriodicDamage")!.SetValue(samus.LiquidPhysics, W(NativeSnapshotMemory.PeriodicDamage));

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialPlms = (Array)typeof(RoomPlmSystem).GetField("_slots", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(runtime.Plms)!;
        for (int index = 0; index < initialPlms.Length; index++)
        {
            AssertTrue(W(NativeSnapshotMemory.PlmHeaders + index * 2) == 0, "native initial PLM pool is empty");
            object slot = initialPlms.GetValue(index)!;
            slot.GetType().GetProperty("Active")!.SetValue(slot, false);
        }
        foreach (var projectile in runtime.Enemies.EnemyProjectiles)
            ImportNativeEnemyProjectile(projectile, W);
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(NativeSnapshotMemory.Random));
        PrivateState.SetField(runtime.Enemies, "_randomEnemyCounter", W(NativeSnapshotMemory.MainEnemyRoutineCount));
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
            unchecked((int)((uint)W(NativeSnapshotMemory.TideOffset) << 16 | W(NativeSnapshotMemory.TideOffsetFraction))));
        typeof(RoomLayer3FxState).GetField("baseYSubposition", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(runtime.RoomLayer3Fx, W(NativeSnapshotMemory.LiquidBaseFraction));
        typeof(RoomLayer3FxState).GetProperty(nameof(RoomLayer3FxState.CurrentYPosition))!.SetValue(runtime.RoomLayer3Fx, W(NativeSnapshotMemory.AcidSurface));
        runtime.RoomLayer3Fx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
        string[] slotWords = ["EnemyDefinitionPointer", "XPosition", "XSubposition", "YPosition", "YSubposition", "XRadius", "YRadius", "Properties", "ExtraProperties", "AiHandlerBits", "Health", "SpritemapPointer", "Timer", "CurrentInstruction", "InstructionTimer", "PaletteIndex", "VramTilesIndex", "Layer", "FlashTimer", "FrozenTimer", "InvincibilityTimer", "ShakeTimer", "FrameCounter"];
        for (int index = 0; index < runtime.Enemies.Slots.Count; index++)
        {
            var slot = runtime.Enemies.Slots[index];
            int address = NativeSnapshotMemory.EnemyBase + index * 64;
            if (W(address) != 0)
                AssertTrue(slot.EnemyDefinitionPointer == W(address), "initial enemy species agrees with room population");
            for (int word = 0; word < slotWords.Length; word++)
                typeof(RoomEnemySlot).GetProperty(slotWords[word])!.SetValue(slot, W(address + word * 2));
            for (int word = 0; word < 6; word++)
                typeof(RoomEnemySlot).GetProperty("Variable" + (char)('A' + word))!.SetValue(slot, W(address + 48 + word * 2));
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnX = slot.VariableB; pipe.SpawnY = slot.VariableC;
                pipe.DelayOrCounter = slot.VariableD;
                pipe.AnimationState = (PipeBugAnimationSelector)slot.VariableE;
                pipe.EmergenceTopY = W(NativeSnapshotMemory.EnemyExtra + index * 64);
                pipe.InstalledAnimationState = (PipeBugAnimationSelector)W(NativeSnapshotMemory.EnemyExtraPreviousAnimation + index * 64);
            }
        }
        if (runtime.Enemies.MotherBrain is not null)
            ImportNativeMotherBrain(runtime, W);
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter))!.SetValue(runtime, W(NativeSnapshotMemory.NmiCounter));
        typeof(SuperMetroidRuntime).GetProperty(nameof(runtime.NmiFrameCounter8))!.SetValue(runtime, memory[NativeSnapshotMemory.NmiCounterByte]);
        samus.CollectedItems = W(NativeSnapshotMemory.CollectedItems); samus.CollectedBeams = W(NativeSnapshotMemory.CollectedBeams);
        samus.Missiles = W(NativeSnapshotMemory.Missiles); samus.MaxMissiles = W(NativeSnapshotMemory.MaxMissiles);
        samus.SuperMissiles = W(NativeSnapshotMemory.SuperMissiles); samus.MaxSuperMissiles = W(NativeSnapshotMemory.MaxSuperMissiles);
        samus.PowerBombs = W(NativeSnapshotMemory.PowerBombs); samus.MaxPowerBombs = W(NativeSnapshotMemory.MaxPowerBombs);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch(W(NativeSnapshotMemory.HeldInput));
        var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
        typeof(SuperMetroidGame).GetField("runtime", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime);
        typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
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
            PrivateState.InvokeStatic(typeof(RoomEnemySystem), "InitializeEnemyProjectileFromDefinition",
                projectile, kind, W(NativeSnapshotMemory.EnemyProjectileGraphics + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.PreInstruction), W(NativeSnapshotMemory.EnemyProjectilePreInstruction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.InstructionPointer), W(NativeSnapshotMemory.EnemyProjectileInstruction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.InstructionTimer), W(NativeSnapshotMemory.EnemyProjectileInstructionTimer + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.GeneralTimer), W(NativeSnapshotMemory.EnemyProjectileTimer + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.XSubposition), W(NativeSnapshotMemory.EnemyProjectileXFraction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.YSubposition), W(NativeSnapshotMemory.EnemyProjectileYFraction + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.Variable0), W(NativeSnapshotMemory.EnemyProjectileVariableE + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.Variable1), W(NativeSnapshotMemory.EnemyProjectileVariableF + index));
            PrivateState.SetProperty(projectile, nameof(RoomEnemyProjectileSlot.CollidedProjectileType), W(NativeSnapshotMemory.EnemyProjectileVariableG + index));
            // The current frame record is [timer][operand]; the list pointer already follows it.
            PrivateState.InvokeStatic(typeof(RoomEnemySystem), "SetEnemyProjectileVisualOperand",
                projectile, unchecked((ushort)(projectile.InstructionPointer - 2)));
            AssertEqual(W(NativeSnapshotMemory.EnemyProjectileRadius + index), (ushort)(projectile.XRadius | projectile.YRadius << 8),
                $"initial enemy projectile {projectile.SlotIndex} radii");
            ushort properties = projectile.Damage;
            if (projectile.DrawPriority == EnemyProjectileDrawPriority.High) properties |= NativeSnapshotMemory.EnemyProjectileHighDraw;
            if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
            if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
            if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
            AssertEqual(W(NativeSnapshotMemory.EnemyProjectileProperties + index), properties,
                $"initial enemy projectile {projectile.SlotIndex} properties");
        }
        foreach (var (property, address) in new[] {
            (nameof(RoomEnemyProjectileSlot.XPosition), NativeSnapshotMemory.EnemyProjectileX),
            (nameof(RoomEnemyProjectileSlot.YPosition), NativeSnapshotMemory.EnemyProjectileY),
            (nameof(RoomEnemyProjectileSlot.XVelocity), NativeSnapshotMemory.EnemyProjectileXVelocity),
            (nameof(RoomEnemyProjectileSlot.YVelocity), NativeSnapshotMemory.EnemyProjectileYVelocity) })
            PrivateState.SetProperty(projectile, property, W(address + index));
    }
}

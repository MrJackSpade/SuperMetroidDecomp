using SuperMetroid.Diagnostics.Input;
using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Core.Assets;
using System.Buffers.Binary;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.TestSupport.MovieReplay.NativeSnapshotMotherBrainImport;
using static SuperMetroid.TestSupport.MovieReplay.NativeSnapshotPauseImport;
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
using SuperMetroid.Core.Enemies.Drawing;
namespace SuperMetroid.TestSupport.MovieReplay;

using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Programs;
using SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// Constructs a replay's initial production state from native WRAM and SRAM while
/// rejecting unsupported live state instead of repairing later replay divergence.
/// </summary>
internal static class NativeSnapshotImport
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
        var runtime = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.CreateRuntime(random: random);
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.Options.MoonwalkEnabled = W(NativeSnapshotMemory.MoonwalkOption) != 0;
        // The initial import reads the native progression tables through the cartridge's table codec.
        PersistentProgress progress = runtime.System.Progress;
        ProgressionTableCodec.ReadCollectedItems(memory.AsSpan(NativeSnapshotMemory.CollectedItemBits, ProgressionTableCodec.RoomFlagByteCount), progress);
        ProgressionTableCodec.ReadAreaBosses(memory.AsSpan(NativeSnapshotMemory.BossBits, ProgressionTableCodec.AreaBossByteCount), progress);
        ProgressionTableCodec.ReadEvents(memory.AsSpan(NativeSnapshotMemory.Events, ProgressionTableCodec.EventByteCount), progress);
        ProgressionTableCodec.ReadOpenedDoors(memory.AsSpan(NativeSnapshotMemory.OpenedDoors, ProgressionTableCodec.RoomFlagByteCount), progress);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.Get(W(NativeSnapshotMemory.Room)), new Position16<RoomPixels>(W(NativeSnapshotMemory.CameraX), W(NativeSnapshotMemory.CameraY)));
        ScrollBoundaryCamera camera = runtime.Room.LoadedCamera;
        // The room load wrote only the integer layer-one words; import the native fractions,
        // ideal position and speed words as whole camera values.
        camera.Position = camera.Position.WithFractionsOf(FixedPosition<RoomPixels>.FromStoredParts(
                0, W(NativeSnapshotMemory.CameraXFraction), 0, W(NativeSnapshotMemory.CameraYFraction)));
        camera.IdealPosition = new Position16<RoomPixels>(W(NativeSnapshotMemory.IdealCameraX), W(NativeSnapshotMemory.IdealCameraY));
        camera.Speed = FixedVector<RoomPixels>.FromStoredParts(
                W(NativeSnapshotMemory.CameraSpeedX), W(NativeSnapshotMemory.CameraSpeedXFraction),
                W(NativeSnapshotMemory.CameraSpeedY), W(NativeSnapshotMemory.CameraSpeedYFraction));
        runtime.Room.LoadedCamera.FinishSamusScrolling(FixedPosition<RoomPixels>.FromStoredParts(
            W(NativeSnapshotMemory.PreviousSamusX), W(NativeSnapshotMemory.PreviousSamusXFraction),
            W(NativeSnapshotMemory.PreviousSamusY), W(NativeSnapshotMemory.PreviousSamusYFraction)));
        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.Room.LevelData ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load room collision data.");
        // The native snapshot holds the room's level words and behaviour bytes row by row.
        for (int index = 0; index < level.Size.Area; index++)
        {
            Position16<RoomBlocks> block = level.Size.CellAtRowMajorIndex(index);
            level.SetTerrain(block, RoomLevelWordCodec.Decode(W(NativeSnapshotMemory.Level + index * sizeof(ushort))));
            level.SetBehavior(block, RoomBlockBehaviorCodec.Decode(memory[NativeSnapshotMemory.Bts + index]));
        }

        // Each live native object becomes a running colour effect. The handler visits its objects from the last down, so
        // the effects join in that order and take their turns in it (ColourEffectTurnOrder); the effects the room load
        // started are replaced by the recorded ones.
        var importedEffects = runtime.RoomPaletteFx.effects;
        importedEffects.Clear();
        Dictionary<PaletteFxSegment, PaletteFxNativeList>? paletteFxLists = null;
        for (int visit = 0; visit < NativeSnapshotMemory.PaletteFxObjectCount; visit++)
        {
            int slot = NativeSnapshotMemory.PaletteFxObjectCount - 1 - visit;
            ushort id = W(NativeSnapshotMemory.PaletteFxId + slot * 2);
            if (id == 0)
                continue;
            PaletteFxObject effect = PaletteFxObjectAccess.FromNative(id);
            // A live object resumes its typed animation where the recorded native pointer stands: in the animation its start
            // plays, or one its conditions restart it at; its native timer is its repeat counter.
            PaletteFxImportedPlace resumed = PaletteFxNativeProgramOracle.PlaceAt(
                paletteFxLists ??= PaletteFxNativeProgramOracle.Lists(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus)),
                ImportedAnimationCandidates(effect), W(NativeSnapshotMemory.PaletteFxInstruction + slot * 2));
            var imported = new RoomColourEffect(effect, resumed.Animation)
            {
                Place = resumed.Place,
                // The native colour index names the colours the object animates: of the colour groups its programs name,
                // the one starting there.
                Colours = SuperMetroid.Core.Fx.PaletteFx.PaletteFxColoursPositions.FromCartridge(effect,
                    RoomPaletteFxSystemConstants.PaletteFxColorIndex(W(NativeSnapshotMemory.PaletteFxColor + slot * 2), id)),
                // The pre-instruction values are arbitrary (#1415): a live object's routine word translates through the native
                // view, where the deleted adjacent-slot inspection (#1526) stands for the none the port's glow keeps.
                Condition = SuperMetroid.Core.Fx.FxNativeWords.RecordedColourEffectCondition(
                    W(NativeSnapshotMemory.PaletteFxFrameBehavior + slot * 2)),
                UpdatesUntilNextStep = W(NativeSnapshotMemory.PaletteFxFrameHold + slot * 2),
                Repeats = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.PaletteFxTimer + slot * 2)),
            };
            ColourEffectTurnOrder.Join(importedEffects, imported);
        }
        runtime.PaletteFxSession.PublishedHeatPhase = new SuperMetroid.Core.Fx.PaletteFx.HeatPhase(W(NativeSnapshotMemory.HeatPalettePhase));
        runtime.PaletteFxSession.ShimmerHeatPhase = new SuperMetroid.Core.Fx.PaletteFx.HeatPhase(W(NativeSnapshotMemory.PreviousHeatPalettePhase));
        ushort[] hudWords = [.. Enumerable.Range(0, HudStateConstants.MutableTileCount).Select(index => W(NativeSnapshotMemory.HudTilemap + index * 2))];
        HudItem highlightedHudItem = SaveSlotNumbering.SavedHudItem(W(NativeSnapshotMemory.PreviousHudSelection));
        // Each recorded scroll byte becomes the cell the camera reads it as.
        for (int cell = 0; cell < RoomScrollGrid.KeptCellCount; cell++)
            runtime.Room.LoadedCamera.Scrolls.SetCell(cell, RoomScrollStates.FromStorageByte(memory[NativeSnapshotMemory.ScrollStorage + cell]));
        runtime.Hud.MinimapDisabled = W(NativeSnapshotMemory.MinimapDisabled) != 0;
        // The current area's exploration lives in its live plane; every other area's in the saved planes.
        for (int area = 0; area < SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.Count; area++)
        {
            int plane = area == W(NativeSnapshotMemory.CurrentArea) ? NativeSnapshotMemory.LiveExploredMap :
                NativeSnapshotMemory.SavedExploredMaps + area * ProgressionTableCodec.ExploredPlaneByteCount;
            ProgressionTableCodec.ReadExploredPlane(memory.AsSpan(plane, ProgressionTableCodec.ExploredPlaneByteCount), SuperMetroid.AssetExtraction.Progression.AreaNumberCodec.At(area), progress);
        }
        ProgressionTableCodec.ReadUsedStations(memory.AsSpan(NativeSnapshotMemory.SaveElevatorMarkers, ProgressionTableCodec.UsedStationByteCount), progress);
        ProgressionTableCodec.ReadAreaMaps(memory.AsSpan(NativeSnapshotMemory.MapStationMarkers, ProgressionTableCodec.MapStationByteCount), progress);

        SamusState samus = runtime.Player.Samus ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load Samus.");
        samus.Control.Unlock();
        // The recorded top index names the picture the body last drew; it is imported as the whole picture of the pose and
        // frame whose top half shows it (#1513, #1585), which is composed of the bottom half that frame draws.
        // The compositions the picture is found by are import data (#1638): the cartridge-derived extraction's are read.
        SamusBodySources bodyArtwork = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusBody;
        if (!ReferenceEquals(samus.Presentation.Artwork, SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.SamusBody.Gameplay))
            throw new InvalidDataException("The native Ridley checkpoint requires the installation's Samus body artwork.");
        SamusBodyHalfFrame drawnTop = SamusSpritemapNative.Frame(bodyArtwork.Spritemaps, SamusPoseNumbering.FromNumber(W(NativeSnapshotMemory.Pose)), SamusBodyHalf.Top, W(NativeSnapshotMemory.SamusTopSpritemap))
            ?? throw new InvalidDataException("The native Ridley checkpoint names no top body picture.");
        samus.Presentation.DrawnFrame = new SamusBodyFrame(drawnTop.Pose, drawnTop.AnimationFrame);
        samus.Presentation.SpritemapPosition = new Position16<ScreenPixels>(W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY));
        samus.ArmCannon.Frame = W(NativeSnapshotMemory.CannonFrame);
        samus.ArmCannon.StableSelectionUpdates = W(NativeSnapshotMemory.CannonToggle);
        samus.ArmCannon.DrawOrder = SuperMetroid.AssetExtraction.Samus.SamusArmCannonDrawOrderCodec.FromDrawingMode(W(NativeSnapshotMemory.CannonDrawingMode));
        samus.ArmCannon.IsOpen = memory[NativeSnapshotMemory.CannonFlags] != 0;
        samus.ArmCannon.IsTransitioning = memory[NativeSnapshotMemory.CannonFlags + 1] != 0;
        new ProjectileInheritanceRecordsAccess(samus.ProjectileInheritance).Load(memory.AsSpan(NativeSnapshotMemory.ProjectileInheritancePrefix,
            NativeSnapshotMemory.SamusSlopeAdjusted - NativeSnapshotMemory.ProjectileInheritancePrefix));
        samus.Kinematics.PositionAdjustedBySlope = W(NativeSnapshotMemory.SamusSlopeAdjusted) != 0;
        for (int direction = 0; direction < 4; direction++)
            samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirectionNativeWords.Directions.Decode(direction, "solid-enemy collision direction"),
                SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.SlotOrNoneOf(W(NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2)));
        samus.HorizontalSpeed.ActiveSpeedTableBaseAddress = W(NativeSnapshotMemory.HorizontalSpeedTable);
        samus.HorizontalSpeed.DecelerationMultiplier = memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier];
        samus.HorizontalSpeed.EchoSoundFlag = W(NativeSnapshotMemory.SpeedEchoSoundLatch);
        samus.HorizontalSpeed.TotalSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.TotalHorizontalSpeed), W(NativeSnapshotMemory.TotalHorizontalSubspeed));
        runtime.Projectiles.SlotTable.ProjectileCounter = W(NativeSnapshotMemory.ProjectileCount);
        runtime.Projectiles.Charge.PreviousCharge = SamusSnapshotWords.DecodeBeamCharge(W(NativeSnapshotMemory.PreviousCharge));
        runtime.Projectiles.Firing.ProjectileInvincibilityTimer = W(NativeSnapshotMemory.ProjectileInteractionImmunity);
        runtime.Projectiles.Charge.ChargedShotGlow = SamusSnapshotWords.DecodeChargedShotGlow(W(NativeSnapshotMemory.ChargedShotGlow));
        int chargePaletteByteOffset = W(NativeSnapshotMemory.ChargePaletteIndex);
        runtime.Projectiles.Charge.ChargePaletteStep = (chargePaletteByteOffset & 1) == 0
            ? chargePaletteByteOffset / sizeof(ushort)
            : throw new InvalidDataException($"Native charge palette byte offset {chargePaletteByteOffset} names no step.");
        runtime.BombProjectiles.BombCounter = W(NativeSnapshotMemory.BombCount);
        samus.BombSpreadChargeTimeoutCounter = W(NativeSnapshotMemory.BombSpreadChargeTimeout);
        samus.PendingTransitionShot = SamusSnapshotWords.DecodeTransitionShot(W(NativeSnapshotMemory.PoseShotDirection));
        samus.HyperBeamEnabled = SamusSnapshotWords.DecodeHyperBeam(W(NativeSnapshotMemory.HyperBeam));
        samus.ResumeChargingBeamSoundPending = SamusSnapshotWords.DecodeResumeChargingBeamSound(W(NativeSnapshotMemory.ResumeChargeSound));
        samus.InputSnapshot.PreviousDrawHeldInput = ControllerWordCodec.FromJoypadWord(W(NativeSnapshotMemory.SamusFilteredHeld), "native Samus filtered held input");
        samus.InputSnapshot.PreviousDrawNewInput = ControllerWordCodec.FromJoypadWord(W(NativeSnapshotMemory.SamusFilteredNew), "native Samus filtered new input");
        samus.InputSnapshot.AutoJumpTimer = W(NativeSnapshotMemory.SamusAutoJumpTimer);
        samus.PreviousHealthForHurtCheck = SamusCountWords.Decode(W(NativeSnapshotMemory.SamusPreviousHealthForFlash));
        VerificationAssertions.AssertTrue(W(NativeSnapshotMemory.SamusInputHandler) is
            NativeSnapshotMemory.SamusNormalInputHandler or NativeSnapshotMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        samus.InputSnapshot.AutoJumpInputPending = W(NativeSnapshotMemory.SamusInputHandler) == NativeSnapshotMemory.SamusAutoJumpInputHandler;
        samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(W(NativeSnapshotMemory.Items));
        samus.EquippedBeams = SamusEquipmentWordCodec.FromBeamWord(W(NativeSnapshotMemory.Beams));
        samus.Health = SamusCountWords.Decode(W(NativeSnapshotMemory.Health));
        samus.MaxHealth = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxHealth));
        samus.InvincibilityTimer = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.InvincibilityTimer));
        samus.KnockbackTimer = W(NativeSnapshotMemory.KnockbackTimer);
        samus.KnockbackDirection = (SamusKnockbackDirection)W(NativeSnapshotMemory.KnockbackDirection);
        samus.KnockbackXDirection = (KnockbackHorizontalDirection)W(NativeSnapshotMemory.KnockbackXDirection);
        samus.HurtFlash.HoldAt(W(NativeSnapshotMemory.HurtFlashCounter));
        samus.SubunitHealth = W(NativeSnapshotMemory.SubunitHealth);
        samus.SelectedHudItem = SaveSlotNumbering.SavedHudItem(W(NativeSnapshotMemory.SelectedHudItem));
        samus.ArmCannon.PreviousSelectedHudItem = samus.SelectedHudItem;
        samus.AutoCancelHudItemIndex = SaveSlotNumbering.SavedHudItem(W(NativeSnapshotMemory.AutoCancelHudItemIndex));
        samus.ReserveTankMode = SaveSlotNumbering.SavedReserveMode(W(NativeSnapshotMemory.ReserveMode));
        samus.MaxReserveEnergy = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxReserve));
        samus.ReserveEnergy = SamusCountWords.Decode(W(NativeSnapshotMemory.Reserve));
        samus.Pose = SamusPoseNumbering.FromNumber(W(NativeSnapshotMemory.Pose));
        samus.Position = samus.Position.WithWhole(new(W(NativeSnapshotMemory.X), W(NativeSnapshotMemory.Y)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(W(NativeSnapshotMemory.XFraction)).WithFractionY(W(NativeSnapshotMemory.YFraction));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, W(NativeSnapshotMemory.Animation), W(NativeSnapshotMemory.AnimationTimer));
        samus.PoseHistory.PreviousPose = SamusPoseNumbering.FromNumber(W(NativeSnapshotMemory.PreviousPose));
        samus.PoseHistory.PreviousMovement = SamusSnapshotWords.DecodeHistoryMovement(W(NativeSnapshotMemory.PreviousDirection));
        samus.PoseHistory.LastDifferentPose = SamusPoseNumbering.FromNumber(W(NativeSnapshotMemory.LastDifferentPose));
        samus.PoseHistory.LastDifferentMovement = SamusSnapshotWords.DecodeHistoryMovement(W(NativeSnapshotMemory.LastDifferentDirection));
        samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.BaseSpeed), W(NativeSnapshotMemory.BaseFraction));
        samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.ExtraSpeed), W(NativeSnapshotMemory.ExtraFraction));
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationModeImport.FromNative(W(NativeSnapshotMemory.AccelerationMode));
        samus.HorizontalSpeed.HasRunningMomentum = W(NativeSnapshotMemory.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = SamusSnapshotWords.DecodeSpeedBoostCounter(W(NativeSnapshotMemory.BoostCounter));
        samus.Kinematics.YSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.VerticalSpeed), W(NativeSnapshotMemory.VerticalFraction));
        samus.Kinematics.YDirection = ClosedEnumWordDecoding.Decode<SamusVerticalDirection>(
            W(NativeSnapshotMemory.VerticalDirection), "Samus vertical direction");
        samus.Kinematics.Radius = new(W(NativeSnapshotMemory.SamusXRadius), W(NativeSnapshotMemory.SamusYRadius));
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.Gravity), W(NativeSnapshotMemory.GravityFraction));
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromStoredParts(W(NativeSnapshotMemory.ExtraXDisplacement), W(NativeSnapshotMemory.ExtraXDisplacementFraction), W(NativeSnapshotMemory.ExtraYDisplacement), W(NativeSnapshotMemory.ExtraYDisplacementFraction));
        if (!SamusSnapshotWords.AlignsToSlopes(W(NativeSnapshotMemory.SlopeCollisionEnable)))
            throw new InvalidDataException(
                $"Native slope-alignment word ${W(NativeSnapshotMemory.SlopeCollisionEnable):X4} disables the alignment, which no retail state does.");
        samus.HorizontalSpeed.SpeedDivisor = W(NativeSnapshotMemory.SpeedDivisor);
        samus.HorizontalSpeed.ContactDamageIndex = ClosedEnumWordDecoding.Decode<SamusContactDamageIndex>(
            W(NativeSnapshotMemory.ContactDamageIndex), "Samus contact damage index");
        samus.MorphBallBounceState = SamusSnapshotWords.DecodeBallBounce(W(NativeSnapshotMemory.MorphBallBounceState));
        samus.BombJumpRequest = SuperMetroid.TestSupport.VerificationImporters.SamusBombJumpRequestImport.FromNative(W(NativeSnapshotMemory.BombJumpDirection));

        // The checkpoint's live native trail places become trails, which show the pictures of the beams equipped there, as
        // its projectile graphics hold them (#1586); a place whose sparkles have both ended holds no trail.
        runtime.Projectiles.Trails.Clear();
        for (int offset = 0; offset < NativeSnapshotMemory.TrailRightFrameHold - NativeSnapshotMemory.TrailLeftFrameHold; offset += 2)
        {
            int leftHold = W(NativeSnapshotMemory.TrailLeftFrameHold + offset);
            int rightHold = W(NativeSnapshotMemory.TrailRightFrameHold + offset);
            if (leftHold == 0 && rightHold == 0)
                continue;
            var trail = runtime.Projectiles.AddTrail(SamusBeamCombinations.Of(samus.EquippedBeams));
            trail.Left.FrameHold = leftHold;
            trail.Right.FrameHold = rightHold;
            trail.Left.InstructionPointer = W(NativeSnapshotMemory.TrailLeftInstructionPointer + offset);
            trail.Right.InstructionPointer = W(NativeSnapshotMemory.TrailRightInstructionPointer + offset);
            trail.Left.ShownPose = ProjectileTrailNative.ShownPose(W(NativeSnapshotMemory.TrailLeftTileNumberAttributes + offset));
            trail.Right.ShownPose = ProjectileTrailNative.ShownPose(W(NativeSnapshotMemory.TrailRightTileNumberAttributes + offset));
            trail.Left.Position = new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailLeftXPosition + offset), W(NativeSnapshotMemory.TrailLeftYPosition + offset));
            trail.Right.Position = new Position16<RoomPixels>(
                W(NativeSnapshotMemory.TrailRightXPosition + offset), W(NativeSnapshotMemory.TrailRightYPosition + offset));
        }

        // The native places become Samus's particles and the waits her breath bubbles and lava spray keep; an empty
        // place's leftover timer and position are not state the port keeps.
        samus.Presentation.Particles.Clear();
        samus.LiquidPhysics.EffectWaits.Clear();
        for (int place = 0; place < SamusAtmosphericEffectWordCodec.PlaceCount; place++)
        {
            SamusAtmosphericEffectWordCodec.Restore(samus.Presentation.Particles, samus.LiquidPhysics.EffectWaits, place,
                W(NativeSnapshotMemory.AtmosphericFrameAndType + place * 2),
                W(NativeSnapshotMemory.AtmosphericTimer + place * 2),
                new(W(NativeSnapshotMemory.AtmosphericX + place * 2), W(NativeSnapshotMemory.AtmosphericY + place * 2)));
        }
        samus.AnimationFrameBuffer = W(NativeSnapshotMemory.AnimationFrameBuffer);
        samus.LiquidPhysics.LiquidPhysicsType = ClosedEnumWordDecoding.Decode<SamusLiquidMedium>(W(NativeSnapshotMemory.LiquidPhysicsType), "liquid physics type");
        samus.LiquidPhysics.PendingPeriodicDamage = new SuperMetroid.Core.Spatial.UnsignedSixteenSixteen(W(NativeSnapshotMemory.PeriodicDamage), W(NativeSnapshotMemory.PeriodicSubDamage));

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialObjects = runtime.RoomObjects.SlotTable.InProcessingOrder;
        for (int index = 0; index < initialObjects.Count; index++)
        {
            VerificationAssertions.AssertTrue(W(NativeSnapshotMemory.RoomObjectHeaders + index * 2) == 0, "native initial PLM pool is empty");
            initialObjects[index].Delete();
        }
        foreach (var projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            ImportNativeEnemyProjectile(projectile, runtime.Enemies.GraphicsSet, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W);
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(NativeSnapshotMemory.Random));
        runtime.Enemies.FrameContext.SeedRandomEnemyCounter(W(NativeSnapshotMemory.MainEnemyRoutineCount));
        // The liquid's BG3 HDMA object has already run its first pass, so its callback is
        // installed. A live object owns the per-frame liquid motion; once Mother Brain's
        // $A9:8C0C has cleared the channel flags, the room's liquid objects are deleted.
        bool lavaAcidObjectLive = Enumerable.Range(0, NativeSnapshotMemory.HdmaObjectCount).Any(slot =>
            W(NativeSnapshotMemory.HdmaObjectChannelBitflags + slot * 2) != 0 &&
            W(NativeSnapshotMemory.HdmaObjectFrameBehaviors + slot * 2) == NativeSnapshotMemory.AcidHdmaCallback);
        VerificationAssertions.AssertTrue(runtime.RoomFxOverlay.Type is RoomFxType.Lava or RoomFxType.Acid,
            "snapshot import models the lava/acid HDMA objects of its room");
        runtime.RoomFxOverlay.LavaAcidOverlayFrameBehaviorInstalled = true;
        runtime.RoomFxOverlay.LiquidMotionStopped = !lavaAcidObjectLive;
        runtime.RoomFxOverlay.TidePhase = RotationAngle.FromRaw(W(NativeSnapshotMemory.TidePhase));
        runtime.RoomFxOverlay.TideOffset = SixteenSixteenWords.ToDisplacement(W(NativeSnapshotMemory.TideOffset), W(NativeSnapshotMemory.TideOffsetFraction));
        runtime.RoomFxOverlay.BaseY = (runtime.RoomFxOverlay.BaseY
            ?? throw new InvalidDataException("The imported acid room's effect names no surface.")).WithFraction(W(NativeSnapshotMemory.LiquidBaseFraction));
        // The surface word is a signed line: one a tide carries above the room's top is negative.
        runtime.RoomFxOverlay.Surface = CoordinateAxis.AsSigned(W(NativeSnapshotMemory.AcidSurface));
        samus.LiquidPhysics.ConfigureFor(runtime.RoomFxOverlay);
        var pipeBugPointers = new PipeBugNativeInstructionPointers(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus));
        var kraidHeadCursors = new KraidHeadNativeCursors(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus));
        // EnemyData +$0E onward; +$00 is the identity, +$02..+$08 the 16.16 position and
        // +$0A/+$0C the radii, each imported as one typed value below.
        for (int index = 0; index < runtime.Enemies.SlotTable.Slots.Count; index++)
        {
            var slot = runtime.Enemies.SlotTable.Slots[index];
            int address = NativeSnapshotMemory.EnemyBase + SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(index);
            if (W(address) != 0)
                VerificationAssertions.AssertTrue(SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(slot.DefinitionId) == W(address), "initial enemy species agrees with room population");
            // A recorded occupant is the one the room load placed, which keeps its species state; a slot the recording
            // holds free loses its loaded occupant with that occupant's state (#1543).
            EnemyDefinitionId recordedOccupant = SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionSlotWords.FromSlotWord(W(address));
            if (recordedOccupant != slot.DefinitionId)
                slot.Bind(recordedOccupant, slot.Definition);
            slot.Position = FixedPosition<RoomPixels>.FromStoredParts(W(address + 2), W(address + 4), W(address + 6), W(address + 8));
            slot.Radius = new(W(address + 10), W(address + 12));
            // The species bits of the property words are the entry's spawn settings, which the population already holds.
            SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.EngineStateOf(W(address + 14), W(address + 16)).ApplyTo(slot);
            slot.AiStates = SuperMetroid.AssetExtraction.Enemies.EnemyAiStatesWordCodec.Decode(W(address + 18));
            slot.Health = W(address + 20);
            // A free native slot keeps its last occupant's word in that occupant's bank; a free port slot shows no frame.
            slot.SpritemapPointer = W(address) == 0 ? (ushort)0 : W(address + 22);
            slot.AnimationClock.Repeats = WrappingCountdown.Start(W(address + 24));

            slot.AnimationClock.HoldFor(W(address + 28));
            // The native palette word names a row; the colouring it denotes is decoded under the room's loaded set.
            slot.SetPaletteRow(runtime.Enemies, NativeSpriteGraphicsWords.PaletteRow(W(address + 30)));
            // The native tile index word is where the import composes the occupant's sheet, not slot state (#1513).
            slot.FlashTimer = W(address + 36);
            slot.FrozenTimer = W(address + 38);
            slot.InvincibilityTimer = W(address + 40);
            slot.ShakeTimer = W(address + 42);
            slot.FrameCounter = W(address + 44);
            var words = new NativeEnemyRecordWords(
                W(address + 48), W(address + 50), W(address + 52), W(address + 54), W(address + 56), W(address + 58));
            // A free native slot's family words are its last occupant's, which no family state of this slot holds (#1435).
            if (W(address) != 0)
                NativeEnemyFamilyStateImport.Import(runtime.Enemies, slot, words);
            // A recorded swoop path's length is word C; before Draygon plans his first swoop it is zero and he has none.
            if (W(address) != 0 && slot.SpeciesState<SuperMetroid.Core.Bosses.Draygon.DraygonEnemyState>() is { } draygon && words.C != 0)
                draygon.SwoopPath = SuperMetroid.Core.Bosses.Draygon.DraygonNativeWords.SwoopPathAt(W, words.C);
            // Kraid's head program is typed and kept in the encounter state; the recorded body word is its native cursor,
            // which only this import translates. Before the first attack the word names no head record and the head stays unstarted.
            if (slot.DefinitionId == EnemyDefinitionId.Kraid && runtime.Enemies.KraidBehavior.Access.Kraid is { } kraid)
                kraid.HeadPlayback = kraidHeadCursors.TryPlaybackAt(words.B);
            // A Pipe Bug's animation runs as a typed program (#641): resume it where the recorded pointer stands.
            if (runtime.Enemies.PipeBugStates[index] is { } pipeBug)
                pipeBug.Playback = pipeBugPointers.PlaybackAt(W(address + MovieDesyncMemory.EnemyAnimationScriptOffset));
            if (runtime.Enemies.PipeBugStates[index] is { IsBrinstar: true } pipe)
            {
                pipe.SpawnPosition = new(words.B, words.C);
                pipe.RespawnDelay = unchecked((short)words.D);
                pipe.RequestedAnimation = PipeBugsNativeWords.BrinstarAnimations.Decode(words.E, "recorded Pipe Bug requested animation");
                pipe.EmergenceTop = pipe.SpawnPosition.WithY(W(NativeSnapshotMemory.EnemyExtra + index * 64));
                pipe.InstalledAnimation = PipeBugsNativeWords.BrinstarAnimations.Decode(
                    W(NativeSnapshotMemory.EnemyExtraPreviousAnimation + index * 64), "recorded Pipe Bug installed animation");
            }
        }
        if (runtime.Enemies.MotherBrainEncounter.MotherBrain is not null)
            ImportNativeMotherBrain(runtime, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportSource.Require(bus), W);
        runtime.Clock.Import(memory[NativeSnapshotMemory.NmiCounterByte], W(NativeSnapshotMemory.NmiCounter));
        samus.CollectedItems = SamusEquipmentWordCodec.FromEquipmentWord(W(NativeSnapshotMemory.CollectedItems)); samus.CollectedBeams = SamusEquipmentWordCodec.FromBeamWord(W(NativeSnapshotMemory.CollectedBeams));
        samus.Missiles = SamusCountWords.Decode(W(NativeSnapshotMemory.Missiles)); samus.MaxMissiles = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxMissiles));
        samus.SuperMissiles = SamusCountWords.Decode(W(NativeSnapshotMemory.SuperMissiles)); samus.MaxSuperMissiles = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxSuperMissiles));
        samus.PowerBombs = SamusCountWords.Decode(W(NativeSnapshotMemory.PowerBombs)); samus.MaxPowerBombs = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxPowerBombs));
        // The HUD imports as the values its native rows show, once Samus's inventory has imported (#1466).
        runtime.Hud.ImportNative(hudWords, samus, highlightedHudItem, (runtime.Artwork.MapPresentation ?? throw new InvalidDataException(
            "The native checkpoint requires the installed HUD artwork.")).GameplayHud);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.Controller1.Latch(ControllerWordCodec.FromJoypadWord(W(NativeSnapshotMemory.HeldInput), "native held input"));
        var game = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(saves, renderGameplayFrames: false, hostServices: hostServices, random: random);
        game.AdoptRuntime(runtime);
        game.Session.LastAudioRoomState = runtime.Room.LoadedRoom.State;
        game.GameState = GameStateNumberCodec.Decode(W(NativeSnapshotMemory.GameState));
        // The room load showed the colours of the Samus the Ceres start made; the imported equipment names the suit and
        // beams whose colours the cartridge loaded (#1512), under which her palette-FX records are then laid.
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(runtime.Player.Colours, samus.EquippedItems);
        SamusProjectileSystem.LoadBeamPalette(runtime.Player.Colours, samus.EquippedBeams);
        ImportShownPaletteFxFrames(runtime);
        if (game.GameState == SuperMetroidGameState.Paused)
            ImportNativePauseMenu(game, runtime, W, memory);
        return new ImportNativeSnapshotResult(bus, runtime, samus, game);
    }

    /// <summary>
    /// Restores the frame each live palette-FX object shows, in native slot order, as its handler pass showed it
    /// (#1512, #1597): the effect holds the frame, which the room's pictures shade with; a frame of Samus's suit groups
    /// is laid over her suit and one of an enemy's groups is shown over its owner's colours, as the runtime does after the
    /// pass. Native colour memory already holds those frames' records at the checkpoint, while the imported slots only
    /// resume their programs, which show their next frame later.
    /// </summary>
    /// <exception cref="InvalidOperationException">A live object's frame lies in colours no gameplay picture shows.</exception>
    private static void ImportShownPaletteFxFrames(SuperMetroidRuntime runtime)
    {
        // Native slot order is the reverse of the order the effects take their turns in.
        foreach (RoomColourEffect effect in Enumerable.Reverse(runtime.RoomPaletteFx.effects))
        {
            if (effect.Colours is not { } colours || LastShownFrame(effect.Animation, effect.Place) is not { } frame)
                continue;
            effect.Frame = frame;
            if (PaletteFxRoomColourDefinitions.RoomColours.Contains(colours))
                continue;
            if (PaletteFxSamusColourDefinitions.SuitColours.Contains(colours))
                runtime.Player.Colours.LayOverSuit(PaletteFxShades.Of(colours, frame));
            else if (PaletteFxEnemyColourDefinitions.EnemyColours.Contains(colours))
                runtime.Enemies.PaletteFxShows.Show(colours, frame);
            else
                throw new InvalidOperationException($"Imported colour effect {effect.Effect} showed {colours}, which no gameplay picture shows.");
        }
    }

    /// <summary>
    /// The animations an imported <paramref name="effect"/> can be playing, its own first: the one its start plays (each
    /// suit's heat shimmer for an effect that chooses one), then those its conditions restart it at.
    /// </summary>
    private static IEnumerable<PaletteFxSegment> ImportedAnimationCandidates(PaletteFxObject effect)
    {
        RoomPaletteFxDefinition definition = RoomPaletteFxSelection.Get(effect);
        PaletteFxSegment[] starts = definition.Start == ColourEffectStart.ChoosesHeatAnimationForSuit
            ? [.. Enum.GetValues<PaletteFxHeatSuit>().Select(PaletteFxHeatAnimations.Start)]
            : [definition.InitialAnimation];
        var seen = new HashSet<PaletteFxSegment>();
        var pending = new Queue<PaletteFxSegment>(starts);
        while (pending.TryDequeue(out PaletteFxSegment start))
        {
            if (!seen.Add(start))
                continue;
            yield return start;
            foreach (AnimationPhase<PaletteFxSegment> phase in PaletteFxAnimations.StartingWith(start).Phases)
            {
                foreach (AnimationBeat<int, PaletteFxCue> beat in PaletteFxSegments.All.Beats(phase.Segment).Skip(phase.FirstBeat).Take(phase.BeatCount))
                {
                    if (beat is AnimationBeat<int, PaletteFxCue>.Event { Raised: PaletteFxCue.Watch { Condition: ColourEffectCondition.RestartsWhileSamusIsHigh high } })
                        pending.Enqueue(high.Restart);
                }
            }
        }
    }

    /// <summary>
    /// The frame a colour effect showed last before <paramref name="place"/> in <paramref name="animation"/>: the nearest
    /// frame before the beat it resumes at, in its segment. The native instruction pointer rests after the record it last
    /// showed (a loop's jump runs only when the object next advances). A list whose last record runs on into the next list
    /// leaves the pointer at that list's entry, so a place at a segment's first beat showed the last frame of the segment
    /// whose list runs on into it. A place with no frame before it, in its segment or any running on into it, has shown no
    /// frame yet: null.
    /// </summary>
    private static PaletteFxFrame? LastShownFrame(SuperMetroid.Core.Programs.Animation<PaletteFxSegment> animation, SuperMetroid.Core.Programs.AnimationPlace place)
    {
        AnimationPhase<PaletteFxSegment> phase = animation.Phases[place.Phase];
        PaletteFxSegment segment = phase.Segment;
        int end = phase.FirstBeat + place.Beat;
        PaletteFxSegment[] authored = Enum.GetValues<PaletteFxSegment>();
        while (true)
        {
            IReadOnlyList<AnimationBeat<int, PaletteFxCue>> beats = PaletteFxSegments.All.Beats(segment);
            for (int beat = Math.Min(end, beats.Count) - 1; beat >= 0; beat--)
            {
                if (beats[beat] is AnimationBeat<int, PaletteFxCue>.Frame shown)
                    return new PaletteFxFrame(segment, shown.Shown);
            }
            int index = Array.IndexOf(authored, segment);
            if (index == 0 || !SuperMetroid.AssetExtraction.Fx.PaletteFx.PaletteFxListLayout.RunOnIntoNext.Contains(authored[index - 1]))
                return null;
            segment = authored[index - 1];
            end = int.MaxValue;
        }
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
    /// Imports one enemy-projectile place from the native slot of the same index. A live slot's projectile is
    /// initialized from its definition, as <c>SpawnEprojInner</c> does, then takes every per-slot word the native arrays
    /// hold; the definition-derived radii and properties must already agree with them. A free slot's place holds a gone
    /// projectile at the slot's position and velocity words, which the next spawn there starts from.
    /// </summary>
    private static void ImportNativeEnemyProjectile(EnemyProjectile projectile, SuperMetroid.AssetExtraction.Enemies.RoomEnemyGraphicsSetDefinition graphicsSet, IImportCartridgeSource cartridge,
        Func<int, ushort> W)
    {
        int index = projectile.Place * 2;
        ushort id = W(NativeSnapshotMemory.EnemyProjectileId + index);
        if (id == 0)
        {
            // Room loading spawns the room's projectiles afresh; a slot the snapshot holds
            // empty (for example Mother Brain's destroyed turrets) must be empty here too.
            projectile.Delete();
        }
        else
        {
            RoomEnemyProjectileKind kind = EnemyProjectileNativeWords.Kinds.Decode(id, "enemy projectile definition");
            VerificationAssertions.AssertTrue(NativeSlotOnlyEnemyProjectileKinds.Contains(kind),
                $"initial native enemy projectile {projectile.Place} kind ${id:X4} has a verified slot-only import");
            // The snapshot's graphics word packs the projectile's tile base and palette row as an attribute word does: the
            // tile base names the owner whose sheet the projectile draws from (#1513), and the row is decoded into the
            // shading it denotes (#1512): a projectile at its spawner's characters draws in its spawner's row, its standard
            // colours, and one at character zero in the shared selection its row is.
            var graphics = new SnesObjAttributeWord(W(NativeSnapshotMemory.EnemyProjectileGraphics + index));
            int standardRow = graphics.TileNumber == 0 ? SuperMetroid.AssetExtraction.Enemies.EnemySheetRowDefinitions.Common : graphics.PaletteIndex;
            FamilyProjectileStarts.Initialize(projectile, kind, new SpriteGraphics(graphicsSet.OwnerDrawingAt(graphics.TileNumber),
                SpriteColouringAccess.ShadingInRow(graphics.PaletteIndex, standardRow)));
            projectile.FrameBehavior = EnemyProjectileFrameBehaviorAccess.FromWord(W(NativeSnapshotMemory.EnemyProjectileFrameBehavior + index));
            ushort instruction = W(NativeSnapshotMemory.NativeEnemyProjectileInstruction + index);
            // The typed family resumes at the step the recorded native pointer stands for.
            EnemyProjectileAnimation typed = projectile.Animation ?? throw new InvalidOperationException(
                $"Initial enemy projectile {projectile.Place} kind {kind} starts no typed program.");
            VerificationAssertions.AssertEqual(1, NativeEnemyProjectileFamilies.All.Count(family => family.TryResumeAt(cartridge, typed, instruction)),
                $"initial enemy projectile {projectile.Place} resumes its typed program");
            projectile.FrameHold = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.EnemyProjectileFrameHold + index));
            // The slot-only kinds keep nothing but their program's loop count in the native timer word.
            projectile.LoopCountdown = WrappingCountdown.FromRaw(W(NativeSnapshotMemory.EnemyProjectileTimer + index));
            projectile.Position = projectile.Position
                .WithFractionX(W(NativeSnapshotMemory.EnemyProjectileXFraction + index))
                .WithFractionY(W(NativeSnapshotMemory.EnemyProjectileYFraction + index));
            // The slot-only kinds keep no family state (#1470), so their native E, F and G words are not imported.
            // The current frame record is [timer][operand]; the list pointer already follows it.
            EnemyProjectileNativeImportAccess.ShowNativeOperand(
                projectile, unchecked((ushort)(instruction - 2)));
            VerificationAssertions.AssertEqual(W(NativeSnapshotMemory.EnemyProjectileRadius + index), LittleEndianBytes.Word(unchecked((byte)projectile.Radius.X), unchecked((byte)projectile.Radius.Y)),
                $"initial enemy projectile {projectile.Place} radii");
            ushort properties = checked((ushort)projectile.Damage);
            if (projectile.Depth == EnemyProjectileDepth.InFrontOfSamus) properties |= NativeSnapshotMemory.EnemyProjectileInFrontOfSamus;
            if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
            if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
            if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
            VerificationAssertions.AssertEqual(W(NativeSnapshotMemory.EnemyProjectileProperties + index), properties,
                $"initial enemy projectile {projectile.Place} properties");
        }
        projectile.Position = projectile.Position.WithWhole(new(
            W(NativeSnapshotMemory.EnemyProjectileX + index), W(NativeSnapshotMemory.EnemyProjectileY + index)));
        projectile.Velocity = new(
            new SignedEightEight(W(NativeSnapshotMemory.EnemyProjectileXVelocity + index)),
            new SignedEightEight(W(NativeSnapshotMemory.EnemyProjectileYVelocity + index)));
    }
}

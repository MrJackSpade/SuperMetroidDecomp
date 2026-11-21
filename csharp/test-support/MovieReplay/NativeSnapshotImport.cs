using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.SamusNativeCodecs;
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
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Shaktool;
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
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Animation;
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
        var game = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.CreateGame(saves, renderGameplayFrames: false, hostServices: hostServices, random: random);
        var runtime = SuperMetroid.TestSupport.RepositoryInstallation.RepositoryInstallation.CreateRuntime(random: random, audio: game.AudioForVerification);
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom(); runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.LoadedScope);
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
        global::SuperMetroid.Core.PrivateState.Set(camera, "Position", camera.Position.WithFractionsOf(FixedPosition<RoomPixels>.FromStoredParts(
                0, W(NativeSnapshotMemory.CameraXFraction), 0, W(NativeSnapshotMemory.CameraYFraction))));
        PrivateState.Set(camera, "IdealPosition", new Position16<RoomPixels>(W(NativeSnapshotMemory.IdealCameraX), W(NativeSnapshotMemory.IdealCameraY)));
        global::SuperMetroid.Core.PrivateState.Set(camera, "Speed", FixedVector<RoomPixels>.FromStoredParts(
                W(NativeSnapshotMemory.CameraSpeedX), W(NativeSnapshotMemory.CameraSpeedXFraction),
                W(NativeSnapshotMemory.CameraSpeedY), W(NativeSnapshotMemory.CameraSpeedYFraction)));
        runtime.Room.LoadedCamera.FinishSamusScrolling(FixedPosition<RoomPixels>.FromStoredParts(
            W(NativeSnapshotMemory.PreviousSamusX), W(NativeSnapshotMemory.PreviousSamusXFraction),
            W(NativeSnapshotMemory.PreviousSamusY), W(NativeSnapshotMemory.PreviousSamusYFraction)));
        // Preserve the native room's already-mutated doors and item blocks. Rebuilding
        // these from the pristine room header would no longer represent this movie frame.
        RoomLevelData level = runtime.Room.LevelData ?? throw new InvalidDataException(
            "The native Ridley checkpoint did not load room collision data.");
        // The native snapshot holds the room's level words and behaviour bytes row by row; the loaded room's area resolves
        // the bytes the areas give their own meanings (#1805).
        for (int index = 0; index < level.Size.Area; index++)
        {
            Position16<RoomBlocks> block = level.Size.CellAtRowMajorIndex(index);
            RoomTerrainBlock terrain = RoomLevelWordCodec.Decode(W(NativeSnapshotMemory.Level + index * sizeof(ushort)));
            level.SetTerrain(block, terrain);
            // A door block's behaviour byte is its place in the room's door list, which the installation already resolved
            // to the door it leads through (#2090); writing the byte as a behaviour would clear that door. A native door
            // block where the installed room has none fails loudly at its first touch, as an unresolved door does.
            if (terrain.Collision == RoomCollisionType.DoorBlock && level.ExitAt(index) is not null)
                continue;
            level.SetBehavior(block, RoomBlockBehaviorCodec.Decode(runtime.Room.LoadedRoom.Area, memory[NativeSnapshotMemory.Bts + index]));
        }

        // Each live native object becomes a running colour effect. The handler visits its objects from the last down, so
        // the effects join in that order and take their turns in it (ColourEffectSystem.effects); the effects the room load
        // started are replaced by the recorded ones.
        var importedEffects = runtime.RoomPaletteFx.effects;
        importedEffects.Clear();
        Dictionary<ushort, PaletteFxNativeList>? paletteFxLists = null;
        for (int visit = 0; visit < NativeSnapshotMemory.PaletteFxObjectCount; visit++)
        {
            int slot = NativeSnapshotMemory.PaletteFxObjectCount - 1 - visit;
            ushort id = W(NativeSnapshotMemory.PaletteFxId + slot * 2);
            if (id == 0)
                continue;
            ColourEffectId effect = PaletteFxObjectAccess.FromNative(id);
            // A live object resumes its typed animation where the recorded native pointer stands: in the animation its start
            // plays, one of the heat shimmer's for an effect that chooses one; its native timer counts the plays its repeated
            // phase has left (#2006).
            PaletteFxImportedPlace resumed = PaletteFxNativeProgramOracle.PlaceAt(
                paletteFxLists ??= PaletteFxNativeProgramOracle.Lists(bus),
                PaletteFxNativeProgramOracle.AnimationsOf(effect), W(NativeSnapshotMemory.PaletteFxInstruction + slot * 2));
            // A Torizo's belly glow watches the room's Torizo, which the room's load set up (#1700).
            ILitTorizo? litTorizo = effect is ColourEffectId.BombTorizoBelly or ColourEffectId.GoldenTorizoBelly
                ? runtime.Enemies.Places.InUpdateOrder.Select(enemy => enemy.Entity).OfType<TorizoEntity>().SingleOrDefault()
                    ?? throw new InvalidDataException("The native snapshot runs a Torizo's belly glow in a room with no Torizo.")
                : null;
            // The gunship's glow shows on the room's gunship, whose placement during the room's load started it (#1783), a
            // Torizo's belly glow on the Torizo it lights (#2240), and a sprite statue's grey-out on the entrance statue's
            // statue it greys (#2288).
            IGlowingEnemy? glowingEnemy = effect == ColourEffectId.GunshipGlow
                ? runtime.Enemies.Places.InUpdateOrder.Select(place => place.SpeciesState<SuperMetroid.Core.Enemies.Gunship.GunshipPieces>())
                    .OfType<SuperMetroid.Core.Enemies.Gunship.GunshipPieces>().SingleOrDefault()
                    ?? throw new InvalidDataException("The native snapshot runs the gunship's glow in a room with no gunship.")
                : effect is ColourEffectId.GreyOutTourianStatueRidley or ColourEffectId.GreyOutTourianStatuePhantoon
                    ? (runtime.Enemies.TourianStatue
                        ?? throw new InvalidDataException("The native snapshot greys a sprite statue in a room with no statues."))
                        .Actors.SpriteStatueGreyOut(SuperMetroid.Core.Areas.Tourian.TourianStatueAnimationDefinitions.Statues.Values.Single(statue => statue.ColourEffect == effect)
                            .GreyedSprite ?? throw new InvalidDataException($"The native snapshot greys {effect}, no sprite statue's grey-out."))
                    : litTorizo;
            // Each effect shows on the target its start hands it (#2544).
            ColourEffectTarget target = litTorizo is not null ? new ColourEffectTarget.OnLitTorizo(litTorizo)
                : glowingEnemy is not null ? new ColourEffectTarget.OnGlowingEnemy(glowingEnemy)
                : ColourEffectTarget.OfGameplay(effect);
            var imported = new RoomColourEffect(effect, resumed.FramesOf, target);
            // The effect's animation goes on from the recorded place, its shown frame holding for the recorded updates (#2488).
            AnimationPlayer<ColourEffectSegment, int, ColourEffectCue> play =
                PrivateState.Field<RoomColourEffect, AnimationPlayer<ColourEffectSegment, int, ColourEffectCue>>(imported, "play");
            play.GoOnFrom(NativeEnemyAnimationOracle.WithNativePlaysLeft(
                new AnimationPlayback<ColourEffectSegment>(resumed.Animation, resumed.Place), W(NativeSnapshotMemory.PaletteFxTimer + slot * 2)));
            PrivateState.Set(play, nameof(play.UpdatesLeft), (int)W(NativeSnapshotMemory.PaletteFxFrameHold + slot * 2));
            // The effect's colours and condition are its definition's (#1982). The native colour index names the colours the
            // object animates once its set-up has run, of the colour groups its programs name the one starting there; zero
            // before.
            if (SuperMetroid.Core.Fx.ColourEffects.PaletteFxColoursPositions.FromCartridge(effect,
                    RoomPaletteFxSystemConstants.PaletteFxColorIndex(W(NativeSnapshotMemory.PaletteFxColor + slot * 2), id)) is { } recordedColours &&
                recordedColours != imported.Colours)
            {
                throw new InvalidDataException($"The native snapshot's {effect} animates {recordedColours}, not its definition's {imported.Colours}.");
            }
            // The pre-instruction values are arbitrary (#1415): a live object's routine word translates through the native
            // view, where the deleted adjacent-slot inspection (#1526) stands for the none the port's glow keeps. A set-up
            // object watches its definition's condition; one not yet set up watches none.
            if (SuperMetroid.Core.Fx.FxNativeWords.RecordedColourEffectCondition(W(NativeSnapshotMemory.PaletteFxFrameBehavior + slot * 2))
                    is { } recordedCondition && recordedCondition != imported.Condition)
            {
                throw new InvalidDataException($"The native snapshot's {effect} watches {recordedCondition}, not its definition's {imported.Condition}.");
            }
            importedEffects.Add(imported);
        }
        runtime.ColourEffectSession.PublishedHeatPhase = SuperMetroid.Core.Fx.ColourEffects.HeatPhase.InOrder[W(NativeSnapshotMemory.HeatPalettePhase)];
        runtime.ColourEffectSession.ShimmerHeatPhase = SuperMetroid.Core.Fx.ColourEffects.HeatPhase.InOrder[W(NativeSnapshotMemory.PreviousHeatPalettePhase)];
        ushort[] hudWords = [.. Enumerable.Range(0, HudStateConstants.MutableTileCount).Select(index => W(NativeSnapshotMemory.HudTilemap + index * 2))];
        HudItem highlightedHudItem = SaveSlotNumbering.SavedHudItem(W(NativeSnapshotMemory.PreviousHudSelection));
        // Each recorded scroll byte becomes the kept cell the camera reads it as.
        runtime.Room.LoadedCamera.Scrolls.KeptCells.LoadAll(
            RoomScrollStates.FromStorageBytes(memory.AsSpan(NativeSnapshotMemory.ScrollStorage, RoomScrollGrid.KeptCellCount)));
        runtime.Hud.ImportMinimapDisabled(W(NativeSnapshotMemory.MinimapDisabled) != 0);
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
        PrivateState.Set(samus.Presentation, "DrawnFrame", new SamusBodyFrame(drawnTop.Pose, SamusAnimationNumbers.Frame(drawnTop.Pose, drawnTop.AnimationFrame)));
        PrivateState.Set(samus.Presentation, "DrawOrigin", Offset16<ScreenPixels>.Wrapped(W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY)));
        PrivateState.Set(samus.ArmCannon, "Cover", ArmCannonCoverOf(memory[NativeSnapshotMemory.CannonFlags] != 0,
            memory[NativeSnapshotMemory.CannonFlags + 1] != 0, SamusArmCannonCoverNativeCodec.FromNative(W(NativeSnapshotMemory.CannonFrame))));
        PrivateState.SetField(samus.ArmCannon, "stability",
            UpdateCountdownAccess.AtAge(SamusArmCannonDefinitions.StableSelectionUpdates, W(NativeSnapshotMemory.CannonToggle)));
        PrivateState.Set(samus.ArmCannon, nameof(SamusArmCannonState.DrawOrder), SuperMetroid.AssetExtraction.Samus.SamusArmCannonDrawOrderCodec.FromDrawingMode(W(NativeSnapshotMemory.CannonDrawingMode)));
        new ProjectileInheritanceRecordsAccess(samus.ProjectileInheritance).Load(memory.AsSpan(NativeSnapshotMemory.ProjectileInheritancePrefix,
            NativeSnapshotMemory.SamusSlopeAdjusted - NativeSnapshotMemory.ProjectileInheritancePrefix));
        global::SuperMetroid.Core.PrivateState.Set(samus.Kinematics, "PositionAdjustedBySlope", W(NativeSnapshotMemory.SamusSlopeAdjusted) != 0);
        // Each direction's recorded word names the enemy in that place of the loaded room, or none (#1702).
        for (int direction = 0; direction < 4; direction++)
            samus.Kinematics.RecordSolidEnemyCollision(SamusCollisionDirectionNativeWords.Directions.Decode(direction, "solid-enemy collision direction"),
                SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.SlotOrNoneOf(W(NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2)) is { } contactPlace
                    ? runtime.Enemies.Places.InUpdateOrder[contactPlace]
                    : null);
        samus.HorizontalSpeed.ActiveSpeedTableBaseAddress = W(NativeSnapshotMemory.HorizontalSpeedTable);
        // Only Crateria's ice reaction sets the multiplier, and no retail level places that block (#2443).
        if (memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier] != 0)
            throw new InvalidDataException($"The native snapshot holds deceleration multiplier {memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier]}, which no retail level sets.");
        samus.HorizontalSpeed.EchoSoundPlaying = W(NativeSnapshotMemory.SpeedEchoSoundLatch) != 0;
        PrivateState.Set(samus.HorizontalSpeed, nameof(SamusHorizontalSpeedState.TotalSpeed), new UnsignedSixteenSixteen(W(NativeSnapshotMemory.TotalHorizontalSpeed), W(NativeSnapshotMemory.TotalHorizontalSubspeed)));
        runtime.Projectiles.Pool.ShotCount.Restore(W(NativeSnapshotMemory.ProjectileCount));
        runtime.Projectiles.Charge.PreviousCharge = SamusSnapshotWords.DecodeBeamCharge(W(NativeSnapshotMemory.PreviousCharge));
        runtime.Projectiles.Firing.InteractionImmunity = UpdateCountdown.For(W(NativeSnapshotMemory.ProjectileInteractionImmunity));
        runtime.Projectiles.Charge.ChargedShotGlow = SamusSnapshotWords.DecodeChargedShotGlow(W(NativeSnapshotMemory.ChargedShotGlow));
        int chargePaletteByteOffset = W(NativeSnapshotMemory.ChargePaletteIndex);
        runtime.Projectiles.Charge.ChargeGlowStep = (chargePaletteByteOffset & 1) == 0
            ? chargePaletteByteOffset / sizeof(ushort)
            : throw new InvalidDataException($"Native charge palette byte offset {chargePaletteByteOffset} names no step.");
        global::SuperMetroid.Core.PrivateState.Set(runtime.BombProjectiles, "BombCounter", (int)(W(NativeSnapshotMemory.BombCount)));
        samus.BombSpreadCharge = UpdateCountdownAccess.AtAge(MorphBallBombDefinitions.DownChargeTimeoutUpdates, W(NativeSnapshotMemory.BombSpreadChargeTimeout));
        samus.TransitionShot.Hand(SamusSnapshotWords.DecodeTransitionShot(W(NativeSnapshotMemory.PoseShotDirection)));
        samus.HyperBeamEnabled = SamusSnapshotWords.DecodeHyperBeam(W(NativeSnapshotMemory.HyperBeam));
        if (SamusSnapshotWords.DecodeResumeChargingBeamSound(W(NativeSnapshotMemory.ResumeChargeSound))) samus.ChargingSound.Arm(); else samus.ChargingSound.Cancel();
        PrivateState.Set(samus.InputSnapshot, "PreviousDrawHeldInput", GameplayActionWordCodec.FromJoypadWord(W(NativeSnapshotMemory.SamusFilteredHeld), "native Samus filtered held input"));
        samus.InputSnapshot.PressesSeenLastUpdate = GameplayActionWordCodec.FromJoypadWord(W(NativeSnapshotMemory.SamusFilteredNew), "native Samus filtered new input");
        samus.InputSnapshot.AutoJumpTimer = W(NativeSnapshotMemory.SamusAutoJumpTimer);
        samus.PreviousHealthForHurtCheck = SamusCountWords.Decode(W(NativeSnapshotMemory.SamusPreviousHealthForFlash));
        VerificationAssertions.AssertTrue(W(NativeSnapshotMemory.SamusInputHandler) is
            NativeSnapshotMemory.SamusNormalInputHandler or NativeSnapshotMemory.SamusAutoJumpInputHandler,
            "initial movie input handler has a verified semantic mapping");
        if (W(NativeSnapshotMemory.SamusInputHandler) == NativeSnapshotMemory.SamusAutoJumpInputHandler) samus.InputSnapshot.AutoJump.Install(); else samus.InputSnapshot.AutoJump.Reinstate();
        samus.EquippedItems = SamusEquipmentWordCodec.FromEquipmentWord(W(NativeSnapshotMemory.Items));
        samus.EquippedBeams = SamusEquipmentWordCodec.FromBeamWord(W(NativeSnapshotMemory.Beams));
        samus.Health = SamusCountWords.Decode(W(NativeSnapshotMemory.Health));
        samus.MaxHealth = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxHealth));
        samus.InvincibilityTimer = WrappingCountdown.Start(W(NativeSnapshotMemory.InvincibilityTimer));
        samus.KnockbackHold = UpdateCountdown.For(W(NativeSnapshotMemory.KnockbackTimer));
        // The checkpoint's movement handler is the ordinary one, so a recorded direction is a knockback that no longer moves her.
        samus.Knockback = SamusMovementNativeWords.KnockbackDirections.Decode(W(NativeSnapshotMemory.KnockbackDirection), "Samus knockback direction") is
            var recordedKnockback and not SamusKnockbackDirection.None
            ? SamusKnockback.Moving(recordedKnockback).Released()
            : SamusKnockback.None;
        samus.KnockbackXDirection = (KnockbackHorizontalDirection)W(NativeSnapshotMemory.KnockbackXDirection);
        samus.HurtFlash.HoldAt(W(NativeSnapshotMemory.HurtFlashCounter));
        PrivateState.Set(samus, "EnergyFraction", SuperMetroid.Core.Compatibility.PeriodicDamageEnergyFraction.Holding(W(NativeSnapshotMemory.SubunitHealth)));
        samus.SelectedHudItem = SaveSlotNumbering.SavedHudItem(W(NativeSnapshotMemory.SelectedHudItem));
        PrivateState.Set(samus.ArmCannon, nameof(SamusArmCannonState.PreviousSelectedHudItem), samus.SelectedHudItem);
        // The cartridge records no auto-cancel as the beams' index.
        samus.AutoCancelledItem = SaveSlotNumbering.SavedHudItem(W(NativeSnapshotMemory.AutoCancelHudItemIndex)) is var autoCancelled &&
            autoCancelled != HudItem.Beam ? autoCancelled : null;
        samus.ReserveTankMode = SaveSlotNumbering.SavedReserveMode(W(NativeSnapshotMemory.ReserveMode));
        samus.MaxReserveEnergy = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxReserve));
        samus.ReserveEnergy = SamusCountWords.Decode(W(NativeSnapshotMemory.Reserve));
        samus.Pose = SamusPoseNumbering.FromNumber(W(NativeSnapshotMemory.Pose));
        samus.Position = samus.Position.WithWhole(new(W(NativeSnapshotMemory.X), W(NativeSnapshotMemory.Y)));
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(W(NativeSnapshotMemory.XFraction)).WithFractionY(W(NativeSnapshotMemory.YFraction));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        SamusPresentation.SetAnimationFrameFromMovementOwner(samus, samus.AnimationFrameNumbered(W(NativeSnapshotMemory.Animation)), W(NativeSnapshotMemory.AnimationTimer));
        samus.PoseHistory.Restore(SamusPoseNumbering.FromNumber(W(NativeSnapshotMemory.PreviousPose)), SamusSnapshotWords.DecodeHistoryMovement(W(NativeSnapshotMemory.PreviousDirection)), SamusSnapshotWords.DecodeHistoryMovement(W(NativeSnapshotMemory.LastDifferentDirection)));
        samus.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.BaseSpeed), W(NativeSnapshotMemory.BaseFraction));
        samus.HorizontalSpeed.ExtraRunSpeed = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.ExtraSpeed), W(NativeSnapshotMemory.ExtraFraction));
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationModeImport.FromNative(W(NativeSnapshotMemory.AccelerationMode));
        samus.HorizontalSpeed.HasRunningMomentum = W(NativeSnapshotMemory.Momentum) != 0;
        samus.HorizontalSpeed.SpeedBoostCounter = SamusSnapshotWords.DecodeSpeedBoostCounter(W(NativeSnapshotMemory.BoostCounter));
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocityConstants.Of(new UnsignedSixteenSixteen(W(NativeSnapshotMemory.VerticalSpeed), W(NativeSnapshotMemory.VerticalFraction)), SamusVerticalDirectionCodec.FromNative(W(NativeSnapshotMemory.VerticalDirection)));
        samus.Kinematics.Radius = new(W(NativeSnapshotMemory.SamusXRadius), W(NativeSnapshotMemory.SamusYRadius));
        samus.Kinematics.YAcceleration = new UnsignedSixteenSixteen(W(NativeSnapshotMemory.Gravity), W(NativeSnapshotMemory.GravityFraction));
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.FromStoredParts(W(NativeSnapshotMemory.ExtraXDisplacement), W(NativeSnapshotMemory.ExtraXDisplacementFraction), W(NativeSnapshotMemory.ExtraYDisplacement), W(NativeSnapshotMemory.ExtraYDisplacementFraction));
        if (!SamusSnapshotWords.AlignsToSlopes(W(NativeSnapshotMemory.SlopeCollisionEnable)))
            throw new InvalidDataException(
                $"Native slope-alignment word ${W(NativeSnapshotMemory.SlopeCollisionEnable):X4} disables the alignment, which no retail state does.");
        samus.HorizontalSpeed.SlowdownLevel = W(NativeSnapshotMemory.SpeedDivisor);
        samus.HorizontalSpeed.BodyAttack = SamusMovementNativeWords.ContactDamage.Decode(
            W(NativeSnapshotMemory.ContactDamageIndex), "Samus body attack");
        samus.MorphBallBounceState = SamusSnapshotWords.DecodeBallBounce(W(NativeSnapshotMemory.MorphBallBounceState));
        samus.BombJump = SuperMetroid.TestSupport.VerificationImporters.SamusBombJumpImport.FromNative(W(NativeSnapshotMemory.BombJumpDirection));

        // The checkpoint's live native trail places become trails, which show the pictures of the beams equipped there, as
        // its projectile graphics hold them (#1586); a place whose sparkles have both ended holds no trail.
        runtime.Projectiles.Trails.Clear();
        for (int offset = 0; offset < NativeSnapshotMemory.TrailRightFrameHold - NativeSnapshotMemory.TrailLeftFrameHold; offset += 2)
        {
            int leftHold = W(NativeSnapshotMemory.TrailLeftFrameHold + offset);
            int rightHold = W(NativeSnapshotMemory.TrailRightFrameHold + offset);
            if (leftHold == 0 && rightHold == 0)
                continue;
            // Native keeps each sparkle's own room place and not the shot's, so the trail stands from the room's origin and
            // each sparkle's offset is its place, which lies in the room or just above or left of it, read as signed.
            var trail = runtime.Projectiles.AddTrail(Position16<RoomPixels>.Zero, SamusBeamCombinations.Of(samus.EquippedBeams));
            // A sparkle whose timer has run out has ended: its animation is over, whatever its pointer still names.
            trail.Left.RestoreNative(W(NativeSnapshotMemory.TrailLeftInstructionPointer + offset), leftHold,
                ProjectileTrailNative.ShownPose(W(NativeSnapshotMemory.TrailLeftTileNumberAttributes + offset)));
            trail.Right.RestoreNative(W(NativeSnapshotMemory.TrailRightInstructionPointer + offset), rightHold,
                ProjectileTrailNative.ShownPose(W(NativeSnapshotMemory.TrailRightTileNumberAttributes + offset)));
            trail.Left.Offset = new Offset16<RoomPixels>(CoordinateAxis.AsSigned(W(NativeSnapshotMemory.TrailLeftXPosition + offset)),
                CoordinateAxis.AsSigned(W(NativeSnapshotMemory.TrailLeftYPosition + offset)));
            trail.Right.Offset = new Offset16<RoomPixels>(CoordinateAxis.AsSigned(W(NativeSnapshotMemory.TrailRightXPosition + offset)),
                CoordinateAxis.AsSigned(W(NativeSnapshotMemory.TrailRightYPosition + offset)));
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
        samus.ExtraAnimationDelay = W(NativeSnapshotMemory.AnimationFrameBuffer);
        PrivateState.Set(samus.LiquidPhysics, nameof(SamusLiquidPhysicsState.MediumLastEntered), SamusLiquidMediumCodec.FromNative(W(NativeSnapshotMemory.LiquidPhysicsType)));
        PrivateState.Set(samus.PeriodicDamage, "Pending", SuperMetroid.Core.Compatibility.EnergyAmount.FromStoredParts(W(NativeSnapshotMemory.PeriodicDamage), W(NativeSnapshotMemory.PeriodicSubDamage)));

        // The snapshot was recorded after the entering door PLM deleted itself.
        // Restore the empty physical pool rather than executing fresh room-entry actors.
        var initialObjects = runtime.RoomObjects.Places.InUpdateOrder;
        for (int index = 0; index < initialObjects.Count; index++)
        {
            VerificationAssertions.AssertTrue(W(NativeSnapshotMemory.RoomObjectHeaders + index * 2) == 0, "native initial PLM pool is empty");
            initialObjects[index].Delete();
        }
        // Every place takes the snapshot's projectile, or, for a free one, what it holds for the room's next spawn there.
        for (int place = 0; place < SuperMetroid.Core.Compatibility.EnemyProjectilePlaces.Capacity; place++)
            ImportNativeEnemyProjectile(runtime.Enemies, place, runtime.Enemies.GraphicsSet, bus, W);
        // This is a one-time initial snapshot import. No native state is fed back during replay.
        runtime.System.SetRandomNumber(W(NativeSnapshotMemory.Random));
        runtime.Enemies.FrameContext.SeedEnemyPassCount(W(NativeSnapshotMemory.MainEnemyRoutineCount));
        // The liquid's BG3 HDMA object has already run its first pass, so its callback is
        // installed. A live object owns the per-frame liquid motion; once Mother Brain's
        // $A9:8C0C has cleared the channel flags, the room's liquid objects are deleted.
        bool lavaAcidObjectLive = Enumerable.Range(0, NativeSnapshotMemory.HdmaObjectCount).Any(slot =>
            W(NativeSnapshotMemory.HdmaObjectChannelBitflags + slot * 2) != 0 &&
            W(NativeSnapshotMemory.HdmaObjectFrameBehaviors + slot * 2) == NativeSnapshotMemory.AcidHdmaCallback);
        VerificationAssertions.AssertTrue(runtime.RoomFxOverlay.Type is RoomFxType.Lava or RoomFxType.Acid,
            "snapshot import models the lava/acid HDMA objects of its room");
        runtime.RoomFxOverlay.ImportLiquidMotion(
            stopped: !lavaAcidObjectLive,
            tidePhase: RotationAngleConstants.FromRaw(W(NativeSnapshotMemory.TidePhase)),
            tideOffset: SixteenSixteenWords.ToDisplacement(W(NativeSnapshotMemory.TideOffset), W(NativeSnapshotMemory.TideOffsetFraction)),
            baseY: (runtime.RoomFxOverlay.BaseY
                ?? throw new InvalidDataException("The imported acid room's effect names no surface.")).WithFraction(new PixelFraction(W(NativeSnapshotMemory.LiquidBaseFraction))),
            // The surface word is a signed line: one a tide carries above the room's top is negative.
            surface: CoordinateAxis.AsSigned(W(NativeSnapshotMemory.AcidSurface)),
            camera: runtime.Room.LoadedCamera.PixelPosition);
        samus.LiquidPhysics.ConfigureFor(runtime.RoomFxOverlay);
        var pipeBugPointers = new PipeBugNativeInstructionPointers(bus);
        var kraidHeadCursors = new KraidHeadNativeCursors(bus);
        // A Shaktool chain's segments are set up again in a chain the import holds, whose segments' states their records
        // fill (#2554); the chain reaches its recorded target through its wall, whose reversal the segments' records then
        // replace. Its leading saw is the first enemy.
        int firstEnemy = NativeSnapshotMemory.EnemyBase + SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(0);
        ShaktoolChain? shaktool = null;
        if (W(firstEnemy) != 0 && runtime.Enemies.Places.InUpdateOrder[0].Holds(EnemyDefinitionId.Shaktool))
        {
            shaktool = runtime.Enemies.HoldShaktoolChain();
            NativeEnemyFamilyStateImport.ImportShaktoolTarget(runtime.Enemies, W(firstEnemy + 48));
        }
        // EnemyData +$0E onward; +$00 is the identity, +$02..+$08 the 16.16 position and
        // +$0A/+$0C the radii, each imported as one typed value below.
        for (int index = 0; index < runtime.Enemies.Places.InUpdateOrder.Count; index++)
        {
            var enemy = runtime.Enemies.Places.InUpdateOrder[index];
            int address = NativeSnapshotMemory.EnemyBase + SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(index);
            if (W(address) != 0)
                VerificationAssertions.AssertTrue(enemy.Occupant is { } loaded && SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(loaded.DefinitionId) == W(address), "initial enemy species agrees with room population");
            // A recorded occupant is the one the room load placed, which keeps its species state; a slot the recording
            // holds free loses its loaded occupant with that occupant's state (#1543).
            if (W(address) == 0)
            {
                if (enemy.Occupant is not null)
                    enemy.Vacate();
            }
            else
            {
                EnemyDefinitionId recordedOccupant = SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIdsConstants.FromHeaderPointer(W(address));
                if (!enemy.Holds(recordedOccupant))
                    enemy.Bind(recordedOccupant, enemy.KeptDefinition);
            }
            enemy.Position = FixedPosition<RoomPixels>.FromStoredParts(W(address + 2), W(address + 4), W(address + 6), W(address + 8));
            enemy.Radius = new(W(address + 10), W(address + 12));
            // The species bits of the property words are the entry's spawn settings, which the population already holds.
            SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.EngineStateOf(W(address + 14), W(address + 16)).ApplyTo(enemy);
            enemy.AiStates = SuperMetroid.AssetExtraction.Enemies.EnemyAiStatesWordCodec.Decode(W(address + 18));
            enemy.Health = W(address + 20);
            // A free native slot keeps its last occupant's word in that occupant's bank; a free port slot shows no frame.
            enemy.SpritemapPointer = W(address) == 0 ? (ushort)0 : W(address + 22);
            // The timer word at +$18 is the loop counter of the occupant's animation; the imported playbacks play no repeated
            // phase, so it counts nothing the port keeps (#2006).
            // A native timer at none is a list that has just slept or shown its frame for good: the port's clock stops.
            if (W(address + 28) == 0)
                enemy.AnimationClock.Stop();
            else
                enemy.AnimationClock.HoldFor(W(address + 28));
            // The native palette word names a row; the colouring it denotes is decoded under the room's loaded set.
            enemy.SetPaletteRow(runtime.Enemies, NativeSpriteGraphicsWords.PaletteRow(W(address + 30)));
            // The native tile index word is where the import composes the occupant's sheet, not slot state (#1513).
            enemy.HurtFlash.Restart(W(address + 36));
            enemy.Freeze.Restart(W(address + 38));
            enemy.Invincibility.Restart(W(address + 40));
            enemy.Shake.Restart(W(address + 42));
            enemy.FrameCounter = W(address + 44);
            var words = new NativeEnemyRecordWords(
                W(address + 48), W(address + 50), W(address + 52), W(address + 54), W(address + 56), W(address + 58));
            // A free native slot's family words are its last occupant's, which no family state of this slot holds (#1435).
            // Draygon's recorded phase is the step that runs it (#2554): a swoop's flies the path whose length is word C,
            // which is zero before he plans his first swoop.
            if (W(address) != 0 && words.A != 0 && enemy.SpeciesState<SuperMetroid.Core.Bosses.Draygon.DraygonEnemyState>() is { } draygon)
            {
                (SuperMetroid.Core.Bosses.Draygon.DraygonPhase phase, HorizontalDirection? pass) =
                    SuperMetroid.Core.Bosses.Draygon.DraygonNativeWords.Phases.Decode(words.A, "recorded DraygonPhase");
                draygon.Step = SuperMetroid.Core.Bosses.Draygon.DraygonStepImport.StepOf(phase, pass ?? draygon.PassDirection,
                    words.C != 0 ? SuperMetroid.Core.Bosses.Draygon.DraygonNativeWords.SwoopPathAt(W, words.C) : null);
            }
            if (W(address) != 0)
            {
                NativeEnemyFamilyStateImport.Import(runtime.Enemies, enemy, words);
                if (enemy.Holds(EnemyDefinitionId.Shaktool))
                    NativeEnemyFamilyStateImport.ImportShaktoolSegment(shaktool
                        ?? throw new InvalidDataException($"Recorded Shaktool segment {index} is not in the chain the first enemy leads."), enemy, words);
            }
            // Kraid's head program is typed and kept in the encounter state; the recorded body word is its native cursor,
            // which only this import translates. Before the first attack the word names no head record and the head stays unstarted.
            if (enemy.SpeciesState<SuperMetroid.Core.Bosses.Kraid.KraidPartState>() is SuperMetroid.Core.Bosses.Kraid.KraidBodyState kraidBody)
                kraidBody.Kraid.HeadPlayback = kraidHeadCursors.TryPlaybackAt(words.B);
            // A Pipe Bug's animation runs as a typed program (#641): resume it where the recorded pointer stands.
            if (enemy.PipeBugPlayback is not null)
                enemy.PipeBugPlayback = pipeBugPointers.PlaybackAt(W(address + MovieDesyncMemory.EnemyAnimationScriptOffset));
            if (runtime.Enemies.BrinstarPipeBugStates[index] is { } pipe)
            {
                pipe.SpawnPosition = new(words.B, words.C);
                pipe.RespawnDelay = UpdateCountdownAccess.Recorded(unchecked((short)words.D));
                SuperMetroid.Core.Enemies.PipeBugs.BrinstarPipeBugKind kind =
                    runtime.Enemies.Places.InUpdateOrder[index].HeldOccupant().SpawnSettings<SuperMetroid.Core.Enemies.PipeBugs.BrinstarPipeBugSpawnSettings>().Kind;
                pipe.EmergenceTopY = W(NativeSnapshotMemory.EnemyExtra + index * NativeSnapshotMemory.EnemyExtraBytes);
                // The game keeps the requested animation beside the installed one and installs a request in the same AI
                // update, so a record whose two differ fails (#1810). Core keeps only the playing animation (#2145): the
                // recorded pointer resumes in the installed animation, which the bug's next request for it finds playing.
                ushort installed = W(NativeSnapshotMemory.EnemyExtraPreviousAnimation + index * NativeSnapshotMemory.EnemyExtraBytes);
                if (words.E != installed)
                    throw new InvalidDataException($"Recorded Pipe Bug {index} requests animation {words.E} while {installed} is installed.");
                pipe.Playback = pipeBugPointers.PlaybackAt(W(address + MovieDesyncMemory.EnemyAnimationScriptOffset),
                    PipeBugsNativeWords.BrinstarAnimations(kind).Decode(installed, "recorded Pipe Bug installed animation"));
            }
        }
        if (runtime.Enemies.BossEncounters.MotherBrain is not null)
            ImportNativeMotherBrain(runtime, bus, W);
        runtime.Clock.Import(memory[NativeSnapshotMemory.NmiCounterByte], W(NativeSnapshotMemory.NmiCounter));
        samus.CollectedItems = SamusEquipmentWordCodec.FromEquipmentWord(W(NativeSnapshotMemory.CollectedItems)); samus.CollectedBeams = SamusEquipmentWordCodec.FromBeamWord(W(NativeSnapshotMemory.CollectedBeams));
        samus.Missiles = SamusCountWords.Decode(W(NativeSnapshotMemory.Missiles)); samus.MaxMissiles = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxMissiles));
        samus.SuperMissiles = SamusCountWords.Decode(W(NativeSnapshotMemory.SuperMissiles)); samus.MaxSuperMissiles = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxSuperMissiles));
        samus.PowerBombs = SamusCountWords.Decode(W(NativeSnapshotMemory.PowerBombs)); samus.MaxPowerBombs = SamusCountWords.Decode(W(NativeSnapshotMemory.MaxPowerBombs));
        // The HUD imports as the values its native rows show, once Samus's inventory has imported (#1466).
        runtime.Hud.ImportNative(hudWords, samus, highlightedHudItem, (runtime.Artwork.ScreenContent ?? throw new InvalidDataException(
            "The native checkpoint requires the installed HUD artwork.")).GameplayHud);
        samus.PreviousHealthForHurtCheck = samus.Health;
        runtime.PlayerActions.Latch(GameplayActionWordCodec.FromJoypadWord(W(NativeSnapshotMemory.HeldInput), "native held input"));
        game.AdoptRuntime(runtime);
        global::SuperMetroid.Core.PrivateState.Set(game.Session, "LastAudioRoomState", (global::SuperMetroid.Core.World.RoomStates.RoomStateSettings?)(runtime.Room.LoadedRoom.State));
        // The pause's stable menu runs over the menu the pause built, which is shown before the game enters it.
        bool pausedOnMenu = GameStateNumberCodec.NamesPauseMenu(W(NativeSnapshotMemory.GameState));
        if (pausedOnMenu)
            ShowNativePauseMenu(game, runtime, W, memory);
        GameStateNumberCodec.Enter(game, W(NativeSnapshotMemory.GameState));
        // The room load showed the colours of the Samus the Ceres start made; the imported equipment names the suit and
        // beams whose colours the cartridge loaded (#1512), under which her palette-FX records are then laid.
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(runtime.Player.Colours, samus.EquippedItems);
        SamusProjectileSystem.ShowBeamShade(runtime.Player.Colours, samus.EquippedBeams);
        ImportShownPaletteFxFrames(runtime);
        if (pausedOnMenu)
            ImportNativePauseMenuCadence(game, runtime, W);
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
    /// <summary>
    /// The arm cannon cover the recorded open and transition flags and cover picture stand for: a closed cover shows
    /// nothing, an open one its fully open picture, and an opening or a closing the picture it is at.
    /// </summary>
    private static SamusArmCannonCover ArmCannonCoverOf(bool openOrOpening, bool transitioning, SamusArmCannonCoverFrame? frame) =>
        (openOrOpening, transitioning, frame) switch
        {
            (false, false, null) => SamusArmCannonCover.Closed,
            // An open cover is where an opening's fully open picture moves on to.
            (true, false, SamusArmCannonCoverFrame.FullyOpen) => new SamusArmCannonCover.Opening(SamusArmCannonCoverFrame.FullyOpen).Advanced(),
            (true, true, { } opening) => new SamusArmCannonCover.Opening(opening),
            (false, true, { } closing) => new SamusArmCannonCover.Closing(closing),
            _ => throw new InvalidDataException(
                $"The recorded arm cannon cover (open {openOrOpening}, moving {transitioning}, picture {frame}) is no cover state."),
        };

    private static void ImportShownPaletteFxFrames(SuperMetroidRuntime runtime)
    {
        // Native slot order is the reverse of the order the effects take their turns in.
        foreach (RoomColourEffect effect in Enumerable.Reverse(runtime.RoomPaletteFx.effects))
        {
            ColourEffectColours colours = effect.Colours;
            if (LastShownFrame(effect.FramesOf, effect.Animation, effect.Place) is not { } frame)
                continue;
            PrivateState.Set(effect, nameof(RoomColourEffect.Frame), frame);
            // The frame goes to the owner of the effect's target, as the effect's turn hands it over (#2544).
            var shown = new ColourEffectFramesOnOwners();
            if (effect.Target is ColourEffectTarget.OnOwner owner)
                owner.Show(new ShownColourEffectFrame(colours, frame), shown);
            foreach (ShownColourEffectFrame suit in shown.Suit)
                runtime.Player.Colours.LayOverSuit(ColourEffectShades.Of(suit.Colours, suit.Frame));
            foreach (ColourEffectFrame flash in shown.ShutterFlash)
                runtime.Enemies.ShowEscapeShutterFlash(flash);
            foreach (ShownEnemyGlow glow in shown.Glows)
                glow.Enemy.ShowGlow(glow.Frame);
        }
    }

    /// <summary>
    /// The frame a colour effect showed last before <paramref name="place"/> in <paramref name="animation"/>, in the frames
    /// of <paramref name="framesOf"/>: the nearest frame before the step it resumes at, among that effect's steps. The native
    /// instruction pointer rests after the record it last showed (a loop's or a jump's goto runs only when the object next
    /// advances), so the steps before the place are the ones the object played, a list that runs on into the next one's last
    /// record among them. A place with no frame before it has shown no frame yet: null.
    /// </summary>
    private static ColourEffectFrame? LastShownFrame(
        ColourEffectId framesOf, SuperMetroid.Core.Animation.Animation<ColourEffectSegment> animation, SuperMetroid.Core.Animation.AnimationPlace place)
    {
        AnimationPhase<ColourEffectId> phase = PaletteFxNativeProgramOracle.InEffectSteps(framesOf, animation)[place.Phase];
        AnimationBeat<int, ColourEffectCue>[] steps = [.. ColourEffectShownFrames.Steps(framesOf)];
        for (int step = Math.Min(phase.FirstBeat + place.Beat, steps.Length) - 1; step >= 0; step--)
        {
            if (steps[step] is AnimationBeat<int, ColourEffectCue>.Frame shown)
                return new ColourEffectFrame(framesOf, shown.Shown);
        }
        return null;
    }

    /// <summary>
    /// Enemy-projectile kinds whose complete state is the native slot arrays and the Mother Brain they belong to: no
    /// family keeps port-only words (afterburn chains, death-drop sources) that the arrays cannot supply.
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
    private static void ImportNativeEnemyProjectile(SuperMetroid.Core.World.RoomEnemySystem enemies, int place, SuperMetroid.AssetExtraction.Enemies.RoomEnemyGraphicsSetDefinition graphicsSet, IImportCartridgeSource cartridge,
        Func<int, ushort> W)
    {
        int index = place * 2;
        ushort id = W(NativeSnapshotMemory.EnemyProjectileId + index);
        var leftPosition = new Position16<RoomPixels>(W(NativeSnapshotMemory.EnemyProjectileX + index), W(NativeSnapshotMemory.EnemyProjectileY + index));
        var leftVelocity = new PixelVelocity<RoomPixels>(
            new SignedEightEight(W(NativeSnapshotMemory.EnemyProjectileXVelocity + index)),
            new SignedEightEight(W(NativeSnapshotMemory.EnemyProjectileYVelocity + index)));
        if (id == 0)
        {
            // Room loading spawns the room's projectiles afresh; a slot the snapshot holds empty (for example Mother
            // Brain's destroyed turrets) holds no projectile here either, only what its last one left for the next spawn.
            enemies.Projectiles.ImportLeftover(place, new SuperMetroid.Core.Compatibility.EnemyProjectileHandOver(leftPosition, leftVelocity));
        }
        else
        {
            RoomEnemyProjectileKind kind = EnemyProjectileNativeWords.Kinds.Decode(id, "enemy projectile definition");
            // The room's load spawned the place's projectile, which carries its spawner's state; a place it left free is occupied afresh.
            EnemyProjectile projectile = enemies.Projectiles.LatestInPlace(place) ?? enemies.OccupyProjectile(place, kind);
            VerificationAssertions.AssertTrue(NativeSlotOnlyEnemyProjectileKinds.Contains(kind),
                $"initial native enemy projectile {projectile.Place} kind ${id:X4} has a verified slot-only import");
            // The snapshot's graphics word packs the projectile's tile base and palette row as an attribute word does: the
            // tile base names the owner whose sheet the projectile draws from (#1513), and the row is decoded into the
            // shading it denotes (#1512): a projectile at its spawner's characters draws in its spawner's row, its standard
            // colours, and one at character zero in the shared selection its row is.
            var graphics = new SnesObjAttributeWord(W(NativeSnapshotMemory.EnemyProjectileGraphics + index));
            int standardRow = graphics.TileNumber == 0 ? SuperMetroid.AssetExtraction.Enemies.EnemySheetRowDefinitions.Common : graphics.PaletteIndex;
            FamilyProjectileStarts.Initialize(enemies, projectile, kind, new SpriteGraphics(graphicsSet.OwnerDrawingAt(graphics.TileNumber),
                SpriteColouringAccess.ShadingInRow(graphics.PaletteIndex, standardRow)));
            // Each slot-only kind belongs to the room's Mother Brain, which her spawn gives it in her behaviour's table
            // (#2571): her head pins the charge and the small breath's end clears her flag. The place's previous projectile
            // leaves no entry the import may borrow, so the projectile is given its own as her spawn gives it.
            SuperMetroid.Core.Bosses.MotherBrain.MotherBrainEnemyState motherBrain = enemies.BossEncounters.MotherBrain ?? throw new InvalidDataException(
                $"Initial native enemy projectile {projectile.Place} kind {kind} is Mother Brain's, but the room has no Mother Brain encounter.");
            enemies.MotherBrainBehavior.Projectiles.Projectile.MotherBrains.Start(
                projectile, new SuperMetroid.Core.Bosses.MotherBrain.MotherBrainProjectileState(motherBrain));
            projectile.StartMotionFromWord(W(NativeSnapshotMemory.EnemyProjectileFrameBehavior + index));
            ushort instruction = W(NativeSnapshotMemory.NativeEnemyProjectileInstruction + index);
            // The typed family resumes at the step the recorded native pointer stands for.
            EnemyProjectileAnimation typed = projectile.Animation ?? throw new InvalidOperationException(
                $"Initial enemy projectile {projectile.Place} kind {kind} starts no typed program.");
            // The slot-only kinds keep nothing but their program's loop count in the native timer word: the plays its
            // repeated phase has left (#2006).
            ushort loopCounter = W(NativeSnapshotMemory.EnemyProjectileTimer + index);
            VerificationAssertions.AssertEqual(1, NativeEnemyProjectileFamilies.All.Count(family => family.TryResumeAt(cartridge, typed, instruction, loopCounter)),
                $"initial enemy projectile {projectile.Place} resumes its typed program");
            projectile.FrameHold = WrappingCountdown.Start(W(NativeSnapshotMemory.EnemyProjectileFrameHold + index));
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
            projectile.Position = projectile.Position.WithWhole(leftPosition);
            projectile.Velocity = leftVelocity;
        }
    }
}

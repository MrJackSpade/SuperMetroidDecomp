using SuperMetroid.TestSupport.Assertions;
using static SuperMetroid.TestSupport.MovieReplay.RidleySamusDrawReference;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using System.Security.Cryptography;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.TestSupport.SamusNativeCodecs;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// Replays the complete supplied Ridley showcase movie (#1266) through production gameplay and compares every accepted
/// update's state with its native checkpoint: the movie's one initial-state import is its snapshot, and every later
/// update receives only its converted controller word. Each update is compared before the host runs the next.
/// </summary>
internal sealed class RidleyShowcaseReplay : IConvertedMovieReplay
{
    /// <summary>The committed compact trace of the movie.</summary>
    private readonly string directory;

    /// <summary>The final report, once every update matched.</summary>
    private string? report;

    /// <summary>Prepares a replay of the movie against the compact trace in <paramref name="directory"/>.</summary>
    internal RidleyShowcaseReplay(string directory) => this.directory = directory;

    /// <inheritdoc/>
    public SuperMetroidGame Game
    {
        get => field ?? throw new InvalidOperationException("The Ridley replay has not imported its snapshot.");
        private set;
    }

    /// <inheritdoc/>
    public IEnumerable<ControllerButton?> Frames()
    {
        var ridleyMovie = ReplayMovieFixture.Ridley.Load();
        byte[] movie = ridleyMovie.Bytes;
        using var replayTrace = ReplayTrace.OpenCompact(directory, ReplayTraceMemoryCatalog.RidleyShowcase);
        var root = replayTrace.Updates;
        // v5-v7 only add message-box and boot-entering dispatch evidence; this snapshot movie
        // never reboots, which the integer expectedRecord read below confirms.
        VerificationAssertions.AssertTrue(root.GetProperty("format").GetString() is "super-metroid-gameplay-updates-v7", "converted replay format");
        VerificationAssertions.AssertEqual(0, root.GetProperty("initialRecord").GetInt32(), "snapshot movie has no folded boot prelude");
        VerificationAssertions.AssertEqual(Convert.ToHexString(SHA256.HashData(movie)), root.GetProperty("movieSha256").GetString(), "converted movie identity");
        VerificationAssertions.AssertEqual(10890, root.GetProperty("sourceFrameCount").GetInt32(), "complete original movie coverage");
        var updates = root.GetProperty("updates").EnumerateArray().ToArray();
        int length = root.GetProperty("updateCount").GetInt32();
        VerificationAssertions.AssertEqual(length, updates.Length, "converted update count");
        // The compact trace checks its stream against its compact manifest; the manifest's
        // source capture must be the one the converted manifest names.
        VerificationAssertions.AssertEqual(replayTrace.SourceCheckpointsSha256, root.GetProperty("checkpointsSha256").GetString(), "native checkpoint identity");
        int lastNativeRecord = -1;
        NativeCheckpointMemory ReadFrame(int update)
        {
            int wantedRecord = update == 0 ? 0 : updates[update - 1].GetProperty("expectedRecord").GetInt32();
            VerificationAssertions.AssertTrue(wantedRecord > lastNativeRecord, "native checkpoint order remains forward-only");
            NativeCheckpoint record = replayTrace.Records.Read(wantedRecord);
            lastNativeRecord = wantedRecord;
            int expectedFrame = update < length ? updates[update].GetProperty("sourceFrame").GetInt32() : 10890;
            VerificationAssertions.AssertEqual(expectedFrame, record.SourceFrame, "native input-boundary frame");
            VerificationAssertions.AssertEqual(update < length ? NativeSnapshotMemory.ReadControllerInput : 0,
                record.BoundaryPc, "native accepted-input boundary or terminal");
            return record.Memory;
        }
        NativeCheckpointMemory memory = ReadFrame(0);
        ushort W(int address) => memory.Word(address);
        var (bus, runtime, samus, game) = NativeSnapshotImport.ImportNativeSnapshot(memory.ToWorkRam(), ridleyMovie.InitialSaveRam);
        Game = game;
        // Ridley's body, fragments and fireballs run typed programs (#1287); compare their cursors as native pointers.
        var pointers = new RidleyNativeInstructionPointers(CartridgeImportSource.Require(bus));
        PlmSlot[] initialPlms = [.. runtime.Plms.SlotTable.Slots];
        var checkedProjectileCompositions = new HashSet<CheckedProjectileComposition>();
        ushort[]? pendingLoadedOwners = null;
        int loadingIntervals = 0;
        ushort[] CaptureLoadedOwners()
        {
            var words = new List<ushort> { checked((ushort)runtime.System.RandomNumber) };
            // The final scrolling IRQ publishes the visor before the deferred
            // loading owners reach their shared completion boundary. Check it there.
            RgbColor[] shown = GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources);
            foreach (int color in GameplayShownColours.Compared)
                if (color != XrayScopeDefinitionsPaletteConstants.VisorCgramIndex)
                    words.Add(shown[color].ToWord());
            foreach (var actor in runtime.Enemies.SlotTable.Slots)
                words.AddRange([SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(actor.DefinitionId), actor.Position.X.Whole, actor.Position.X.Fraction,
                    actor.Position.Y.Whole, actor.Position.Y.Fraction, checked((ushort)actor.Health), actor.SpritemapPointer,
                    actor.FrameHold.RawValue]);
            return words.ToArray();
        }
        int pairedBodyDraws = 0, phaseShiftedBodyDraws = 0, retainedHiddenBodyRecords = 0;
        // The body records the pose half and frame it drew; the cartridge's index for that picture is compared.
        int SpritemapIndex(SamusBodyHalfFrame? drawn) => SamusSpritemapNative.Index(samus.Presentation.BodyFrames.Artwork?.Spritemaps ??
            throw new InvalidOperationException("Verification requires installed Samus spritemaps."), drawn);
        BodyRecordResult BodyRecord() =>
            new BodyRecordResult(SpritemapIndex(samus.Presentation.DrawnFrame?.Top), SpritemapIndex(samus.Presentation.DrawnFrame?.Bottom),
                samus.Presentation.SpritemapPosition.X, samus.Presentation.SpritemapPosition.Y);
        var previousBodyRecord = BodyRecord();
        for (int frame = 0; frame <= length; frame++)
        {
            ushort previousKnockbackTimer = W(NativeSnapshotMemory.KnockbackTimer);
            ushort previousInvincibilityTimer = W(NativeSnapshotMemory.InvincibilityTimer);
            if (frame != 0) memory = ReadFrame(frame);
            var mismatches = new List<string>();
            // The cartridge parks an unaimed whip target at $FFFF.
            static ushort WhipTargetWord(int? target) => target is int angle ? checked((ushort)angle) : ushort.MaxValue;

            void Check(string name, int actual, int address) => CheckValue(name, actual, W(address));
            void CheckValue(string name, int actual, int expected)
            {
                if (actual != expected) mismatches.Add($"{name}: native={expected:X4} port={actual:X4}");
            }
            void CheckBytes(string name, int count, Func<int, byte> read, int address)
            {
                for (int index = 0; index < count; index++)
                {
                    byte actual = read(index);
                    byte expected = memory[address + index];
                    if (actual != expected)
                        mismatches.Add($"{name}[{index}]: native={expected:X2} port={actual:X2}");
                }
            }
            // Progress is compared as the native tables decode: each event, boss, item, door, station and map.
            var nativeProgress = new PersistentProgress();
            ProgressionTableCodec.ReadAreaBosses(memory.Slice(NativeSnapshotMemory.BossBits, ProgressionTableCodec.AreaBossByteCount), nativeProgress);
            ProgressionTableCodec.ReadFlags(memory.Slice(NativeSnapshotMemory.Events, ProgressionTableCodec.EventByteCount), nativeProgress.Events);
            ProgressionTableCodec.ReadFlags(memory.Slice(NativeSnapshotMemory.CollectedItemBits, ProgressionTableCodec.RoomFlagByteCount), nativeProgress.CollectedItems);
            ProgressionTableCodec.ReadFlags(memory.Slice(NativeSnapshotMemory.OpenedDoors, ProgressionTableCodec.RoomFlagByteCount), nativeProgress.OpenedDoors);
            ProgressionTableCodec.ReadUsedStations(memory.Slice(NativeSnapshotMemory.SaveElevatorMarkers, ProgressionTableCodec.UsedStationByteCount), nativeProgress);
            ProgressionTableCodec.ReadAreaMaps(memory.Slice(NativeSnapshotMemory.MapStationMarkers, ProgressionTableCodec.MapStationByteCount), nativeProgress);
            PersistentProgress portProgress = runtime.System.Progress;
            void CheckFlags(string name, NumberedFlagSet actual, NumberedFlagSet expected)
            {
                if (!actual.SetEquals(expected))
                    mismatches.Add($"{name}: native=[{string.Join(",", expected.Members)}] port=[{string.Join(",", actual.Members)}]");
            }
            CheckFlags("Events", portProgress.Events, nativeProgress.Events);
            CheckFlags("Collected items", portProgress.CollectedItems, nativeProgress.CollectedItems);
            CheckFlags("Opened doors", portProgress.OpenedDoors, nativeProgress.OpenedDoors);
            foreach (AreaId area in Enum.GetValues<AreaId>())
            {
                if (portProgress.DefeatedBosses(area) != nativeProgress.DefeatedBosses(area))
                    mismatches.Add($"{area} bosses: native={nativeProgress.DefeatedBosses(area)} port={portProgress.DefeatedBosses(area)}");
                CheckFlags($"{area} used stations", portProgress.UsedStations(area), nativeProgress.UsedStations(area));
                if (portProgress.HasAreaMap(area) != nativeProgress.HasAreaMap(area))
                    mismatches.Add($"{area} map: native={nativeProgress.HasAreaMap(area)} port={portProgress.HasAreaMap(area)}");
            }
            if (game.GameState is not (SuperMetroidGameState.HitDoorBlock or
                SuperMetroidGameState.LoadingNextRoomA or SuperMetroidGameState.LoadingNextRoomB))
                mismatches.AddRange(runtime.Hud.NativeRowDifferences(
                    [.. Enumerable.Range(0, HudStateConstants.MutableTileCount).Select(tile => W(NativeSnapshotMemory.HudTilemap + tile * 2))]));
            Check("Previous HUD selection", runtime.Hud.PreviousSelectedItem.ToWord(), NativeSnapshotMemory.PreviousHudSelection);
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                // $82:8B44 draws before $A0:9169 ages hurt timers. A final one
                // therefore still affects this update's draw although the checkpoint
                // stores zero. This movie has no debug invincibility/timer-reset path.
                bool knockbackAtDraw = W(NativeSnapshotMemory.KnockbackTimer) != 0 ||
                    (frame != 0 && previousKnockbackTimer == 1);
                bool invincibleAtDraw = W(NativeSnapshotMemory.InvincibilityTimer) != 0 ||
                    (frame != 0 && previousInvincibilityTimer == 1);
                bool forcedBodyVisible = knockbackAtDraw || !invincibleAtDraw ||
                    W(NativeSnapshotMemory.SamusShineTimer) != 0;
                bool nativeBodyVisible = forcedBodyVisible || (W(NativeSnapshotMemory.NmiCounter) & 1) == 0;
                bool normalizedBodyVisible = forcedBodyVisible || (runtime.Clock.Counter & 1) == 0;
                if (frame != 0)
                {
                    if (nativeBodyVisible && normalizedBodyVisible) pairedBodyDraws++;
                    if (nativeBodyVisible != normalizedBodyVisible) phaseShiftedBodyDraws++;
                    if (!normalizedBodyVisible)
                    {
                        VerificationAssertions.AssertEqual(previousBodyRecord, BodyRecord(),
                            $"update {frame}: hidden body retains prior sprite records");
                        retainedHiddenBodyRecords++;
                    }
                }
                if (frame != 0)
                {
                    var nativeDraw = RidleyNativeBodyRecord(bus, memory);
                    if (nativeBodyVisible)
                        VerificationAssertions.AssertEqual(new RidleyNativeBodyRecordResult(W(NativeSnapshotMemory.SamusTopSpritemap), W(NativeSnapshotMemory.SamusBottomSpritemap),
                                W(NativeSnapshotMemory.SamusSpriteX), W(NativeSnapshotMemory.SamusSpriteY)),
                            nativeDraw.Records, $"update {frame}: cartridge draw oracle matches native visible records");
                    if (normalizedBodyVisible)
                        VerificationAssertions.AssertEqual(new BodyRecordResult(nativeDraw.Records.Top, nativeDraw.Records.Bottom, nativeDraw.Records.X, nativeDraw.DrawnY), BodyRecord(),
                            $"update {frame}: visible body matches cartridge draw oracle regardless of flicker phase");
                }
                // Flicker deliberately follows the retained NMI clock. Removed hardware
                // waits can change its parity, and hidden frames retain the last visible
                // origin/indices. Compare newly published records when both draws execute.
                if (frame == 0 || (nativeBodyVisible && normalizedBodyVisible))
                {
                    Check("Samus top spritemap", SpritemapIndex(samus.Presentation.DrawnFrame?.Top), NativeSnapshotMemory.SamusTopSpritemap);
                    Check("Samus bottom spritemap", SpritemapIndex(samus.Presentation.DrawnFrame?.Bottom), NativeSnapshotMemory.SamusBottomSpritemap);
                    Check("Samus sprite X", samus.Presentation.SpritemapPosition.X, NativeSnapshotMemory.SamusSpriteX);
                    // A landing frame's native position word carries the table byte after its own, which the drawn origin
                    // never shows (#1500); the oracle gives the row the cartridge draws at.
                    ushort nativeSpriteY = frame == 0 ? W(NativeSnapshotMemory.SamusSpriteY) : RidleyNativeBodyRecord(bus, memory).DrawnY;
                    if (samus.Presentation.SpritemapPosition.Y != nativeSpriteY)
                        mismatches.Add($"Samus sprite Y: native={nativeSpriteY:X4} port={samus.Presentation.SpritemapPosition.Y:X4}");
                }
                if (frame != 0)
                {
                    var nativeCannon = RidleyNativeCannonDraw(bus, memory, invincibleAtDraw, W(NativeSnapshotMemory.NmiCounter));
                    if (nativeCannon.SpriteWritten)
                        VerificationAssertions.AssertTrue(ContainsMovieCannonSprite(memory.Slice(NativeSnapshotMemory.OamLow, 512),
                            memory.Slice(NativeSnapshotMemory.OamHigh, 32), nativeCannon),
                            $"update {frame}: native OAM contains reference cannon sprite");
                    // The normalized draw is observed through its outputs: the OAM image of the port's frame must
                    // hold the reference sprite (position and attributes), and the cover draw must show the frame
                    // composed over the cover tile the reference uploads.
                    var cannon = RidleyNativeCannonDraw(bus, memory, invincibleAtDraw, checked((ushort)runtime.Clock.Counter));
                    OamBuffer portImage = runtime.Sprites.FinalizedAsOam();
                    if (cannon.SpriteWritten)
                        VerificationAssertions.AssertTrue(ContainsMovieCannonSprite(portImage.LowTable, portImage.HighTable, cannon),
                            $"update {frame}: port frame contains reference cannon sprite");
                    SpriteDraw[] covers = [.. runtime.Sprites.Draws
                        .Where(draw => draw.Source.Kind == SpriteSourceKind.SamusArmCannon)];
                    VerificationAssertions.AssertEqual(cannon.SpriteWritten ? 1 : 0, covers.Length, $"update {frame}: cannon draw count");
                    if (cannon.SpriteWritten && cannon.TileUploadQueued)
                        VerificationAssertions.AssertEqual(ArmCannonTileSources.CoverTileDmaSources.IndexOf(cannon.TileSource),
                            SheetFrameSources.CharacterDraw((ImageSpriteDraw)covers[0]).Artwork.Id.Variant,
                            $"update {frame}: cannon draw binds the reference cover tile");
                }
                // The recorded cover flags are a byte each; the drawing mode is compared as the draw order it selects.
                ushort recordedCannonFlags = W(NativeSnapshotMemory.CannonFlags);
                if (samus.ArmCannon.IsOpen != (LittleEndianBytes.LowByte(recordedCannonFlags) != 0) ||
                    samus.ArmCannon.IsTransitioning != (LittleEndianBytes.HighByte(recordedCannonFlags) != 0))
                    mismatches.Add($"Cannon flags: native={recordedCannonFlags:X4} port open={samus.ArmCannon.IsOpen} transitioning={samus.ArmCannon.IsTransitioning}");
                Check("Cannon frame", samus.ArmCannon.Frame, NativeSnapshotMemory.CannonFrame);
                Check("Cannon toggle", samus.ArmCannon.StableSelectionUpdates, NativeSnapshotMemory.CannonToggle);
                SamusArmCannonDrawOrder recordedDrawOrder = SamusArmCannonState.DrawOrderOf(W(NativeSnapshotMemory.CannonDrawingMode));
                if (samus.ArmCannon.DrawOrder != recordedDrawOrder)
                    mismatches.Add($"Cannon draw order: native={recordedDrawOrder} port={samus.ArmCannon.DrawOrder}");
                Check("Minimap disabled", runtime.Hud.MinimapDisabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.MinimapDisabled);
                CheckBytes("Room scroll storage", RoomScrollGrid.StorageByteCount,
                    runtime.Room.LoadedCamera.Scrolls.ReadStorage, NativeSnapshotMemory.ScrollStorage);
                for (int area = 0; area < AreaIds.RetailCount; area++)
                {
                    int plane = area == W(NativeSnapshotMemory.CurrentArea) ? NativeSnapshotMemory.LiveExploredMap :
                        NativeSnapshotMemory.SavedExploredMaps + area * ProgressionTableCodec.ExploredPlaneByteCount;
                    ProgressionTableCodec.ReadExploredPlane(memory.Slice(plane, ProgressionTableCodec.ExploredPlaneByteCount),
                        AreaIds.At(area), nativeProgress);
                    if (!nativeProgress.ExploredCells(AreaIds.At(area)).SequenceEqual(portProgress.ExploredCells(AreaIds.At(area))))
                        mismatches.Add($"Explored map {AreaIds.At(area)} differs");
                }
            }
            // Only the reference's proven hardware-upload NMI count is normalized;
            // gameplay state is never copied back into the production runtime.
            int excludedNmis = frame == 0 ? 0 : updates[frame - 1].GetProperty("excludedNmiAfter").GetInt32();
            ushort normalizedNmi = unchecked((ushort)(W(NativeSnapshotMemory.NmiCounter) - excludedNmis));
            if (runtime.Clock.Counter != normalizedNmi)
                mismatches.Add($"Accepted gameplay NMI: native={normalizedNmi:X4} port={runtime.Clock.Counter:X4}");
            // All excluded accepted upload NMIs in this movie occur while the door
            // IRQ repeatedly clears this byte. Its post-door phase therefore compares
            // directly, unlike the independently retained word counter above.
            VerificationAssertions.AssertEqual(memory[NativeSnapshotMemory.NmiCounterByte], runtime.Clock.ShortCounter,
                $"update {frame}: byte NMI counter including native door IRQ reset");
            Check("Game state", GameStateNumberCodec.Encode(game.GameState), NativeSnapshotMemory.GameState);
            Check("Enemy door gate", runtime.Enemies.RoomStatus.EnemyDoorTransitionActive ? (ushort)1 : (ushort)0, NativeSnapshotMemory.EnemyDoorTransition);
            // Native LoadDoorHeader publishes the destination room pointer before
            // loading its room/state data. The port keeps that identity in the pending
            // door while ActiveRoom still owns the source room's loaded data.
            ushort selectedRoom = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.AlignSourceCamera or DoorTransitionPhase.FixDoorsMovingUp or
                DoorTransitionPhase.SetupNewRoom or DoorTransitionPhase.SetupScrolling or
                DoorTransitionPhase.PlaceSamusAndLoadTiles or DoorTransitionPhase.LoadMoreThingsAndOpenDoor
                ? RoomHeaderDefinitions.PointerOf((runtime.RoomLoader.PendingDoorTransition ?? throw new InvalidDataException("Missing selected destination door")).DestinationRoom)
                : runtime.Room.LoadedRoom.Pointer;
            Check("Selected room", selectedRoom, NativeSnapshotMemory.Room);
            Check("Camera X", runtime.Room.LoadedCamera.PixelPosition.X, NativeSnapshotMemory.CameraX);
            Check("Camera X fraction", runtime.Room.LoadedCamera.Position.X.Fraction, NativeSnapshotMemory.CameraXFraction);
            Check("Camera Y", runtime.Room.LoadedCamera.PixelPosition.Y, NativeSnapshotMemory.CameraY);
            Check("Camera Y fraction", runtime.Room.LoadedCamera.Position.Y.Fraction, NativeSnapshotMemory.CameraYFraction);
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                Check("Ideal camera X", runtime.Room.LoadedCamera.IdealPosition.X, NativeSnapshotMemory.IdealCameraX);
                Check("Ideal camera Y", runtime.Room.LoadedCamera.IdealPosition.Y, NativeSnapshotMemory.IdealCameraY);
                Check("Camera speed X", runtime.Room.LoadedCamera.Speed.X.WholeWord, NativeSnapshotMemory.CameraSpeedX);
                Check("Camera speed X fraction", runtime.Room.LoadedCamera.Speed.X.Fraction, NativeSnapshotMemory.CameraSpeedXFraction);
                Check("Camera speed Y", runtime.Room.LoadedCamera.Speed.Y.WholeWord, NativeSnapshotMemory.CameraSpeedY);
                Check("Camera speed Y fraction", runtime.Room.LoadedCamera.Speed.Y.Fraction, NativeSnapshotMemory.CameraSpeedYFraction);
                // A fresh door camera defers its first sample to the runtime's frame-start
                // fallback. Compare that effective sample instead of requiring storage.
                var previous = runtime.Room.LoadedCamera.PreviousSamusPoint ?? samus.Position;
                Check("Previous Samus X", previous.X.Whole, NativeSnapshotMemory.PreviousSamusX);
                Check("Previous Samus X fraction", previous.X.Fraction, NativeSnapshotMemory.PreviousSamusXFraction);
                Check("Previous Samus Y", previous.Y.Whole, NativeSnapshotMemory.PreviousSamusY);
                Check("Previous Samus Y fraction", previous.Y.Fraction, NativeSnapshotMemory.PreviousSamusYFraction);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                for (int index = 0; index < SamusAtmosphericEffectsState.SlotCount; index++)
                {
                    var effect = samus.LiquidPhysics.AtmosphericEffects.Slots[index];
                    // The recorded frame/type word is compared as the effect and frame the import codec decodes it to.
                    ushort recordedEffect = W(NativeSnapshotMemory.AtmosphericFrameAndType + index * 2);
                    if (effect.Type != SuperMetroid.AssetExtraction.Samus.SamusAtmosphericEffectWordCodec.DecodeType(recordedEffect) ||
                        effect.AnimationFrame != SuperMetroid.AssetExtraction.Samus.SamusAtmosphericEffectWordCodec.DecodeFrame(recordedEffect))
                        mismatches.Add($"Atmosphere {index} frame/type: native={recordedEffect:X4} port={effect.Type}/{effect.AnimationFrame}");
                    if (effect.IsEmpty) continue;
                    Check($"Atmosphere {index} timer", effect.AnimationTimer.RawValue, NativeSnapshotMemory.AtmosphericTimer + index * 2);
                    Check($"Atmosphere {index} X", effect.Position.X, NativeSnapshotMemory.AtmosphericX + index * 2);
                    Check($"Atmosphere {index} Y", effect.Position.Y, NativeSnapshotMemory.AtmosphericY + index * 2);
                }
                Check("Liquid animation buffer", checked((ushort)samus.AnimationFrameBuffer), NativeSnapshotMemory.AnimationFrameBuffer);
                Check("LiquidPhysicsType", (ushort)samus.LiquidPhysics.LiquidPhysicsType, NativeSnapshotMemory.LiquidPhysicsType);
                Check("PeriodicSubDamage", checked((ushort)samus.LiquidPhysics.PeriodicSubDamage), NativeSnapshotMemory.PeriodicSubDamage);
                Check("PeriodicDamage", checked((ushort)samus.LiquidPhysics.PeriodicDamage), NativeSnapshotMemory.PeriodicDamage);
                Check("Acid damage surface", checked((ushort)samus.LiquidPhysics.LavaAcidYPosition), NativeSnapshotMemory.AcidSurface);
                Check("Liquid tide phase", checked((ushort)runtime.RoomFxOverlay.TidePhase), NativeSnapshotMemory.TidePhase);
            }
            Check("Samus X", samus.Position.X.Whole, NativeSnapshotMemory.X);
            Check("Samus X fraction", samus.Kinematics.Position.X.Fraction, NativeSnapshotMemory.XFraction);
            Check("Samus Y", samus.Position.Y.Whole, NativeSnapshotMemory.Y);
            Check("Samus Y fraction", samus.Kinematics.Position.Y.Fraction, NativeSnapshotMemory.YFraction);
            Check("Samus pose", SamusPoseNumbering.Number(samus.Pose), NativeSnapshotMemory.Pose);
            Check("PreviousDrawHeldInput", (ushort)samus.InputSnapshot.PreviousDrawHeldInput, NativeSnapshotMemory.SamusFilteredHeld);
            Check("PreviousDrawNewInput", (ushort)samus.InputSnapshot.PreviousDrawNewInput, NativeSnapshotMemory.SamusFilteredNew);
            Check("AutoJumpTimer", samus.InputSnapshot.AutoJumpTimer, NativeSnapshotMemory.SamusAutoJumpTimer);
            Check("PreviousHealthForHurtCheck", SamusQuantity.Encode(samus.PreviousHealthForHurtCheck), NativeSnapshotMemory.SamusPreviousHealthForFlash);
            VerificationAssertions.AssertTrue(!samus.InputSnapshot.ShinesparkPoseInputLocked && !samus.InputSnapshot.CrystalFlashPoseInputLocked,
                "Movie now uses a special pose-input lock; map its handler explicitly");
            Check("Samus input handler", samus.InputSnapshot.AutoJumpInputPending
                ? NativeSnapshotMemory.SamusAutoJumpInputHandler : NativeSnapshotMemory.SamusNormalInputHandler,
                NativeSnapshotMemory.SamusInputHandler);

            Check("Samus previous pose", SamusPoseNumbering.Number(samus.PoseHistory.PreviousPose), NativeSnapshotMemory.PreviousPose);
            CheckValue("Samus previous movement", SamusMovementTypeNativeWords.Byte(samus.PoseHistory.PreviousMovement),
                SamusMovementTypeNativeWords.Byte(SamusSnapshotWords.DecodeHistoryMovement(W(NativeSnapshotMemory.PreviousDirection))));
            Check("Samus last different pose", SamusPoseNumbering.Number(samus.PoseHistory.LastDifferentPose), NativeSnapshotMemory.LastDifferentPose);
            CheckValue("Samus last different movement", SamusMovementTypeNativeWords.Byte(samus.PoseHistory.LastDifferentMovement),
                SamusMovementTypeNativeWords.Byte(SamusSnapshotWords.DecodeHistoryMovement(W(NativeSnapshotMemory.LastDifferentDirection))));
            Check("Samus animation", samus.AnimationFrame, NativeSnapshotMemory.Animation);
            Check("Samus animation timer", samus.AnimationFrameTimer.RawValue, NativeSnapshotMemory.AnimationTimer);
            Check("Samus base speed", samus.HorizontalSpeed.BaseSpeed.Whole, NativeSnapshotMemory.BaseSpeed);
            Check("Samus base fraction", samus.HorizontalSpeed.BaseSpeed.Fraction, NativeSnapshotMemory.BaseFraction);
            Check("Samus extra speed", samus.HorizontalSpeed.ExtraRunSpeed.Whole, NativeSnapshotMemory.ExtraSpeed);
            Check("Samus extra fraction", samus.HorizontalSpeed.ExtraRunSpeed.Fraction, NativeSnapshotMemory.ExtraFraction);
            Check("Samus vertical speed", samus.Kinematics.YSpeed.Whole, NativeSnapshotMemory.VerticalSpeed);
            Check("Samus vertical fraction", samus.Kinematics.YSpeed.Fraction, NativeSnapshotMemory.VerticalFraction);
            Check("Samus vertical direction", (ushort)samus.Kinematics.YDirection, NativeSnapshotMemory.VerticalDirection);
            Check("Samus SamusXRadius", samus.Kinematics.Radius.X, NativeSnapshotMemory.SamusXRadius);
            Check("Samus SamusYRadius", samus.Kinematics.Radius.Y, NativeSnapshotMemory.SamusYRadius);
            Check("Samus Gravity", samus.Kinematics.YAcceleration.Whole, NativeSnapshotMemory.Gravity);
            Check("Samus GravityFraction", samus.Kinematics.YAcceleration.Fraction, NativeSnapshotMemory.GravityFraction);
            Check("Samus ExtraXDisplacement", unchecked((ushort)samus.Kinematics.ExtraDisplacement.X.WholePixels), NativeSnapshotMemory.ExtraXDisplacement);
            Check("Samus ExtraXDisplacementFraction", samus.Kinematics.ExtraDisplacement.X.Fraction, NativeSnapshotMemory.ExtraXDisplacementFraction);
            Check("Samus ExtraYDisplacement", unchecked((ushort)samus.Kinematics.ExtraDisplacement.Y.WholePixels), NativeSnapshotMemory.ExtraYDisplacement);
            Check("Samus ExtraYDisplacementFraction", samus.Kinematics.ExtraDisplacement.Y.Fraction, NativeSnapshotMemory.ExtraYDisplacementFraction);
            Check("Samus SlopeCollisionEnable", samus.Kinematics.HorizontalSlopeCollisionEnable, NativeSnapshotMemory.SlopeCollisionEnable);
            Check("Samus SpeedDivisor", checked((ushort)samus.HorizontalSpeed.SpeedDivisor), NativeSnapshotMemory.SpeedDivisor);
            Check("Samus ContactDamageIndex", (ushort)samus.HorizontalSpeed.ContactDamageIndex, NativeSnapshotMemory.ContactDamageIndex);
            Check("Samus MorphBallBounceState", SamusSnapshotWords.EncodeBallBounce(samus.MorphBallBounceState), NativeSnapshotMemory.MorphBallBounceState);
            Check("Samus bomb jump", SamusBombJumpRequestCodec.ToNative(samus.BombJumpRequest), NativeSnapshotMemory.BombJumpDirection);
            Check("Samus running momentum", samus.HorizontalSpeed.HasRunningMomentum ? (ushort)1 : (ushort)0, NativeSnapshotMemory.Momentum);
            Check("Samus speed boost counter", SamusSnapshotWords.EncodeSpeedBoostCounter(samus.HorizontalSpeed.SpeedBoostCounter), NativeSnapshotMemory.BoostCounter);
            Check("Samus horizontal speed table", samus.HorizontalSpeed.ActiveSpeedTableBaseAddress, NativeSnapshotMemory.HorizontalSpeedTable);
            VerificationAssertions.AssertEqual(memory[NativeSnapshotMemory.HorizontalDecelerationMultiplier], samus.HorizontalSpeed.DecelerationMultiplier,
                $"update {frame}: Samus horizontal deceleration multiplier");
            Check("Samus echo sound latch", samus.HorizontalSpeed.EchoSoundFlag, NativeSnapshotMemory.SpeedEchoSoundLatch);
            Check("Samus slope adjustment", samus.Kinematics.PositionAdjustedBySlope ? (ushort)1 : (ushort)0,
                NativeSnapshotMemory.SamusSlopeAdjusted);
            for (int direction = 0; direction < 4; direction++)
                Check($"Samus solid enemy {direction}", SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.WordOf(samus.Kinematics.SolidEnemyCollisions[direction]),
                    NativeSnapshotMemory.SamusSolidEnemyIndices + direction * 2);
            var inheritance = new ProjectileInheritanceInspection(samus.ProjectileInheritance);
            for (int offset = 0; offset < inheritance.ByteCount; offset += 2)
            {
                int address = NativeSnapshotMemory.ProjectileInheritancePrefix + offset;
                Check($"Projectile inherited movement ${address:X4}", inheritance.Word(offset), address);
            }
            Check("Samus total horizontal speed", samus.HorizontalSpeed.TotalSpeed.Whole, NativeSnapshotMemory.TotalHorizontalSpeed);
            Check("Samus total horizontal fraction", samus.HorizontalSpeed.TotalSpeed.Fraction, NativeSnapshotMemory.TotalHorizontalSubspeed);
            Check("Samus movement handler", samus.KnockbackActive
                ? NativeSnapshotMemory.KnockbackMovementHandler : NativeSnapshotMemory.NormalMovementHandler,
                NativeSnapshotMemory.SamusMovementHandler);
            // DoorTransitionState installs a locked pair to suppress host control, while
            // native door dispatch retains ordinary alpha/beta pointers unused.
            bool nativeControlLock = samus.Control.AlphaLocked && game.GameState is not
                (SuperMetroidGameState.HitDoorBlock or SuperMetroidGameState.LoadingNextRoomA or
                 SuperMetroidGameState.LoadingNextRoomB);
            Check("Samus alpha handler", nativeControlLock
                ? NativeSnapshotMemory.LockedAlphaHandler : NativeSnapshotMemory.NormalAlphaHandler,
                NativeSnapshotMemory.SamusAlphaHandler);
            Check("Samus beta handler", nativeControlLock
                ? NativeSnapshotMemory.LockedBetaHandler : NativeSnapshotMemory.NormalBetaHandler,
                NativeSnapshotMemory.SamusBetaHandler);

            Check("Samus health", SamusQuantity.Encode(samus.Health), NativeSnapshotMemory.Health);
            Check("Samus general Samus damage immunity countdown", samus.InvincibilityTimer.RawValue, NativeSnapshotMemory.InvincibilityTimer);
            Check("Samus Samus knockback countdown", samus.KnockbackTimer.RawValue, NativeSnapshotMemory.KnockbackTimer);
            Check("Samus Samus knockback direction", (ushort)samus.KnockbackDirection, NativeSnapshotMemory.KnockbackDirection);
            Check("Samus horizontal knockback direction", (ushort)samus.KnockbackXDirection, NativeSnapshotMemory.KnockbackXDirection);
            Check("Samus hurt palette/audio recovery countdown", checked((ushort)samus.HurtFlashCounter), NativeSnapshotMemory.HurtFlashCounter);
            Check("Samus fractional health word", checked((ushort)samus.SubunitHealth), NativeSnapshotMemory.SubunitHealth);
            Check("Samus selected HUD weapon", samus.SelectedHudItem.ToWord(), NativeSnapshotMemory.SelectedHudItem);
            Check("Samus auto-cancel HUD selection", samus.AutoCancelHudItemIndex.ToWord(), NativeSnapshotMemory.AutoCancelHudItemIndex);
            Check("Samus acceleration mode", SamusAccelerationModeCodec.ToNative(samus.HorizontalSpeed.AccelerationMode), NativeSnapshotMemory.AccelerationMode);
            Check("Samus maximum health", SamusQuantity.Encode(samus.MaxHealth), NativeSnapshotMemory.MaxHealth);
            Check("Samus equipped items", samus.EquippedItems.ToEquipmentWord(), NativeSnapshotMemory.Items);
            Check("Samus collected items", samus.CollectedItems.ToEquipmentWord(), NativeSnapshotMemory.CollectedItems);
            Check("Samus equipped beams", samus.EquippedBeams.ToEquipmentWord(), NativeSnapshotMemory.Beams);
            Check("Samus collected beams", samus.CollectedBeams.ToEquipmentWord(), NativeSnapshotMemory.CollectedBeams);
            Check("Samus missiles", SamusQuantity.Encode(samus.Missiles), NativeSnapshotMemory.Missiles);
            Check("Samus missile capacity", SamusQuantity.Encode(samus.MaxMissiles), NativeSnapshotMemory.MaxMissiles);
            Check("Samus super missiles", SamusQuantity.Encode(samus.SuperMissiles), NativeSnapshotMemory.SuperMissiles);
            Check("Samus super missile capacity", SamusQuantity.Encode(samus.MaxSuperMissiles), NativeSnapshotMemory.MaxSuperMissiles);
            Check("Samus power bombs", SamusQuantity.Encode(samus.PowerBombs), NativeSnapshotMemory.PowerBombs);
            Check("Samus power bomb capacity", SamusQuantity.Encode(samus.MaxPowerBombs), NativeSnapshotMemory.MaxPowerBombs);
            Check("Samus reserve mode", samus.ReserveTankMode.ToWord(), NativeSnapshotMemory.ReserveMode);
            Check("Samus reserve capacity", SamusQuantity.Encode(samus.MaxReserveEnergy), NativeSnapshotMemory.MaxReserve);
            Check("Samus reserve energy", SamusQuantity.Encode(samus.ReserveEnergy), NativeSnapshotMemory.Reserve);
            // The native CPU can still be decompressing source-room tiles while
            // IRQ scrolling advances; the port loads the destination atomically.
            // Align these owners at completed loading, not elapsed upload time.
            // Movement/input remain compared on EVERY IRQ interval. A stable owner
            // snapshot also proves the port does not run destination actors early.
            bool nativeLoading = game.GameState == SuperMetroidGameState.LoadingNextRoomB &&
                W(NativeSnapshotMemory.DoorFunction) is NativeSnapshotMemory.PlaceSamusLoadTiles or NativeSnapshotMemory.LoadMoreThings;
            bool portWaitingForScroll = game.DoorTransitionPhaseForVerification is
                DoorTransitionPhase.WaitForDoorOpeningScroll or DoorTransitionPhase.FinishDoorLoading;
            bool deferLoadingOwners = nativeLoading && portWaitingForScroll;
            // Gameplay holds no colour image (#1512): its pictures show the import's colours under the shadings it publishes.
            ReadOnlySpan<RgbColor> comparedPalette = GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources);
            IEnumerable<int> comparedColours = GameplayShownColours.Compared;
            if (game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB or
                SuperMetroidGameState.UnpausingA or SuperMetroidGameState.UnpausingB)
            {
                // The pause screen holds no colours; its pictures show the import's colours under the shadings it names (#1512).
                PauseMenuState menu = game.Session.ScreenAs<PauseMenuState>() ?? throw new InvalidDataException("Native pause palette requires the active pause menu.");
                comparedPalette = PauseShownColours.Of(menu);
                comparedColours = Enumerable.Range(0, SnesCgram.ColorCount);
            }
            if (!deferLoadingOwners)
            foreach (int color in comparedColours)
            {
                ushort nativeColor = (ushort)(W(NativeSnapshotMemory.PaletteBuffer + color * 2) & 0x7fff);
                if (comparedPalette[color].ToWord() != nativeColor)
                    mismatches.Add($"Palette {color}: native={nativeColor:X4} port={comparedPalette[color]:X4}");
            }
            if (deferLoadingOwners)
            {
                if (game.DoorTransitionPhaseForVerification == DoorTransitionPhase.FinishDoorLoading)
                    Check("Completed-scroll visor", GameplayShownColours.Of(runtime, InstalledFixtures.SamusShadeSources)[XrayScopeDefinitionsPaletteConstants.VisorCgramIndex].ToWord(),
                        NativeSnapshotMemory.PaletteBuffer + XrayScopeDefinitionsPaletteConstants.VisorCgramIndex * 2);
                ushort[] owners = CaptureLoadedOwners();
                if (pendingLoadedOwners is not null && !owners.SequenceEqual(pendingLoadedOwners))
                    throw new InvalidDataException("Destination RNG/palette/enemy owners advanced while native hardware loading was still pending.");
                pendingLoadedOwners ??= owners;
                loadingIntervals++;
            }
            else
            {
                if (pendingLoadedOwners is not null)
                    VerificationAssertions.AssertEqual(NativeSnapshotMemory.HandleAnimTiles, W(NativeSnapshotMemory.DoorFunction), "deferred destination owners reach native completed-loading boundary");
                Check("RNG", runtime.System.RandomNumber, NativeSnapshotMemory.Random);
            }
            foreach (var actor in runtime.Enemies.SlotTable.Slots)
            {
                if (deferLoadingOwners) continue;
                int address = NativeSnapshotMemory.EnemyBase + SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(actor.SlotIndex);
                string owner = $"Enemy {actor.SlotIndex}";
                Check(owner + " identity", SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(actor.DefinitionId), address);
                if (actor.DefinitionId == EnemyDefinitionId.None || W(address) == 0) continue;
                Check(owner + " X", actor.Position.X.Whole, address + 2);
                Check(owner + " X fraction", actor.Position.X.Fraction, address + 4);
                Check(owner + " Y", actor.Position.Y.Whole, address + 6);
                Check(owner + " Y fraction", actor.Position.Y.Fraction, address + 8);
                Check(owner + " health", checked((ushort)actor.Health), address + 20);
                Check(owner + " spritemap", actor.SpritemapPointer, address + 22);
                // Only actors whose animation cursor translates to a native pointer are compared on it (#1471).
                if (pointers.TranslatedEnemy(runtime.Enemies, actor) is { } instruction)
                    Check(owner + " instruction", instruction, address + 26);
                Check(owner + " instruction timer", actor.FrameHold.RawValue, address + 28);
                Check(owner + " XRadius", actor.Radius.X, address + 10);
                Check(owner + " YRadius", actor.Radius.Y, address + 12);
                // The native words also hold species bits that species code writes and nothing reads (#1472); the port
                // keeps the engine flags, which are compared.
                CheckValue(owner + " Properties", SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Encode(actor.Properties),
                    SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Encode(
                        SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.EngineFlagsOf(W(address + 14))));
                CheckValue(owner + " ExtraProperties", SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Encode(actor.ExtraProperties),
                    SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.Encode(
                        SuperMetroid.AssetExtraction.Enemies.EnemyPropertyWordCodec.ExtraFlagsOf(W(address + 16))));
                Check(owner + " AiStates", SuperMetroid.AssetExtraction.Enemies.EnemyAiStatesWordCodec.Encode(actor.AiStates), address + 18);
                Check(owner + " Timer", actor.Timer.RawValue, address + 24);
                // The port holds the colouring; the row holding it is compared as the native palette word that row is stored as.
                Check(owner + " PaletteRow", NativeSpriteGraphicsWords.PaletteBits(actor.PaletteRowIn(runtime.Enemies)), address + 30);
                // The port's slots carry no tile index (#1513); the index the import composes the occupant's sheet at is compared.
                Check(owner + " TileIndexBase", actor.NativeTileBaseIn(runtime.Enemies), address + 32);
                Check(owner + " Layer", actor.Layer.Order, address + 34);
                Check(owner + " FlashTimer", actor.FlashTimer.RawValue, address + 36);
                Check(owner + " FrozenTimer", actor.FrozenTimer.RawValue, address + 38);
                Check(owner + " InvincibilityTimer", actor.InvincibilityTimer.RawValue, address + 40);
                Check(owner + " ShakeTimer", actor.ShakeTimer.RawValue, address + 42);
                Check(owner + " FrameCounter", actor.FrameCounter, address + 44);
            }
            if (!deferLoadingOwners && runtime.Enemies.RidleyEncounter.Ridley is { } ridleyState &&
                W(NativeSnapshotMemory.EnemyBase) == SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(EnemyDefinitionId.Ridley))
            {
                bool expectedGate = (W(NativeSnapshotMemory.EnemyBase + 14) & 0x0400) != 0;
                if (runtime.Enemies.SlotTable.Slots[0].Properties.HasAny(EnemyProperties.IgnoreSamusCollision) != expectedGate)
                    mismatches.Add($"Ridley interaction gate: native={expectedGate}");
                // Ridley's AI function is an identity (#1413): the native routine word is translated and the identities compared.
                ushort nativeFunctionWord = W(NativeSnapshotMemory.RidleyFunction);
                if (!RidleyNativeWords.AiFunctions.TryDecode(nativeFunctionWord, out RidleyAiFunction nativeFunction) ||
                    nativeFunction != ridleyState.Function)
                    mismatches.Add($"Ridley AI function: native={nativeFunctionWord:X4} port={ridleyState.Function}");
                Check("Ridley AI timer", ridleyState.FunctionTimer.RawValue, NativeSnapshotMemory.RidleyFunctionTimer);
                Check("Ridley TailFunctionIndex", (ushort)ridleyState.TailFunctionIndex, NativeSnapshotMemory.RidleyTailFunctionIndex);
                Check("Ridley IdleTailWhipEnabled", ridleyState.IdleTailWhipEnabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.RidleyIdleTailWhipEnabled);
                Check("Ridley TailWhipRequest", checked((ushort)ridleyState.TailWhipRequest), NativeSnapshotMemory.RidleyTailWhipRequest);
                Check("Ridley TailExtensionSpeed", checked((ushort)ridleyState.TailExtensionSpeed), NativeSnapshotMemory.RidleyTailExtensionSpeed);
                Check("Ridley TailAngleDelta", checked((ushort)ridleyState.TailAngleDelta), NativeSnapshotMemory.RidleyTailAngleDelta);
                Check("Ridley TailMinimumClockwiseAngle", ridleyState.TailMinimumClockwiseAngle.RawValue, NativeSnapshotMemory.RidleyTailMinimumClockwiseAngle);
                Check("Ridley TailMaximumCounterClockwiseAngle", ridleyState.TailMaximumCounterClockwiseAngle.RawValue, NativeSnapshotMemory.RidleyTailMaximumCounterClockwiseAngle);
                Check("Ridley TailWhipTargetClockwiseAngle", WhipTargetWord(ridleyState.ClockwiseWhipTarget), NativeSnapshotMemory.RidleyTailWhipTargetClockwiseAngle);
                Check("Ridley TailWhipTargetCounterClockwiseAngle", WhipTargetWord(ridleyState.CounterClockwiseWhipTarget), NativeSnapshotMemory.RidleyTailWhipTargetCounterClockwiseAngle);
                Check("Ridley IdealInterSegmentTailAngle", checked((ushort)ridleyState.IdealInterSegmentTailAngle), NativeSnapshotMemory.RidleyIdealInterSegmentTailAngle);
                Check("Ridley HorizontalVelocity", ridleyState.Velocity.X.RawValue, NativeSnapshotMemory.RidleyHorizontalVelocity);
                Check("Ridley VerticalVelocity", ridleyState.Velocity.Y.RawValue, NativeSnapshotMemory.RidleyVerticalVelocity);
                Check("Ridley FightMode", RidleyNativeWords.FightModes.Word(ridleyState.FightMode), NativeSnapshotMemory.RidleyFightMode);
                Check("Ridley MovementAnimationEnabled", ridleyState.MovementAnimationEnabled ? (ushort)1 : (ushort)0, NativeSnapshotMemory.RidleyMovementAnimationEnabled);
                Check("Ridley WingFrame", checked((ushort)ridleyState.WingFrame), NativeSnapshotMemory.RidleyWingFrame);
                Check("Ridley WingAnimationTimerDelta", checked((ushort)ridleyState.WingAnimationTimerDelta), NativeSnapshotMemory.RidleyWingAnimationTimerDelta);
                Check("Ridley WingAnimationTimer", ridleyState.WingAnimationTimer.RawValue, NativeSnapshotMemory.RidleyWingAnimationTimer);
                Check("Ridley FacingDirection", RidleyNativeWords.Facings.Word(ridleyState.FacingDirection), NativeSnapshotMemory.RidleyFacingDirection);
                Check("Ridley HealthStage", checked((ushort)ridleyState.HealthStage), NativeSnapshotMemory.RidleyHealthStage);
                Check("Ridley GrabXOffset", unchecked((ushort)ridleyState.GrabOffset.X), NativeSnapshotMemory.RidleyGrabXOffset);
                Check("Ridley GrabYOffset", unchecked((ushort)ridleyState.GrabOffset.Y), NativeSnapshotMemory.RidleyGrabYOffset);
                Check("Ridley TailDamage", checked((ushort)ridleyState.TailDamage), NativeSnapshotMemory.RidleyTailDamage);
                Check("Ridley FeetDistanceIndex", checked((ushort)(ridleyState.FeetDistanceIndex * sizeof(ushort))), NativeSnapshotMemory.RidleyFeetDistanceIndex);
                Check("Ridley IntangibilityTimer", ridleyState.IntangibilityTimer.RawValue, NativeSnapshotMemory.RidleyIntangibilityTimer);
                if (ridleyState.Function is RidleyAiFunction.NorfairSwoopMoveToStart or
                    RidleyAiFunction.NorfairSwoopAimDown or RidleyAiFunction.NorfairSwoopAimSideways or
                    RidleyAiFunction.NorfairSwoopAimUp or RidleyAiFunction.NorfairSwoopClimb or RidleyAiFunction.NorfairSwoopRecover)
                    Check("Ridley swoop timer", ridleyState.SwoopPhaseTimer.RawValue, NativeSnapshotMemory.RidleySwoopTimer);
                if (game.GameState == SuperMetroidGameState.MainGameplay)
                {
                    // Tail workspace becomes live after its first fade-owned composition.
                    Check("Ridley tail tip X", ridleyState.TailSegments[6].Position.X, NativeSnapshotMemory.TailTipX);
                    Check("Ridley tail tip Y", ridleyState.TailSegments[6].Position.Y, NativeSnapshotMemory.TailTipY);
                    for (int tailIndex = 0; tailIndex < ridleyState.TailSegments.Length; tailIndex++)
                    {
                        var segment = ridleyState.TailSegments[tailIndex];
                        int tailAddress = NativeSnapshotMemory.TailSegments + tailIndex * NativeSnapshotMemory.TailSegmentStride;
                        string tailOwner = $"Ridley tail {tailIndex}";
                        Check(tailOwner + " active", segment.Active ? NativeSnapshotMemory.TailSegmentActive : (ushort)0, tailAddress);
                        // A propagated stagger has no angle in the port (#1413); the cartridge stores -1 there.
                        Check(tailOwner + " StaggerAngle", segment.StaggerAngle is { } stagger ? checked((ushort)stagger) : ushort.MaxValue, tailAddress + 2);
                        Check(tailOwner + " MovementDirection", segment.Clockwise ? (ushort)0x8000 : (ushort)0, tailAddress + 4);
                        Check(tailOwner + " Distance", checked((ushort)segment.Distance), tailAddress + 6);
                        Check(tailOwner + " TargetDistance", checked((ushort)segment.TargetDistance), tailAddress + 8);
                        Check(tailOwner + " Angle", checked((ushort)segment.Angle), tailAddress + 10);
                        Check(tailOwner + " XPosition", segment.Position.X, tailAddress + 12);
                        Check(tailOwner + " YPosition", segment.Position.Y, tailAddress + 14);
                        Check(tailOwner + " XOffset", unchecked((ushort)segment.Offset.X), tailAddress + 16);
                        Check(tailOwner + " YOffset", unchecked((ushort)segment.Offset.Y), tailAddress + 18);
                    }
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                // The movie's memory lists PLMs by slot ordinal, and the table processes the highest ordinal first.
                var plmSlots = runtime.Plms.PopulationSlots.ToDictionary(slot => PlmSlotTable.Capacity - 1 - slot.ProcessingPosition);
                for (int slotIndex = 0; slotIndex < initialPlms.Length; slotIndex++)
                {
                    int offset = slotIndex * 2;
                    bool active = plmSlots.TryGetValue(slotIndex, out var slot);
                    Check($"PLM {slotIndex} header", active ? PlmNativeWords.Word(slot.HeaderPointer) : (ushort)0, NativeSnapshotMemory.PlmHeaders + offset);
                    if (!active || W(NativeSnapshotMemory.PlmHeaders + offset) == 0) continue;
                    ushort frameBehavior = slot.FrameBehavior;
                    var greyDoor = runtime.Plms.GreyDoorSnapshots.FirstOrDefault(door => door.Header == slot.HeaderPointer && door.BlockIndex == slot.BlockIndex);
                    VerificationAssertions.AssertTrue(greyDoor.Header != PlmHeaderId.None,
                        $"Movie PLM {slot.HeaderPointer} needs a family-variable coverage mapping");
                    Check($"PLM {slotIndex} grey-door condition", (ushort)((int)greyDoor.Condition * 2), NativeSnapshotMemory.PlmFamilyVariable + offset);
                    // Grey doors require one hit. Opening owns the incremented counter;
                    // before that transition setup's zero is the live counter value.
                    Check($"PLM {slotIndex} grey-door hit count", checked(greyDoor.HitCounter), NativeSnapshotMemory.PlmExtraVariable + offset);
                    // The typed cap names its native instruction and link through the oracle's step addresses; a closing
                    // cap's link is the one its previous program left, which the closing never reads.
                    GreyDoorPlmAnimation typedDoor = slot.Animation as GreyDoorPlmAnimation
                        ?? throw new InvalidDataException($"Movie grey door at block {slot.BlockIndex} runs no typed program.");
                    ushort instruction = DoorPlmNativeProgramOracle.GreyDoorNativeInstruction(bus, typedDoor);
                    if (greyDoor.Phase != GreyDoorPhase.Closing && typedDoor.Link is { } link)
                        Check($"PLM {slotIndex} grey-door link", DoorPlmNativeProgramOracle.GreyDoorNativeEntry(typedDoor.Variant, link),
                            NativeSnapshotMemory.PlmLinkInstruction + offset);
                    Check($"PLM {slotIndex} BlockIndex", unchecked((ushort)(slot.BlockIndex * 2)), NativeSnapshotMemory.PlmBlockIndex + offset);
                    Check($"PLM {slotIndex} FrameBehavior", frameBehavior, NativeSnapshotMemory.PlmFrameBehavior + offset);
                    Check($"PLM {slotIndex} InstructionPointer", instruction, NativeSnapshotMemory.PlmInstructionPointer + offset);
                    // The cartridge keeps a pending projectile hit in the loop counter's storage; a pending hit is
                    // compared as the family the recorded word decodes to, otherwise the loop counter is compared.
                    if (slot.ProjectileHit is { } pendingHit)
                    {
                        SamusProjectileFamily recordedHit = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(W(NativeSnapshotMemory.PlmLoopTimer + offset)).Family;
                        if (recordedHit != pendingHit.Family)
                            mismatches.Add($"PLM {slotIndex} projectile hit: native={recordedHit} port={pendingHit.Family}");
                    }
                    else
                    {
                        Check($"PLM {slotIndex} LoopTimer", slot.LoopTimer, NativeSnapshotMemory.PlmLoopTimer + offset);
                    }
                    Check($"PLM {slotIndex} RoomArgument", slot.RoomArgument, NativeSnapshotMemory.PlmRoomArgument + offset);
                    // The native sleep leaves the countdown at zero and lets it run on from $FFFF; the typed hold re-arms
                    // one on each pass. Both wake with one, so only a sleeping door's countdown is left uncompared.
                    if (typedDoor.Program != GreyDoorProgram.Sleep)
                        Check($"PLM {slotIndex} FrameHold", slot.FrameHold, NativeSnapshotMemory.PlmFrameHold + offset);
                }
                var activeLevel = runtime.Room.LevelData ?? throw new InvalidDataException("Missing active collision data.");
                for (int block = 0; block < activeLevel.Size.X * activeLevel.Size.Y; block++)
                {
                    // The native level word and BTS byte decode to the terrain and behaviour the port's grid holds.
                    RoomTerrainBlock expectedTerrain = RoomLevelWordCodec.Decode(W(NativeSnapshotMemory.Level + block * 2));
                    RoomTerrainBlock actualTerrain = activeLevel.TerrainGrid[block];
                    if (actualTerrain != expectedTerrain)
                        mismatches.Add($"Room block {block}: native={expectedTerrain} port={actualTerrain}");
                    var expectedBehavior = new RoomBlockBehavior(memory[NativeSnapshotMemory.Bts + block]);
                    RoomBlockBehavior actualBehavior = activeLevel.BehaviorGrid[block];
                    if (actualBehavior != expectedBehavior)
                        mismatches.Add($"Room behaviour {block}: native={expectedBehavior} port={actualBehavior}");
                }
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            {
                foreach (var trail in runtime.Projectiles.TrailSlots)
                {
                    Check($"Trail {trail.SlotIndex} Left timer", trail.Left.FrameHold.RawValue, NativeSnapshotMemory.TrailLeftFrameHold + trail.SlotTableOffset);
                    if (!trail.Left.FrameHold.IsZero)
                    {
                        Check($"Trail {trail.SlotIndex} Left InstructionPointer", trail.Left.InstructionPointer, NativeSnapshotMemory.TrailLeftInstructionPointer + trail.SlotTableOffset);
                        Check($"Trail {trail.SlotIndex} Left TileNumberAttributes", ProjectileTrailNative.NativeWord(trail.Left.ShownPose), NativeSnapshotMemory.TrailLeftTileNumberAttributes + trail.SlotTableOffset);
                        Check($"Trail {trail.SlotIndex} Left XPosition", trail.Left.Position.X, NativeSnapshotMemory.TrailLeftXPosition + trail.SlotTableOffset);
                        Check($"Trail {trail.SlotIndex} Left YPosition", trail.Left.Position.Y, NativeSnapshotMemory.TrailLeftYPosition + trail.SlotTableOffset);
                    }
                    Check($"Trail {trail.SlotIndex} Right timer", trail.Right.FrameHold.RawValue, NativeSnapshotMemory.TrailRightFrameHold + trail.SlotTableOffset);
                    if (!trail.Right.FrameHold.IsZero)
                    {
                        Check($"Trail {trail.SlotIndex} Right InstructionPointer", trail.Right.InstructionPointer, NativeSnapshotMemory.TrailRightInstructionPointer + trail.SlotTableOffset);
                        Check($"Trail {trail.SlotIndex} Right TileNumberAttributes", ProjectileTrailNative.NativeWord(trail.Right.ShownPose), NativeSnapshotMemory.TrailRightTileNumberAttributes + trail.SlotTableOffset);
                        Check($"Trail {trail.SlotIndex} Right XPosition", trail.Right.Position.X, NativeSnapshotMemory.TrailRightXPosition + trail.SlotTableOffset);
                        Check($"Trail {trail.SlotIndex} Right YPosition", trail.Right.Position.Y, NativeSnapshotMemory.TrailRightYPosition + trail.SlotTableOffset);
                    }
                }
                Check("Projectile cooldown", runtime.BombProjectiles.CooldownTimer.RawValue, NativeSnapshotMemory.ProjectileCooldown);
                Check("Beam charge", runtime.Projectiles.Charge.FlareCounter, NativeSnapshotMemory.BeamCharge);
                Check("ProjectileCounter", runtime.Projectiles.SlotTable.ProjectileCounter, NativeSnapshotMemory.ProjectileCount);
                Check("PreviousBeamChargeCounter", runtime.Projectiles.Charge.PreviousBeamChargeCounter, NativeSnapshotMemory.PreviousCharge);
                Check("ProjectileInvincibilityTimer", runtime.Projectiles.Firing.ProjectileInvincibilityTimer.RawValue, NativeSnapshotMemory.ProjectileInteractionImmunity);
                Check("ChargedShotGlowTimer", runtime.Projectiles.Charge.ChargedShotGlowTimer.RawValue, NativeSnapshotMemory.ChargedShotGlow);
                Check("ChargePaletteStep", runtime.Projectiles.Charge.ChargePaletteStep * sizeof(ushort), NativeSnapshotMemory.ChargePaletteIndex);
                Check("BombCounter", runtime.BombProjectiles.BombCounter, NativeSnapshotMemory.BombCount);
                // This movie never places a bomb. Check every physical bomb slot and
                // both activation owners so that absence is verified, not assumed from
                // the aggregate counter or from ordinary beam/missile comparisons.
                foreach (var bomb in runtime.BombProjectiles.Slots)
                {
                    int address = NativeSnapshotMemory.ProjectileType + (bomb.Index + 5) * 2;
                    VerificationAssertions.AssertTrue(bomb.Type.IsNone, $"Bomb {bomb.Index} holds no bomb");
                    VerificationAssertions.AssertTrue(W(address) == 0,
                        "Movie now activates a bomb slot; add its full live-state mapping");
                }
                var explosion = runtime.BombProjectiles.PowerBombExplosion;
                Check("Power-bomb armed flag", explosion.IsArmed ? ushort.MaxValue : (ushort)0, NativeSnapshotMemory.PowerBombArmedFlag);
                Check("Power-bomb explosion status", explosion.IsActive ? (ushort)(1 << 15) : (ushort)0, NativeSnapshotMemory.PowerBombExplosionStatus);
                VerificationAssertions.AssertTrue(W(NativeSnapshotMemory.PowerBombArmedFlag) == 0 &&
                    W(NativeSnapshotMemory.PowerBombExplosionStatus) == 0,
                    "Movie now activates a power bomb; add its full live-state mapping");
                Check("BombSpreadChargeTimeoutCounter", checked((ushort)samus.BombSpreadChargeTimeoutCounter), NativeSnapshotMemory.BombSpreadChargeTimeout);
                if (samus.PendingTransitionShot != SamusSnapshotWords.DecodeTransitionShot(W(NativeSnapshotMemory.PoseShotDirection)))
                    mismatches.Add($"PendingTransitionShot: native={W(NativeSnapshotMemory.PoseShotDirection):X4} port={samus.PendingTransitionShot}");
                Check("HyperBeam", SamusSnapshotWords.EncodeHyperBeam(samus.HyperBeamEnabled), NativeSnapshotMemory.HyperBeam);
                if (samus.ResumeChargingBeamSound != SamusSnapshotWords.DecodeResumeChargingBeamSound(W(NativeSnapshotMemory.ResumeChargeSound)))
                    mismatches.Add($"ResumeChargingBeamSound: native={W(NativeSnapshotMemory.ResumeChargeSound):X4} port={samus.ResumeChargingBeamSound}");

            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Projectiles.Slots.Take(5))
            {
                int index = projectile.SlotTableOffset;
                string owner = $"Projectile {projectile.SlotIndex}";
                // The recorded type, direction and missile variable words are compared as the projectile state they decode to.
                SamusProjectileType recordedType = SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeType(W(NativeSnapshotMemory.ProjectileType + index));
                if (projectile.Type != recordedType)
                    mismatches.Add($"{owner} type: native={recordedType} port={projectile.Type}");
                if (projectile.Type.IsNone || recordedType.IsNone) continue;
                Check(owner + " X", projectile.Position.X.Whole, NativeSnapshotMemory.ProjectileX + index);
                Check(owner + " Y", projectile.Position.Y.Whole, NativeSnapshotMemory.ProjectileY + index);
                Check(owner + " X radius", projectile.Radius.X, NativeSnapshotMemory.ProjectileXRadius + index);
                Check(owner + " Y radius", projectile.Radius.Y, NativeSnapshotMemory.ProjectileYRadius + index);
                Check(owner + " damage", projectile.Damage, NativeSnapshotMemory.ProjectileDamage + index);
                // A beam explosion never moves, so nothing reads its sub-pixel position; one a muzzle check killed keeps
                // the vertical fraction the original game's reset left, which no room hands over (RoomHandOver).
                if (!projectile.Type.IsFamily(SamusProjectileFamily.BeamExplosion))
                {
                    Check(owner + " XFraction", unchecked(projectile.Position.X.Fraction), NativeSnapshotMemory.ProjectileXFraction + index);
                    Check(owner + " YFraction", unchecked(projectile.Position.Y.Fraction), NativeSnapshotMemory.ProjectileYFraction + index);
                }
                Check(owner + " XVelocity", unchecked((ushort)projectile.Velocity.X.SignedRawValue), NativeSnapshotMemory.ProjectileXVelocity + index);
                Check(owner + " YVelocity", unchecked((ushort)projectile.Velocity.Y.SignedRawValue), NativeSnapshotMemory.ProjectileYVelocity + index);
                ushort recordedDirection = W(NativeSnapshotMemory.ProjectileDirection + index);
                if (projectile.Direction != SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeDirection(recordedDirection) ||
                    projectile.IsSpent != SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeSpent(recordedDirection))
                    mismatches.Add($"{owner} Direction: native={recordedDirection:X4} port={projectile.Direction} spent={projectile.IsSpent}");
                Check(owner + " Instruction", unchecked((ushort)projectile.InstructionPointer), NativeSnapshotMemory.ProjectileInstruction + index);
                ushort callback = projectile.FrameBehavior switch
                {
                    SamusProjectileFrameBehavior.None => NativeSnapshotMemory.ProjectileEmptyCallback,
                    SamusProjectileFrameBehavior.NoWaveBeam => SamusBeamFrameBehaviorCodes.NoWave,
                    SamusProjectileFrameBehavior.WaveBeamThreeFrameTrail => SamusBeamFrameBehaviorCodes.WaveThreeFrameTrail,
                    SamusProjectileFrameBehavior.WaveBeamFourFrameTrail => SamusBeamFrameBehaviorCodes.WaveFourFrameTrail,
                    SamusProjectileFrameBehavior.Missile => NativeSnapshotMemory.ProjectileMissileCallback,
                    SamusProjectileFrameBehavior.SuperMissile => NativeSnapshotMemory.ProjectileSuperMissileCallback,
                    SamusProjectileFrameBehavior.SuperMissileLink => NativeSnapshotMemory.ProjectileSuperMissileLinkCallback,
                    SamusProjectileFrameBehavior.HyperBeam or
                        SamusProjectileFrameBehavior.IceCombo or
                        SamusProjectileFrameBehavior.IceComboOutward or
                        SamusProjectileFrameBehavior.WaveCombo or
                        SamusProjectileFrameBehavior.SpazerCombo or
                        SamusProjectileFrameBehavior.PlasmaCombo or
                        SamusProjectileFrameBehavior.SpazerComboFalling or
                        SamusProjectileFrameBehavior.ShinesparkEcho or
                        SamusProjectileFrameBehavior.SpacetimePaletteCopyTail or
                        SamusProjectileFrameBehavior.ChainsawPowerBomb or
                        SamusProjectileFrameBehavior.ChargedChainsawOverrun or
                        SamusProjectileFrameBehavior.MurderBeamMisalignedExecution => throw new InvalidDataException($"Movie projectile callback {projectile.FrameBehavior} needs a native identity mapping"),
                    _ => throw new InvalidDataException($"Movie projectile callback {projectile.FrameBehavior} needs a native identity mapping"),
                };
                Check(owner + " FrameBehavior", callback, NativeSnapshotMemory.ProjectileFrameBehavior + index);
                Check(owner + " FrameHold", unchecked(projectile.FrameHold.RawValue), NativeSnapshotMemory.ProjectileFrameHold + index);
                // Only missiles keep state in the variable word that this movie reaches: their ignition and link.
                if (projectile.Type.Family is SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile)
                {
                    ushort recordedVariable = W(NativeSnapshotMemory.ProjectileVariable + index);
                    if (projectile.MissileIgnition != SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeMissileIgnition(recordedVariable) ||
                        projectile.SuperMissileLinkSlot != SuperMetroid.AssetExtraction.Samus.SamusProjectileWordCodec.DecodeMissileLinkSlot(recordedVariable))
                        mismatches.Add($"{owner} missile ignition: native={recordedVariable:X4} port={projectile.MissileIgnition}/{projectile.SuperMissileLinkSlot}");
                }
                Check(owner + " TrailTimer", unchecked(projectile.TrailTimer.RawValue), NativeSnapshotMemory.ProjectileTrailTimer + index);
                Check(owner + " AuxiliaryPhase", unchecked((ushort)projectile.AuxiliaryPhase), NativeSnapshotMemory.ProjectileAuxiliaryPhase + index);
                Check(owner + " Spritemap", unchecked((ushort)projectile.SpritemapPointer), NativeSnapshotMemory.ProjectileSpritemap + index);
            }
            if (game.GameState == SuperMetroidGameState.MainGameplay)
            foreach (var projectile in runtime.Enemies.Projectiles.EnemyProjectiles)
            {
                int index = projectile.SlotIndex * 2;
                string owner = $"Enemy projectile {projectile.SlotIndex}";
                Check(owner + " identity", EnemyProjectileNativeWords.Kinds.Word(projectile.Kind), NativeSnapshotMemory.EnemyProjectileId + index);
                if (!projectile.IsActive || W(NativeSnapshotMemory.EnemyProjectileId + index) == 0) continue;
                Check(owner + " X", projectile.Position.X.Whole, NativeSnapshotMemory.EnemyProjectileX + index);
                Check(owner + " Y", projectile.Position.Y.Whole, NativeSnapshotMemory.EnemyProjectileY + index);
                // The port holds a tile base and colouring; they are compared, with the row holding the colouring, as the native
                // graphics word that stores them.
                Check(owner + " Graphics", NativeSpriteGraphicsWords.GraphicsWord(projectile.Graphics, runtime.Enemies, projectile.Graphics.PaletteRowIn(runtime.Enemies)),
                    NativeSnapshotMemory.EnemyProjectileGraphics + index);
                Check(owner + " Timer", projectile.GeneralTimer, NativeSnapshotMemory.EnemyProjectileTimer + index);
                EnemyProjectileFrameBehavior nativeFrameBehavior = EnemyProjectileFrameBehaviorAccess.FromWord(W(NativeSnapshotMemory.EnemyProjectileFrameBehavior + index));
                if (projectile.FrameBehavior != nativeFrameBehavior)
                    mismatches.Add($"{owner} FrameBehavior: native={nativeFrameBehavior} port={projectile.FrameBehavior}");
                Check(owner + " XFraction", projectile.Position.X.Fraction, NativeSnapshotMemory.EnemyProjectileXFraction + index);
                Check(owner + " YFraction", projectile.Position.Y.Fraction, NativeSnapshotMemory.EnemyProjectileYFraction + index);
                Check(owner + " XVelocity", projectile.Velocity.X.RawValue, NativeSnapshotMemory.EnemyProjectileXVelocity + index);
                Check(owner + " YVelocity", projectile.Velocity.Y.RawValue, NativeSnapshotMemory.EnemyProjectileYVelocity + index);
                Check(owner + " Instruction", pointers.Projectile(projectile), NativeSnapshotMemory.NativeEnemyProjectileInstruction + index);
                ushort nativeMap = W(NativeSnapshotMemory.EnemyProjectileSpritemap + index);
                if (checkedProjectileCompositions.Add(new CheckedProjectileComposition(projectile.PresentationOperandAddress, projectile.SpritemapPointer, nativeMap)))
                {
                    var artwork = (runtime.Enemies.TileArtwork ?? throw new InvalidOperationException("Verification requires runtime.Enemies.TileArtwork.")).ProjectileSpritemaps;
                    var parts = projectile.PresentationOperandAddress != 0
                        ? (artwork ?? throw new InvalidOperationException("Verification requires artwork.")).GetProgramFrame(projectile.PresentationOperandAddress)
                        : (artwork ?? throw new InvalidOperationException("Verification requires artwork.")).Get(projectile.SpritemapPointer);
                    var expectedOam = new OamBuffer();
                    var actualOam = new OamBuffer();
                    expectedOam.BeginFrame();
                    actualOam.BeginFrame();
                    ImportedSpritemapOracle.DrawEnemyProjectile(CartridgeImportSource.Require(bus), expectedOam, nativeMap, new(128, 96), 0, true);
                    actualOam.AddEnemySpritemap(parts.Span, new Position16<SpritePixels>(128, 96), 0, 0,
                        clipVerticalWrap: true, originYIsOnScreen: true);
                    if (actualOam.NextByteOffset != expectedOam.NextByteOffset ||
                        !actualOam.LowTable.SequenceEqual(expectedOam.LowTable) ||
                        !actualOam.HighTable.SequenceEqual(expectedOam.HighTable))
                        mismatches.Add(owner + $" composition differs from native ${nativeMap:X4}");
                }
                Check(owner + " FrameHold", projectile.FrameHold.RawValue, NativeSnapshotMemory.EnemyProjectileFrameHold + index);
                Check(owner + " radii", LittleEndianBytes.Word(unchecked((byte)projectile.Radius.X), unchecked((byte)projectile.Radius.Y)), NativeSnapshotMemory.EnemyProjectileRadius + index);
                ushort properties = checked((ushort)projectile.Damage);
                if (projectile.Depth == EnemyProjectileDepth.InFrontOfSamus) properties |= NativeSnapshotMemory.EnemyProjectileInFrontOfSamus;
                if (!projectile.CanDamageSamus) properties |= NativeSnapshotMemory.EnemyProjectileNoContact;
                if (projectile.PersistsOnSamusContact) properties |= NativeSnapshotMemory.EnemyProjectilePersistent;
                if (projectile.BlocksSamusProjectiles) properties |= NativeSnapshotMemory.EnemyProjectileShotCollision;
                Check(owner + " properties", properties, NativeSnapshotMemory.EnemyProjectileProperties + index);
                switch (projectile.Kind)
                {
                    case RoomEnemyProjectileKind.CeresRidleyFireball:
                        // $86:940E consumes F only as a zero/nonzero afterburn gate.
                        // The caller may leave a noncanonical nonzero parameter (e.g. $E).
                        if ((projectile.RemainingAfterburns != 0) !=
                            (W(NativeSnapshotMemory.EnemyProjectileVariableF + index) == 0))
                            mismatches.Add(owner + " fireball afterburn gate differs");
                        break;
                    case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnCenter:
                    case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnCenter:
                    case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnRight:
                    case RoomEnemyProjectileKind.CeresRidleyHorizontalAfterburnLeft:
                    case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnUp:
                    case RoomEnemyProjectileKind.CeresRidleyVerticalAfterburnDown:
                        Check(owner + " remaining afterburns", checked((ushort)projectile.RemainingAfterburns), NativeSnapshotMemory.EnemyProjectileVariableE + index);
                        Check(owner + " next afterburn kind", EnemyProjectileNativeWords.Kinds.Word(projectile.NextAfterburnKind), NativeSnapshotMemory.EnemyProjectileVariableF + index);
                        break;
                    case RoomEnemyProjectileKind.MiscDustExplosion:
                    case RoomEnemyProjectileKind.EnemyDeathExplosion:
                    case RoomEnemyProjectileKind.EnemyDeathPickup:
                        break;
                    case RoomEnemyProjectileKind.None:
                    case RoomEnemyProjectileKind.PrePhantoonRoom:
                    case RoomEnemyProjectileKind.YappingMawBody:
                    case RoomEnemyProjectileKind.SkreeParticleDownRight:
                    case RoomEnemyProjectileKind.SkreeParticleUpRight:
                    case RoomEnemyProjectileKind.SkreeParticleDownLeft:
                    case RoomEnemyProjectileKind.SkreeParticleUpLeft:
                    case RoomEnemyProjectileKind.MetareeParticleDownRight:
                    case RoomEnemyProjectileKind.MetareeParticleUpRight:
                    case RoomEnemyProjectileKind.MetareeParticleDownLeft:
                    case RoomEnemyProjectileKind.MetareeParticleUpLeft:
                    case RoomEnemyProjectileKind.CrocomireProjectile:
                    case RoomEnemyProjectileKind.CrocomireBridgeCrumbling:
                    case RoomEnemyProjectileKind.CrocomireSpikeWallPieces:
                    case RoomEnemyProjectileKind.KraidSpitRock:
                    case RoomEnemyProjectileKind.KraidCeilingRock:
                    case RoomEnemyProjectileKind.KraidRisingRockLeft:
                    case RoomEnemyProjectileKind.KraidRisingRockRight:
                    case RoomEnemyProjectileKind.PhantoonDestroyableFlame:
                    case RoomEnemyProjectileKind.PhantoonStartingFlame:
                    case RoomEnemyProjectileKind.DraygonGoop:
                    case RoomEnemyProjectileKind.DraygonWallTurret:
                    case RoomEnemyProjectileKind.MotherBrainRoomTurret:
                    case RoomEnemyProjectileKind.MotherBrainRoomTurretBullet:
                    case RoomEnemyProjectileKind.MotherBrainGlassShard:
                    case RoomEnemyProjectileKind.MotherBrainGlassSparkle:
                    case RoomEnemyProjectileKind.MotherBrainOnionRing:
                    case RoomEnemyProjectileKind.OldTourianEscapeShaftFakeWallExplosion:
                    case RoomEnemyProjectileKind.MotherBrainBomb:
                    case RoomEnemyProjectileKind.MotherBrainHandBeamCharging:
                    case RoomEnemyProjectileKind.MotherBrainHandBeamFired:
                    case RoomEnemyProjectileKind.MotherBrainRainbowBeamCharging:
                    case RoomEnemyProjectileKind.MotherBrainPurpleBreathBig:
                    case RoomEnemyProjectileKind.MotherBrainPurpleBreathSmall:
                    case RoomEnemyProjectileKind.MotherBrainDrool:
                    case RoomEnemyProjectileKind.MotherBrainDyingDrool:
                    case RoomEnemyProjectileKind.MotherBrainRainbowBeamExplosion:
                    case RoomEnemyProjectileKind.MotherBrainDeathExplosion:
                    case RoomEnemyProjectileKind.MotherBrainEscapeDoorFragment:
                    case RoomEnemyProjectileKind.MotherBrainEscapeSubtitle:
                    case RoomEnemyProjectileKind.MotherBrainTopRightTube:
                    case RoomEnemyProjectileKind.MotherBrainTopLeftTube:
                    case RoomEnemyProjectileKind.MotherBrainTopMiddleLeftTube:
                    case RoomEnemyProjectileKind.MotherBrainTopMiddleRightTube:
                    case RoomEnemyProjectileKind.CeresFallingDebrisLight:
                    case RoomEnemyProjectileKind.CeresFallingDebrisDark:
                    case RoomEnemyProjectileKind.GunshipLiftoffDustCloud:
                    case RoomEnemyProjectileKind.AlcoonFireball:
                    case RoomEnemyProjectileKind.PowampSpike:
                    case RoomEnemyProjectileKind.WorkRobotLaserUpLeft:
                    case RoomEnemyProjectileKind.WorkRobotLaserHorizontal:
                    case RoomEnemyProjectileKind.WorkRobotLaserDownLeft:
                    case RoomEnemyProjectileKind.WorkRobotLaserUpRight:
                    case RoomEnemyProjectileKind.WorkRobotLaserDownRight:
                    case RoomEnemyProjectileKind.FallingSpark:
                    case RoomEnemyProjectileKind.NuclearWaffleBody:
                    case RoomEnemyProjectileKind.FakeKraidSpit:
                    case RoomEnemyProjectileKind.FakeKraidSpikeLeft:
                    case RoomEnemyProjectileKind.FakeKraidSpikeRight:
                    case RoomEnemyProjectileKind.KiHunterAcidSpitLeft:
                    case RoomEnemyProjectileKind.KiHunterAcidSpitRight:
                    case RoomEnemyProjectileKind.PirateMotherBrainLaser:
                    case RoomEnemyProjectileKind.PirateClaw:
                    case RoomEnemyProjectileKind.PolypRock:
                    case RoomEnemyProjectileKind.CacatacSpike:
                    case RoomEnemyProjectileKind.StokeProjectile:
                    case RoomEnemyProjectileKind.NamiheFireball:
                    case RoomEnemyProjectileKind.FuneFireball:
                    case RoomEnemyProjectileKind.LavaThrownByMagdollite:
                    case RoomEnemyProjectileKind.DragonFireball:
                    case RoomEnemyProjectileKind.EyeDoorProjectile:
                    case RoomEnemyProjectileKind.EyeDoorSweat:
                    case RoomEnemyProjectileKind.EyeDoorSmoke:
                    case RoomEnemyProjectileKind.KagoBug:
                    case RoomEnemyProjectileKind.BotwoonBody:
                    case RoomEnemyProjectileKind.BotwoonSpit:
                    case RoomEnemyProjectileKind.BombTorizoLowHealthDrool:
                    case RoomEnemyProjectileKind.BombTorizoInitialDrool:
                    case RoomEnemyProjectileKind.BombTorizoExplosiveSwipe:
                    case RoomEnemyProjectileKind.BombTorizoLowHealthExplosion:
                    case RoomEnemyProjectileKind.BombTorizoDeathExplosion:
                    case RoomEnemyProjectileKind.BombTorizoStatueBreaking:
                    case RoomEnemyProjectileKind.BombTorizoChozoOrb:
                    case RoomEnemyProjectileKind.BombTorizoSonicBoom:
                    case RoomEnemyProjectileKind.GoldenTorizoChozoOrb:
                    case RoomEnemyProjectileKind.GoldenTorizoSonicBoom:
                    case RoomEnemyProjectileKind.BombTorizoRightFootDust:
                    case RoomEnemyProjectileKind.BombTorizoLeftFootDust:
                    case RoomEnemyProjectileKind.GoldenTorizoEgg:
                    case RoomEnemyProjectileKind.GoldenTorizoSuperMissile:
                    case RoomEnemyProjectileKind.GoldenTorizoEyeBeam:
                    case RoomEnemyProjectileKind.TourianStatueSplash:
                    case RoomEnemyProjectileKind.TourianStatueEyeGlow:
                    case RoomEnemyProjectileKind.TourianStatueParticle:
                    case RoomEnemyProjectileKind.TourianStatueTail:
                    case RoomEnemyProjectileKind.TourianStatueSoul:
                    case RoomEnemyProjectileKind.TourianStatueRidley:
                    case RoomEnemyProjectileKind.TourianStatuePhantoon:
                    case RoomEnemyProjectileKind.TourianStatueBaseDecoration:
                    case RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstep:
                    case RoomEnemyProjectileKind.WreckedShipChozoSpikeFootstepAlternate:
                    case RoomEnemyProjectileKind.TourianStatueDescentDust:
                    case RoomEnemyProjectileKind.ShaktoolAttackFrontCircle:
                    case RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle:
                    case RoomEnemyProjectileKind.ShaktoolAttackBackCircle:
                    case RoomEnemyProjectileKind.SporeSpawnStalk:
                    case RoomEnemyProjectileKind.SporeSpawnSpore:
                    case RoomEnemyProjectileKind.SporeSpawnSpawner:
                    case RoomEnemyProjectileKind.SaveStationElectricity:
                    case RoomEnemyProjectileKind.DownwardGateMoving:
                    case RoomEnemyProjectileKind.DownwardGateClosed:
                    case RoomEnemyProjectileKind.NoobTubeCrack:
                    case RoomEnemyProjectileKind.NoobTubeShard:
                    case RoomEnemyProjectileKind.NoobTubeReleasedAirBubble:
                    default:
                        throw new InvalidDataException($"Movie projectile {projectile.Kind} needs an E/F mapping");
                }
                Check(owner + " collision option", EnemyProjectileCollisionOptionCodec.ToNative(projectile.CollisionOption), NativeSnapshotMemory.EnemyProjectileCollisionOption + index);
                if (projectile.Kind is RoomEnemyProjectileKind.EnemyDeathExplosion or RoomEnemyProjectileKind.EnemyDeathPickup)
                {
                    VerificationAssertions.AssertTrue(projectile.DropChancesOverride is null,
                        "Movie drop uses a direct chance-table override; map its native source before comparing");
                    // Death explosions consume their per-slot header on their later
                    // drop instruction. Direct F337 pickups consume it immediately via
                    // caller X ($86:EF3E/F118), not allocated-slot Y, and never read it
                    // again. Their retained per-slot word is not a live source identity.
                    if (projectile.Kind == RoomEnemyProjectileKind.EnemyDeathExplosion)
                        Check(owner + " source enemy header", SuperMetroid.AssetExtraction.Enemies.EnemyDefinitionIds.HeaderPointer(projectile.DropDefinitionId), NativeSnapshotMemory.EnemyProjectileEnemyHeader + index);
                    // The native word holds the killed slot's table offset, bit 15 for a respawn, $FFFF for none.
                    KilledEnemy killed = projectile.KilledEnemy;
                    Check(owner + " killed enemy index", killed.SlotIndex < 0 ? 0xffff
                        : SuperMetroid.AssetExtraction.Enemies.EnemySlotOffsets.OffsetOf(killed.SlotIndex) | (killed.Respawns ? 0x8000 : 0),
                        NativeSnapshotMemory.EnemyProjectileKilledEnemy + index);
                }
                ushort nativeDamage = (ushort)(W(NativeSnapshotMemory.EnemyProjectileProperties + index) & 0x0fff);
                if (projectile.Damage != nativeDamage) mismatches.Add(owner + $" damage: native={nativeDamage} port={projectile.Damage}");
            }
            if (mismatches.Count != 0)
            {
                RoomLevelData level = runtime.Room.LevelData ?? throw new InvalidDataException("Missing active room collision data.");
                Console.Error.WriteLine($"Pose history: port={(int)SamusPoseNumbering.Number(samus.PoseHistory.PreviousPose):X4}/{samus.PoseHistory.PreviousMovement}/{(int)SamusPoseNumbering.Number(samus.PoseHistory.LastDifferentPose):X4}/{samus.PoseHistory.LastDifferentMovement}, native={W(NativeSnapshotMemory.PreviousPose):X4}/{W(NativeSnapshotMemory.PreviousDirection):X4}/{W(NativeSnapshotMemory.LastDifferentPose):X4}/{W(NativeSnapshotMemory.LastDifferentDirection):X4}");
                Console.Error.WriteLine($"Liquid diagnostic: Y={samus.Position.Y.Whole:X4}, surface={samus.LiquidPhysics.LavaAcidYPosition:X4}, pose={(int)SamusPoseNumbering.Number(samus.Pose):X2}, radius={samus.Kinematics.Radius.Y}");
                Console.Error.WriteLine($"Shot diagnostic: locked={samus.Control.AlphaLocked}, HUD={samus.SelectedHudItem}, charge={runtime.Projectiles.Charge.FlareCounter}, cooldown={runtime.BombProjectiles.CooldownTimer.RawValue}, held={runtime.Controller1.Current:X}, new={runtime.Controller1.NewlyPressed:X}");
                Console.Error.WriteLine($"Room width={level.Size.X}, Samus radius={samus.Kinematics.Radius.X}/{samus.Kinematics.Radius.Y}, speed={samus.HorizontalSpeed.BaseSpeed.Whole:X4}.{samus.HorizontalSpeed.BaseSpeed.Fraction:X4}+{samus.HorizontalSpeed.ExtraRunSpeed.Whole:X4}.{samus.HorizontalSpeed.ExtraRunSpeed.Fraction:X4}");
                for (int block = 0; block < level.Size.X * level.Size.Y; block++)
                {
                    RoomTerrainBlock expectedBlock = RoomLevelWordCodec.Decode(W(NativeSnapshotMemory.Level + block * 2));
                    RoomTerrainBlock actualBlock = level.TerrainGrid[block];
                    if (expectedBlock != actualBlock) Console.Error.WriteLine($"Block {block} ({block % level.Size.X},{block / level.Size.X}): native={expectedBlock} port={actualBlock}");
                }
                throw new InvalidDataException($"Full movie first divergence at update {frame} (SMV source frame {(frame == 0 ? 0 : updates[frame - 1].GetProperty("sourceFrame").GetInt32())}): " + string.Join("; ", mismatches));
            }
            if (!deferLoadingOwners && pendingLoadedOwners is not null)
            {
                Console.WriteLine($"Completed-loading RNG/palette/enemy owners match after {loadingIntervals} IRQ intervals; movement/input checked throughout.");
                pendingLoadedOwners = null;
            }
            // This update is compared: the host may run the next one.
            yield return null;
            // Only the converted controller event enters production; reference memory is
            // read-only. An accepted input read during APU transfer is not another
            // gameplay update. Until its latch/counter effects are normalized, stop
            // explicitly instead of replaying hardware upload time as gameplay.
            if (frame < length)
            {
                if (updates[frame].GetProperty("timingClass").GetString() is
                    "apu-upload-continuation" or "apu-upload-tail-continuation")
                    throw new InvalidDataException($"SMV source frame {updates[frame].GetProperty("sourceFrame").GetInt32()} is an APU upload continuation; hardware-wait input normalization is not implemented.");
                previousBodyRecord = BodyRecord();
                yield return ControllerWordCodec.FromRaw((ushort)updates[frame].GetProperty("input").GetInt32(), $"movie update {frame}");
            }
        }
        VerificationAssertions.AssertTrue(pendingLoadedOwners is null, "no deferred loading comparison remains at movie end");
        VerificationAssertions.AssertTrue(replayTrace.Records.IsExhausted(), "trace ends after movie terminal frame");
        report = $"Samus body records: {pairedBodyDraws} paired visible updates, {phaseShiftedBodyDraws} hardware-phase-shifted updates, {retainedHiddenBodyRecords} hidden-record retention checks." +
            Environment.NewLine + $"Full movie input replay: {length} updates across all 10890 source frames match the currently instrumented fields.";
    }

    /// <inheritdoc/>
    public string Finish() => report ?? throw new InvalidOperationException("The Ridley replay has not finished.");

    /// <inheritdoc/>
    public Exception? NamedProductionFailure(Exception exception) => null;

    /// <inheritdoc/>
    /// <remarks>The frame sequence owns the trace it opens; ending or disposing the sequence closes it.</remarks>
    public void Dispose()
    {
    }
}

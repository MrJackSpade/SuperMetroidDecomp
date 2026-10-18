using SuperMetroid.Core.Input;
using System.Reflection;
using System.IO.Compression;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Checks automatic door recovery snapshots, restored continuation, and protection of manual save slots.</summary>
internal static class DoorTransitionAutosaveVerification
{
    /// <summary>Drives retail Ceres and Landing Site door transitions and checks save timing, atomic replacement, replay exclusion, and restored state.</summary>
    internal static void VerifyDoorTransitionAutosave()
    {
        AssertTrue(new SuperMetroidGameOptions().DoorTransitionAutosave, "new sessions enable door recovery");
        AssertTrue(SuperMetroidGameOptionsIni.Parse("[Game]\nInfiniteAmmo=false").DoorTransitionAutosave,
            "existing INIs without the key enable recovery");
        AssertTrue(SuperMetroidGameOptionsIni.Parse(SuperMetroidGameOptionsIni.DefaultFileContents).DoorTransitionAutosave,
            "shipped default enables recovery");
        AssertTrue(!SuperMetroidGameOptionsIni.Parse("[Game]\nDoorTransitionAutosave=false").DoorTransitionAutosave,
            "explicit disable is honored");
        AssertThrows<InvalidDataException>(() => SuperMetroidGameOptionsIni.Parse("[Game]\nDoorTransitionAutosave=maybe"), "invalid autosave value");
        AssertThrows<InvalidDataException>(() => SuperMetroidGameOptionsIni.Parse("[Game]\nDoorTransitionAutosave=true\nDoorTransitionAutosave=false"), "duplicate autosave key");

        string parent = Path.GetFullPath("csharp/test-temp");
        Directory.CreateDirectory(parent);
        string directory = Directory.CreateDirectory(Path.Combine(parent, "door-autosave-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(playerInvincibilityEnabled: true);
            runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus();
            var game = CreateRetailGameFixture(new SuperMetroid.Core.Saves.SaveSlotStore(), renderGameplayFrames: false);
            game.Session.runtime = runtime;
            var assets = RepositoryInstallation.Installation.LoadAudioClips();
            game.BindAudio(assets);
            var identity = GameContentIdentity.Create(new string('A', 64), new string('B', 64), new string('C', 64),
                typeof(SuperMetroidGame).Module.ModuleVersionId);
            var store = DebuggerSaveStateStore.ForInstalledGame(directory, null, identity);
            string storeDirectory = (Path.GetDirectoryName(store.GetSlotPath(0)) ?? throw new InvalidOperationException("Verification requires Path.GetDirectoryName(store.GetSlotPath(0))."));
            Directory.CreateDirectory((storeDirectory ?? throw new InvalidOperationException("Verification requires storeDirectory.")));
            for (int slot = 0; slot < DebuggerStateFormat.SlotCount; slot++)
                File.WriteAllText(store.GetSlotPath(slot), "manual slot " + slot);
            string autoPath = store.GetSlotPath(DebuggerStateFormat.AutomaticSlot);
            AssertEqual("SuperMetroid-debug-slot-auto.smstate", Path.GetFileName(autoPath), "diagnostic filename");
            AssertTrue(!store.TryLoad(DebuggerStateFormat.AutomaticSlot, out _), "missing auto is clearly empty");
            AssertThrows<FileNotFoundException>(() => store.Load(DebuggerStateFormat.AutomaticSlot), "missing auto load error");
            AssertThrows<ArgumentOutOfRangeException>(() => store.GetSlotPath(11), "no other implicit slots");

            PrepareDoor(0x92fd, 0x91f8);
            int writes = FinishDoor(enabled: true);
            AssertEqual(1, writes, "one autosave only at completed transition");
            AssertEqual((ushort)0x91f8, (game.GameplayActiveRoomPointer ?? throw new InvalidOperationException("Verification requires game.GameplayActiveRoomPointer.")), "autosave destination is Landing Site");
            byte[] first = File.ReadAllBytes(autoPath);
            string bundle = Path.Combine(directory, "auto-diagnostics.zip");
            SuperMetroid.Diagnostics.Android.AndroidDiagnosticBundle.Create(directory, bundle, DebuggerStateFormat.AutomaticSlot);
            using (var zip = ZipFile.OpenRead(bundle))
            {
                AssertTrue(zip.GetEntry("debug-states/SuperMetroid-debug-slot-auto.smstate") is not null,
                    "Android diagnostic export includes selected auto state");
                AssertTrue(!zip.Entries.Any(entry => entry.FullName.Contains("debug-slot-0.smstate", StringComparison.Ordinal)),
                    "auto export does not include unrelated manual states");
            }
            AssertTrue(SuperMetroid.Diagnostics.Android.AndroidFileImport.ImportState(directory, "unused", DebuggerStateFormat.AutomaticSlot)
                .Contains("reserved", StringComparison.Ordinal), "manual import cannot replace automatic recovery");
            var loaded = store.Load(DebuggerStateFormat.AutomaticSlot);
            AssertEqual(SuperMetroidGameState.MainGameplay, loaded.Game.GameState, "auto resumes after destination fade");
            AssertEqual(game.FrameNumber, loaded.Game.FrameNumber, "auto captures completed transition frame");
            InstalledFixtures.BindGame(loaded.Game);
            // The player is restored inside the game graph; the host rebinds only the catalog.
            loaded.Game.BindAudio(assets);
            var expectedPcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
            var actualPcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
            for (int frame = 0; frame < 6; frame++)
            {
                SnesButton input = frame < 3 ? SnesButton.Left : SnesButton.None;
                game.Step(input);
                game.CopyAudioFrame(expectedPcm);
                loaded.Game.Step(input);
                loaded.Game.CopyAudioFrame(actualPcm);
                AssertEqual(game.AudioAcknowledgementsForVerification, loaded.Game.AudioAcknowledgementsForVerification,
                    "restored continuation acknowledgements");
                AssertEqual(game.FrameNumber, loaded.Game.FrameNumber, "restored frame cadence");
                AssertSameBytes(bus.WorkRam, loaded.AddressSpace.WorkRam, "restored continuation WRAM");
                AssertSameBytes(SuperMetroid.Core.Saves.SaveRamCodec.Encode(game.Saves, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps),
                    SuperMetroid.Core.Saves.SaveRamCodec.Encode(loaded.Game.Saves, SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps), "restored continuation saves");
                AssertEqual(runtime.Player.IntroducedSamus.Position.X.Whole, loaded.Game.GameplayRuntimeForVerification.Player.IntroducedSamus.Position.X.Whole, "restored Samus movement");
                AssertEqual(runtime.System.RandomNumber, loaded.Game.GameplayRuntimeForVerification.System.RandomNumber, "restored RNG");
                AssertTrue(expectedPcm.SequenceEqual(actualPcm), "restored continuation PCM");
                AssertTrue(DoorTransitionAutosave.TrySave(true, false, SuperMetroidGameState.MainGameplay,
                    store, bus, game) is null, "ordinary gameplay does not replace auto");
            }
            AssertTrue(first.SequenceEqual(File.ReadAllBytes(autoPath)), "advancing gameplay preserves transition snapshot");
            AssertTrue(DoorTransitionAutosave.TrySave(true, true, SuperMetroidGameState.LoadingNextRoomB,
                store, bus, game) is null, "replay does not overwrite recovery");
            PrepareDoor(0x91f8, 0x92fd);
            AssertEqual(1, FinishDoor(enabled: true), "second transition writes one replacement");
            AssertEqual((ushort)0x92fd, (store.Load(DebuggerStateFormat.AutomaticSlot).Metadata.RoomPointer ?? throw new InvalidOperationException("Verification requires store.Load(DebuggerStateFormat.AutomaticSlot).Metadata.RoomPointer.")),
                "second auto points at new destination");
            byte[] second = File.ReadAllBytes(autoPath);
            AssertTrue(!first.SequenceEqual(second), "second transition replaces auto");
            PrepareDoor(0x92fd, 0x91f8);
            AssertEqual(0, FinishDoor(enabled: false), "disabled transition makes no autosave");
            AssertTrue(second.SequenceEqual(File.ReadAllBytes(autoPath)), "disabled policy preserves prior auto");
            // Keep the destination open without delete sharing: replacement must fail
            // after a complete temporary write and retain the last valid snapshot.
            using (var locked = new FileStream(autoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                AssertThrows<IOException>(() => store.Save(DebuggerStateFormat.AutomaticSlot, bus, game),
                    "failed atomic replacement is reported");
            AssertTrue(second.SequenceEqual(File.ReadAllBytes(autoPath)), "failed replacement preserves last valid auto");
            AssertEqual(0, Directory.GetFiles(storeDirectory, "*.tmp").Length, "failed replacement cleans temporary file");
            for (int slot = 0; slot < DebuggerStateFormat.SlotCount; slot++)
                AssertEqual("manual slot " + slot, File.ReadAllText(store.GetSlotPath(slot)), "manual slots untouched");
            File.WriteAllText(autoPath, "invalid state");
            AssertThrows<InvalidDataException>(() => store.Load(DebuggerStateFormat.AutomaticSlot), "malformed auto retains normal compatibility validation");
            Console.WriteLine("Door autosave: default/disabled INI, old options, real door completion, six-frame WRAM/SRAM/movement/RNG/PCM continuation, second replacement, replay exclusion, failed-write preservation and ten untouched manual slots pass.");

            int FinishDoor(bool enabled)
            {
                int count = 0;
                for (int frame = 0; frame < 500; frame++)
                {
                    var before = game.GameState;
                    game.Step(0);
                    var saved = DoorTransitionAutosave.TrySave(enabled, false, before, store, bus, game);
                    if (saved is not null)
                    {
                        count++;
                        AssertEqual(SuperMetroidGameState.LoadingNextRoomB, before, "capture comes from door phase");
                        AssertEqual(DoorTransitionPhase.Complete, game.DoorTransitionPhaseForVerification, "capture waits for complete fade");
                    }
                    if (game.GameState == SuperMetroidGameState.MainGameplay) return count;
                    if (!File.Exists(autoPath)) AssertEqual(0, count, "no save in incomplete destination");
                }
                throw new InvalidDataException("Autosave fixture door did not complete.");
            }
            void PrepareDoor(ushort source, ushort destination)
            {
                runtime.LoadCartridgeRoomForDebug(source);
                var level = runtime.Room.LoadedLevelData;
                for (int y = 0; y < level.Size.Y; y++)
                for (int x = 0; x < level.Size.X; x++)
                {
                    var block = level.GetCollisionBlock(new(checked((ushort)x), checked((ushort)y)));
                    if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
                    var door = level.ResolveDoorCollision(block.Bts.Value, SamusPoseId.FacingRightNormalPose, false);
                    if (door.Door?.DestinationRoomPointer != destination) continue;
                    runtime.LoadCartridgeRoomForDebug(source, camera: new Position16<RoomPixels>((ushort)(x / 16 * 256), (ushort)(y / 16 * 256)));
                    var samus = runtime.Player.IntroducedSamus;
                    samus.InputLocked = false;
                    samus.PoseId = SamusPoseId.FacingRightNormalPose;
                    samus.Position = samus.Position.WithWhole(new((ushort)(x * 16 + 8), (ushort)(y * 16 + 8)));
                    samus.PoseMetadata.RefreshCollisionRadii(); samus.Presentation.InitializeAnimation();
                    runtime.Room.LoadedLevelData.ResolveDoorCollision(block.Bts.Value, samus.Pose, true);
                    game.Session.lastAudioRoomStatePointer = runtime.Room.LoadedRoom.State.Pointer;
                    game.GameState = SuperMetroidGameState.HitDoorBlock;
                    return;
                }
                throw new InvalidDataException("Requested fixture door is absent.");
            }
        }
        finally
        {
            if (!Path.GetFullPath(directory).StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Autosave cleanup escaped fixture parent.");
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>Runs the door-transition autosave verification command.</summary>
    [VerificationCommand("--door-autosave")]
    internal static int DoorAutosaveCommand()
    {
     VerifyDoorTransitionAutosave(); return 0; 
    }
}

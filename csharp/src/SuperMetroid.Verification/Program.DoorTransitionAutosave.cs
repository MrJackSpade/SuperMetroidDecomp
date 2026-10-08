using System.Reflection;
using System.IO.Compression;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyDoorTransitionAutosave()
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
        var fields = typeof(SuperMetroidGameOptions).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .OrderBy(field => field.Name, StringComparer.Ordinal).ToArray();
        var oldFields = DebuggerStateFieldMigrations.WithoutIntroductions(typeof(SuperMetroidGameOptions), fields, "<DoorTransitionAutosave>k__BackingField");
        AssertTrue(!oldFields.Any(field => field.Name.Contains("DoorTransitionAutosave", StringComparison.Ordinal)), "legacy field order excludes new policy");
        var legacyOptions = (SuperMetroidGameOptions)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(SuperMetroidGameOptions));
        RestoreLegacy(legacyOptions, "<DoorTransitionAutosave>k__BackingField");
        AssertTrue(legacyOptions.DoorTransitionAutosave, "legacy graphs get default enabled policy");

        string parent = Path.GetFullPath("csharp/test-temp");
        Directory.CreateDirectory(parent);
        string directory = Directory.CreateDirectory(Path.Combine(parent, "door-autosave-" + Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
            var runtime = CreateRetailRuntimeFixture(bus, playerInvincibilityEnabled: true);
            runtime.InitializeHud(HudSnapshot.CeresDebug);
            runtime.InitializeStartingCeresRoom();
            runtime.InitializeCeresStartSamus();
            var game = CreateRetailGameFixture(bus, renderGameplayFrames: false);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(SuperMetroidGame).GetField("runtime", flags)!.SetValue(game, runtime);
            var assets = RepositoryInstallation.Installation.LoadAudio();
            var audio = new CartridgeAudioRenderer(assets);
            var identity = GameContentIdentity.Create(new string('A', 64), new string('B', 64), new string('C', 64),
                typeof(SuperMetroidGame).Module.ModuleVersionId);
            var store = DebuggerSaveStateStore.ForInstalledGame(directory, null, identity);
            string storeDirectory = Path.GetDirectoryName(store.GetSlotPath(0))!;
            Directory.CreateDirectory(storeDirectory);
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
            AssertEqual((ushort)0x91f8, game.GameplayActiveRoomPointer!.Value, "autosave destination is Landing Site");
            byte[] first = File.ReadAllBytes(autoPath);
            string bundle = Path.Combine(directory, "auto-diagnostics.zip");
            SuperMetroid.Android.AndroidDiagnosticBundle.Create(directory, bundle, DebuggerStateFormat.AutomaticSlot);
            using (var zip = ZipFile.OpenRead(bundle))
            {
                AssertTrue(zip.GetEntry("debug-states/SuperMetroid-debug-slot-auto.smstate") is not null,
                    "Android diagnostic export includes selected auto state");
                AssertTrue(!zip.Entries.Any(entry => entry.FullName.Contains("debug-slot-0.smstate", StringComparison.Ordinal)),
                    "auto export does not include unrelated manual states");
            }
            AssertTrue(SuperMetroid.Android.AndroidFileImport.ImportState(directory, "unused", DebuggerStateFormat.AutomaticSlot)
                .Contains("reserved", StringComparison.Ordinal), "manual import cannot replace automatic recovery");
            var loaded = store.Load(DebuggerStateFormat.AutomaticSlot);
            AssertEqual(SuperMetroidGameState.MainGameplay, loaded.Game.GameState, "auto resumes after destination fade");
            AssertEqual(game.FrameNumber, loaded.Game.FrameNumber, "auto captures completed transition frame");
            RepositoryInstallation.BindGame(loaded.Game);
            var restoredAudio = new CartridgeAudioRenderer(assets, loaded.AudioPlayer);
            for (int frame = 0; frame < 6; frame++)
            {
                ushort input = frame < 3 ? (ushort)0x0200 : (ushort)0;
                var expected = game.Step(input);
                var expectedPcm = audio.RenderFrame(expected.AudioCommands);
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                var actual = loaded.Game.Step(input);
                var actualPcm = restoredAudio.RenderFrame(actual.AudioCommands);
                loaded.Game.SetAudioAcknowledgements(restoredAudio.ReadAcknowledgements());
                AssertEqual(game.FrameNumber, loaded.Game.FrameNumber, "restored frame cadence");
                AssertSameBytes(bus.WorkRam, loaded.AddressSpace.WorkRam, "restored continuation WRAM");
                AssertSameBytes(bus.SaveRam, loaded.AddressSpace.SaveRam, "restored continuation SRAM");
                AssertEqual(runtime.Samus!.XPosition, loaded.Game.RuntimeForVerification!.Samus!.XPosition, "restored Samus movement");
                AssertEqual(runtime.System.RandomNumber, loaded.Game.RuntimeForVerification!.System.RandomNumber, "restored RNG");
                AssertTrue(expectedPcm.AsSpan().SequenceEqual(actualPcm), "restored continuation PCM");
                AssertTrue(DoorTransitionAutosave.TrySave(true, false, SuperMetroidGameState.MainGameplay,
                    store, bus, game, audio.Player) is null, "ordinary gameplay does not replace auto");
            }
            AssertTrue(first.AsSpan().SequenceEqual(File.ReadAllBytes(autoPath)), "advancing gameplay preserves transition snapshot");
            AssertTrue(DoorTransitionAutosave.TrySave(true, true, SuperMetroidGameState.LoadingNextRoomB,
                store, bus, game, audio.Player) is null, "replay does not overwrite recovery");
            PrepareDoor(0x91f8, 0x92fd);
            AssertEqual(1, FinishDoor(enabled: true), "second transition writes one replacement");
            AssertEqual((ushort)0x92fd, store.Load(DebuggerStateFormat.AutomaticSlot).Metadata.RoomPointer!.Value,
                "second auto points at new destination");
            byte[] second = File.ReadAllBytes(autoPath);
            AssertTrue(!first.AsSpan().SequenceEqual(second), "second transition replaces auto");
            PrepareDoor(0x92fd, 0x91f8);
            AssertEqual(0, FinishDoor(enabled: false), "disabled transition makes no autosave");
            AssertTrue(second.AsSpan().SequenceEqual(File.ReadAllBytes(autoPath)), "disabled policy preserves prior auto");
            // Keep the destination open without delete sharing: replacement must fail
            // after a complete temporary write and retain the last valid snapshot.
            using (var locked = new FileStream(autoPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                AssertThrows<IOException>(() => store.Save(DebuggerStateFormat.AutomaticSlot, bus, game, audio.Player),
                    "failed atomic replacement is reported");
            AssertTrue(second.AsSpan().SequenceEqual(File.ReadAllBytes(autoPath)), "failed replacement preserves last valid auto");
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
                    var output = game.Step(0);
                    audio.RenderFrame(output.AudioCommands);
                    game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                    var saved = DoorTransitionAutosave.TrySave(enabled, false, before, store, bus, game, audio.Player);
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
                var level = runtime.LevelData!;
                for (int y = 0; y < level.HeightInBlocks; y++)
                for (int x = 0; x < level.WidthInBlocks; x++)
                {
                    var block = level.GetCollisionBlock(x, y);
                    if (block.CollisionType != RoomCollisionType.DoorBlock) continue;
                    var door = level.ResolveDoorCollision(bus, block.Behavior, 1, false);
                    if (door.DestinationRoomPointer != destination) continue;
                    runtime.LoadCartridgeRoomForDebug(source, cameraX: (ushort)(x / 16 * 256), cameraY: (ushort)(y / 16 * 256));
                    var samus = runtime.Samus!;
                    samus.InputLocked = false;
                    samus.PoseId = SamusPoseId.FacingRightNormalPose;
                    samus.XPosition = (ushort)(x * 16 + 8); samus.YPosition = (ushort)(y * 16 + 8);
                    samus.RefreshCollisionRadii(bus); samus.InitializeAnimation(bus);
                    runtime.LevelData!.ResolveDoorCollision(bus, block.Behavior, samus.Pose, true);
                    typeof(SuperMetroidGame).GetField("lastAudioRoomStatePointer", flags)!.SetValue(game, runtime.ActiveRoom!.State.Pointer);
                    typeof(SuperMetroidGame).GetProperty(nameof(SuperMetroidGame.GameState))!.SetValue(game, SuperMetroidGameState.HitDoorBlock);
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
}

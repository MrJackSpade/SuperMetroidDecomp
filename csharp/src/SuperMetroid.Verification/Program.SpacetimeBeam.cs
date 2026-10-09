using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{
    /// <summary>
    /// Replays the two alpha passes measured by the native Pit Room probe. Expected
    /// pointers, hashes and the saved dispatcher word come from the retail callback at
    /// $90:AD16, not from another C# implementation.
    /// </summary>
    private static void VerifySpacetimeBeam()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SpacetimeBeamCopyDefinitions.LastSourceAddress - SpacetimeBeamCopyDefinitions.FirstSourceAddress + 1,
            SpacetimeBeamCopyDefinitions.SourceBytes.Length, "bounded native copy definition size");
        for (int offset = 0; offset < SpacetimeBeamCopyDefinitions.SourceBytes.Length; offset++)
            AssertEqual(bus.ReadCartridgeByte(SpacetimeBeamCopyDefinitions.FirstSourceAddress + offset),
                SpacetimeBeamCopyDefinitions.ReadCartridgeByte(SpacetimeBeamCopyDefinitions.FirstSourceAddress + offset),
                "SpaceTime copy source agrees with supported cartridge");
        AssertThrows<InvalidOperationException>(() => SpacetimeBeamCopyDefinitions.ReadCartridgeByte(0x009130),
            "adjacent instruction before bounded source is not admitted");
        AssertThrows<InvalidOperationException>(() => SpacetimeBeamCopyDefinitions.ReadCartridgeByte(0x010000),
            "word carry must return to the mutable-memory owner");
        var gameplayBus = new SpacetimeMutableOnlyBus(bus);
        var level = new RoomLevelData(
            16,
            16,
            new ushort[256],
            new byte[256],
            new ushort[256],
            new byte[8]);

        // Exercise the production pause input that creates Charge + Ice + Spazer + Plasma
        // without Wave; accepting the raw word only in the projectile test would miss the
        // technique's actual player-accessible setup.
        var samus = new SamusState
        {
            CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            CollectedBeams = 0x100f,
            EquippedBeams = 0x1006,
        };
        var selection = CreateRetailPauseFixture(
            bus,
            samus,
            new Bank80SystemState(),
            AreaId.Crateria,
            0,
            0);
        EnterPauseEquipment(selection);
        SelectPauseBoots(selection);
        selection.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x100e, samus.EquippedBeams,
            "same-frame Boots Left+A creates the SpaceTime beam word");

        Suite(nameof(VerifySpacetimeBeamGraphics), () => VerifySpacetimeBeamGraphics());

        samus.Pose = 1;
        samus.XPosition = 128;
        samus.YPosition = 128;
        samus.SelectedHudItem = 0;
        samus.Missiles = 20;
        samus.MaxMissiles = 50;
        samus.SuperMissiles = 7;
        samus.MaxSuperMissiles = 10;
        samus.PowerBombs = 3;
        samus.MaxPowerBombs = 5;

        var system = new Bank80SystemState();
        byte[] a5Events = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.EventByteCount).ToArray();
        byte[] a5Bosses = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.AreaCount).ToArray();
        byte[] a5Chozo = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.RoomChozoBitByteCount).ToArray();
        byte[] a5Items = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.ItemBitByteCount).ToArray();
        byte[] a5Doors = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.DoorBitByteCount).ToArray();
        byte[] a5Stations = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.UsedSaveStationByteCount).ToArray();
        byte[] a5Maps = Enumerable.Repeat((byte)0xa5,
            Bank80SystemState.MapStationByteCount).ToArray();
        system.LoadEventBytes(a5Events);
        system.LoadBossBytes(a5Bosses);
        system.LoadRoomChozoBytes(a5Chozo);
        system.LoadCollectedItemBytes(a5Items);
        system.LoadOpenedDoorBytes(a5Doors);
        system.LoadUsedSaveStationBytes(a5Stations);
        system.LoadMapStationBytes(a5Maps);
        system.LoadSavedLoadingGameState(SaveLoadingGameStates.OpeningCinematic);

        var shared = CreateBombFixture();
        var projectiles = CreateProjectileFixture();
        system.WritePersistentMirror(bus);
        for (int index = 0; index < SaveRamLayout.ProgressionPaddingByteCount; index++)
        {
            bus.WriteByte(
                SaveRamLayout.ProgressionPaddingWramAddress + index,
                0xa5);
        }
        SamusProjectileSlotObservation slotsBeforeHeld = projectiles.ObserveSlots();
        SamusProjectileFrameResult held = projectiles.StepFrame(
            gameplayBus,
            level,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            shared);
        AssertEqual((int?)0, slotsBeforeHeld.FiredSlot(projectiles),
            "SpaceTime initial shot allocation");
        AssertTrue(!held.PersistentMemoryCorrupted,
            "initial inherited Y=$0038 does not enter the palette-copy loop");
        AssertEqual((ushort)0x9137, projectiles.Slots[0].InstructionPointer,
            "native frame-zero next animation record");
        AssertEqual(0x8e3eec21u, HashWram(bus),
            "native frame-zero progression hash remains the A5 baseline");

        system.WritePersistentMirror(bus);
        SamusProjectileSlotObservation slotsBeforeRelease = projectiles.ObserveSlots();
        SamusProjectileFrameResult released = projectiles.StepFrame(
            gameplayBus,
            level,
            samus,
            0,
            0,
            0,
            0,
            shared);
        AssertEqual((int?)1, slotsBeforeRelease.FiredSlot(projectiles),
            "Charge release allocates the second uncharged SpaceTime shot");
        AssertTrue(released.PersistentMemoryCorrupted,
            "prior shot's inherited Y=$912F crosses the persistent mirror");
        system.LoadPersistentMirror(bus);
        AssertEqual(0x2c169a4au, HashWram(bus),
            "retail frame-one progression hash");
        AssertEqual((ushort)0x0880, system.SavedLoadingGameState,
            "retail frame-one loading_game_state corruption");
        AssertEqual((ushort)0x100e, samus.EquippedBeams,
            "SpaceTime corruption does not rewrite live equipment");

        var saveRam = new SuperMetroidSaveRam(bus, RetailPresentationFixture());
        saveRam.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(
            samus,
            system,
            area: (ushort)AreaId.Crateria,
            saveStation: 7));
        SuperMetroidSaveSlot saved = saveRam.ReadSlot(0)
            ?? throw new InvalidOperationException("SpaceTime save did not survive checksum validation.");
        AssertEqual((ushort)0x100e, saved.EquippedBeams,
            "save retains invalid SpaceTime equipment");
        AssertEqual((ushort)0x0880, saved.LoadingGameState,
            "save retains corrupted startup dispatcher word");
        AssertTrue(saved.EventBytes.AsSpan().SequenceEqual(ReadWram(
                bus, SaveRamLayout.EventsWramAddress, saved.EventBytes.Length)),
            "save retains corrupted event bytes");
        AssertTrue(saved.BossBytes.AsSpan().SequenceEqual(ReadWram(
                bus, SaveRamLayout.BossBitsWramAddress, saved.BossBytes.Length)),
            "save retains corrupted boss bytes");
        AssertTrue(saved.CollectedItemBytes.AsSpan().SequenceEqual(ReadWram(
                bus, SaveRamLayout.CollectedItemBitsWramAddress,
                saved.CollectedItemBytes.Length)),
            "save retains corrupted item bytes");
        AssertTrue(saved.OpenedDoorBytes.AsSpan().SequenceEqual(ReadWram(
                bus, SaveRamLayout.OpenedDoorBitsWramAddress,
                saved.OpenedDoorBytes.Length)),
            "save retains corrupted door bytes");

        var restarted = new SamusState();
        SuperMetroidGame.RestoreSpacetimeRestartInventory(restarted, saved);
        AssertEqual((ushort)0x100e, restarted.EquippedBeams,
            "Ceres restart retains the SpaceTime equipment word");
        AssertEqual((ushort)0, restarted.Missiles,
            "intro flashback removes current missiles");
        AssertEqual((ushort)0, restarted.MaxMissiles,
            "intro flashback removes missile capacity");
        AssertEqual(saved.SuperMissiles, restarted.SuperMissiles,
            "intro flashback preserves Super Missiles");
        AssertEqual(saved.PowerBombs, restarted.PowerBombs,
            "intro flashback preserves Power Bombs");

        Suite(nameof(VerifySpacetimeSaveRestartFrontend), () => VerifySpacetimeSaveRestartFrontend());

        Console.WriteLine(
            "SpaceTime Beam: pause setup, $90:AD16 WRAM copy, progression corruption, " +
            "equipment retention and SRAM persistence match the retail probe.");
    }

    /// <summary>Computes the retail probe's FNV-1a checksum over the progression mirror in WRAM.</summary>
    /// <param name="bus">Address space containing the live progression bytes.</param>
    /// <returns>The 32-bit checksum used to compare the two measured beam-update states.</returns>
    private static uint HashWram(SuperMetroidAddressSpace bus)
    {
        uint hash = 2166136261;
        for (int address = SaveRamLayout.EventsWramAddress;
            address < SaveRamLayout.MapStationsWramAddress + Bank80SystemState.MapStationByteCount;
            address++)
        {
            hash ^= bus.ReadByte(address);
            hash *= 16777619;
        }
        return hash;
    }

    /// <summary>Copies a contiguous range of live WRAM bytes for comparison with the saved progression snapshot.</summary>
    /// <param name="bus">Address space containing the requested memory range.</param>
    /// <param name="address">Starting WRAM address.</param>
    /// <param name="length">Number of consecutive bytes to copy.</param>
    /// <returns>A new array containing the bytes observed at the requested addresses.</returns>
    private static byte[] ReadWram(SuperMetroidAddressSpace bus, int address, int length)
    {
        var bytes = new byte[length];
        for (int index = 0; index < length; index++)
            bytes[index] = bus.ReadByte(address + index);
        return bytes;
    }

    /// <summary>Checks that SpaceTime-corrupted save data dispatches to the opening cinematic or Ceres restart as configured.</summary>
    private static void VerifySpacetimeSaveRestartFrontend()
    {
        SuperMetroidSaveSnapshot CreateResetSnapshot()
        {
            var snapshot = new SuperMetroidSaveSnapshot
            {
                Area = (ushort)AreaId.Crateria,
                SaveStation = 7,
                Health = 399,
                MaxHealth = 499,
                Missiles = 20,
                MaxMissiles = 50,
                SuperMissiles = 7,
                MaxSuperMissiles = 10,
                PowerBombs = 3,
                MaxPowerBombs = 5,
                EquippedBeams = 0x100e,
                CollectedBeams = 0x100f,
                EquippedItems = (ushort)SamusEquipmentFlags.GravitySuit,
                CollectedItems = (ushort)SamusEquipmentFlags.GravitySuit,
                LoadingGameState = SaveLoadingGameStates.OpeningCinematic,
            };
            Array.Fill(snapshot.EventBytes, byte.MaxValue);
            return snapshot;
        }

        SuperMetroidGame CreateGame(bool skipOpening)
        {
            var restartBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            var restartSaves = new SuperMetroidSaveRam(restartBus, RetailPresentationFixture());
            restartSaves.SaveSlot(0, CreateResetSnapshot());
            restartSaves.SelectSlot(0);
            var game = CreateRetailGameFixture(
                restartBus,
                new SuperMetroidGameOptions { SkipOpeningCinematic = skipOpening },
                renderGameplayFrames: false);
            game.BindMapPresentation(RetailPresentationFixture());
            return game;
        }

        static void ConfirmSelectedSave(SuperMetroidGame game)
        {
            FrontendFrame frame = game.Step(0);
            frame = game.Step((ushort)SnesButton.Start);
            Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen),
                () => frame = game.Step(0), 150, "SpaceTime title setup");
            frame = game.Step((ushort)SnesButton.Start);
            Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus,
                () => frame = game.Step(0), 150, "SpaceTime file menu");
            for (int frameIndex = 0; frameIndex < 16; frameIndex++)
                frame = game.Step(0);
            frame = game.Step((ushort)SnesButton.A);
            Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu,
                () => frame = game.Step(0), 200, "SpaceTime options menu");
            for (int frameIndex = 0; frameIndex < 16; frameIndex++)
                frame = game.Step(0);
            game.Step((ushort)SnesButton.A);
        }

        var cinematicGame = CreateGame(skipOpening: false);
        ConfirmSelectedSave(cinematicGame);
        Until(() => cinematicGame.GameState == SuperMetroidGameState.IntroCinematic,
            () => cinematicGame.Step(0), 200, "SpaceTime opening-cinematic dispatch");

        var skippedGame = CreateGame(skipOpening: true);
        ConfirmSelectedSave(skippedGame);
        Until(() => skippedGame.RuntimeForVerification is not null,
            () => skippedGame.Step(0), 200, "SpaceTime Ceres restart");
        SuperMetroidRuntime runtime = skippedGame.RuntimeForVerification!;
        AssertEqual(AreaId.Ceres, runtime.ActiveRoom!.AreaIndex,
            "SpaceTime save restarts at Ceres");
        AssertEqual((ushort)0x100e, runtime.Samus!.EquippedBeams,
            "SpaceTime Ceres restart retains equipped beams");
        AssertEqual((ushort)0, runtime.Samus.MaxMissiles,
            "SpaceTime Ceres restart loses Missiles after the intro flashback");
        AssertEqual((ushort)7, runtime.Samus.SuperMissiles,
            "SpaceTime Ceres restart retains Super Missiles");
        AssertEqual((ushort)3, runtime.Samus.PowerBombs,
            "SpaceTime Ceres restart retains Power Bombs");
        AssertTrue(Enumerable.Range(0, Bank80SystemState.EventByteCount).All(
                index => runtime.System.GetEventByteRaw(index) == 0),
            "SpaceTime Ceres restart begins with fresh progression events");

        static void Until(
            Func<bool> predicate,
            Action step,
            int limit,
            string context)
        {
            for (int tick = 0; tick < limit && !predicate(); tick++)
                step();
            AssertTrue(predicate(), context);
        }
    }
    /// <summary>Verifies SpaceTime beam tile uploads and confirms its palette is read from current mutable memory.</summary>
    private static void VerifySpacetimeBeamGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var gameplayBus = new SpacetimeMutableOnlyBus(bus);
        var artwork = RepositoryInstallation.Projectiles.BeamTiles;
        AssertEqual(SpacetimeBeamGraphicsDefinitions.TileSource & 0xffff,
            bus.ReadCartridgeByte(0x90c3cd) | bus.ReadCartridgeByte(0x90c3ce) << 8,
            "SpaceTime tile pointer comes from adjacent native palette table");
        AssertEqual(SpacetimeBeamGraphicsDefinitions.PalettePointer,
            bus.ReadCartridgeByte(0x90c3e5) | bus.ReadCartridgeByte(0x90c3e6) << 8,
            "SpaceTime palette pointer comes from the native color word");
        AssertTrue(artwork.TryResolve(SpacetimeBeamGraphicsDefinitions.TileSource, 256, out _),
            "legacy SpaceTime graphics queue resolves through installed artwork");
        foreach (bool queued in new[] { false, true })
        {
            // The palette aliases current enemy-projectile WRAM; changing it must be observable.
            for (int i = 0; i < 32; i++) bus.WriteByte(0x7e19ff + i, (byte)(i * 7 + (queued ? 19 : 5)));
            var vram = new SnesVram(); var cgram = new SnesCgram();
            if (queued)
            {
                var writes = new VramWriteQueue();
                SamusProjectileSystem.QueueBeamTilesAndLoadPalette(gameplayBus, writes, cgram, 0x100e, artwork);
                AssertEqual(1, writes.Entries.Count, "unpausing SpaceTime queues one beam upload");
                writes.DrainTo(vram, gameplayBus, artwork);
            }
            else SamusProjectileSystem.LoadBeamTilesAndPalette(gameplayBus, vram, cgram, 0x100e, artwork);
            for (int i = 0; i < 256; i++) AssertEqual(bus.ReadCartridgeByte(0x9ac401 + i),
                vram.ReadByte(0xc600 + i), "SpaceTime native tile bytes");
            for (int i = 0; i < 16; i++) AssertEqual((bus.ReadByte(0x7e19ff + i * 2) |
                bus.ReadByte(0x7e1a00 + i * 2) << 8) & 0x7fff, cgram.Colors[0xe0 + i],
                "SpaceTime colors retain current mutable memory rather than a frozen palette");
        }
    }
    // No import/cartridge capability is available while the production projectile path executes.
    /// <summary>Exposes mutable memory and CPU peripherals while withholding cartridge-import access from projectile execution.</summary>
    /// <param name="source">Retail-backed address space that supplies the allowed mutable-memory and peripheral operations.</param>
    private sealed class SpacetimeMutableOnlyBus(SuperMetroidAddressSpace source) :
        ISnesAddressSpace, ISnesMutableMemory, ISnesCpuPeripheralSource
    {
        /// <summary>Forwards reads from the wrapped SNES work-RAM implementation.</summary>
        /// <param name="address">CPU address in work RAM.</param>
        /// <returns>The byte currently stored at that address.</returns>
        public byte ReadWorkRamByte(int address) => ((ISnesMutableMemory)source).ReadWorkRamByte(address);

        /// <summary>Forwards reads from the wrapped SNES save-RAM implementation.</summary>
        /// <param name="address">CPU address in save RAM.</param>
        /// <returns>The byte currently stored at that address.</returns>
        public byte ReadSaveRamByte(int address) => ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        /// <summary>Forwards CPU peripheral reads to the wrapped address space.</summary>
        /// <param name="address">CPU peripheral address to read.</param>
        /// <returns>The peripheral byte supplied by the wrapped source.</returns>
        public byte ReadPeripheralByte(int address) => ((ISnesCpuPeripheralSource)source).ReadPeripheralByte(address);

        /// <summary>Forwards mutable-memory writes to the wrapped address space.</summary>
        /// <param name="address">Address to update.</param>
        /// <param name="value">Byte to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

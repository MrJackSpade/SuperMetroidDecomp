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
        var bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
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
        };
        var selection = new PauseMenuState(
            bus,
            samus,
            new Bank80SystemState(),
            AreaId.Crateria,
            0,
            0);
        samus.CollectedBeams = 0x100f;
        samus.EquippedBeams = 0x1006;
        selection.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int frame = 0; frame < 32; frame++)
            selection.Step(0, 0);
        selection.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x100e, samus.EquippedBeams,
            "same-frame Boots Left+A creates the SpaceTime beam word");

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

        var shared = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem();
        system.WritePersistentMirror(bus);
        for (int index = 0; index < SaveRamLayout.ProgressionPaddingByteCount; index++)
        {
            bus.WriteByte(
                SaveRamLayout.ProgressionPaddingWramAddress + index,
                0xa5);
        }
        SamusProjectileFrameResult held = projectiles.StepFrame(
            bus,
            level,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            shared);
        AssertEqual((int?)0, held.FiredSlot, "SpaceTime initial shot allocation");
        AssertTrue(!held.PersistentMemoryCorrupted,
            "initial inherited Y=$0038 does not enter the palette-copy loop");
        AssertEqual((ushort)0x9137, projectiles.Slots[0].InstructionPointer,
            "native frame-zero next animation record");
        AssertEqual(0x8e3eec21u, HashWram(bus),
            "native frame-zero progression hash remains the A5 baseline");

        system.WritePersistentMirror(bus);
        SamusProjectileFrameResult released = projectiles.StepFrame(
            bus,
            level,
            samus,
            0,
            0,
            0,
            0,
            shared);
        AssertEqual((int?)1, released.FiredSlot,
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

        var saveRam = new SuperMetroidSaveRam(bus);
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

        VerifySpacetimeSaveRestartFrontend();

        Console.WriteLine(
            "SpaceTime Beam: pause setup, $90:AD16 WRAM copy, progression corruption, " +
            "equipment retention and SRAM persistence match the retail probe.");
    }

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

    private static byte[] ReadWram(SuperMetroidAddressSpace bus, int address, int length)
    {
        var bytes = new byte[length];
        for (int index = 0; index < length; index++)
            bytes[index] = bus.ReadByte(address + index);
        return bytes;
    }

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
            var restartBus = SuperMetroidAddressSpace.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            var restartSaves = new SuperMetroidSaveRam(restartBus);
            restartSaves.SaveSlot(0, CreateResetSnapshot());
            restartSaves.SelectSlot(0);
            return new SuperMetroidGame(
                restartBus,
                new SuperMetroidGameOptions { SkipOpeningCinematic = skipOpening },
                renderGameplayFrames: false);
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
}

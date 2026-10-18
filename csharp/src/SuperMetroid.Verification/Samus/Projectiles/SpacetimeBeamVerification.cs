using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.Pause.PauseEquipmentRulesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;

/// <summary>Confirms the SpaceTime beam's saved progression corruption and graphics against bounded retail probes.</summary>
internal static class SpacetimeBeamVerification
{
    /// <summary>
    /// Replays the two alpha passes measured by the native Pit Room probe. Expected
    /// pointers, hashes and the saved dispatcher word come from the retail callback at
    /// $90:AD16, not from another C# implementation.
    /// </summary>
    internal static void VerifySpacetimeBeam()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        VerifySpacetimeCopySourceAgainstRetailRom(bus);
        AssertThrows<ArgumentOutOfRangeException>(() => InstalledFixtures.SamusOverreads.SpacetimeCopySourceByte(-1),
            "adjacent instruction before bounded source is not admitted");
        AssertThrows<ArgumentOutOfRangeException>(
            () => InstalledFixtures.SamusOverreads.SpacetimeCopySourceByte(SamusOverreadDefinitions.SpacetimeCopySourceByteCount),
            "word carry must return to the mutable-memory owner");
        var gameplayBus = NewSpacetimeMutableOnlyBus(bus);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new ushort[256], new byte[256], new ushort[256]), new byte[8]);

        // Exercise the production pause input that creates Charge + Ice + Spazer + Plasma
        // without Wave; accepting the raw word only in the projectile test would miss the
        // technique's actual player-accessible setup.
        var samus = new SamusState
        {
            CollectedItems = SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = SamusEquipmentFlags.HiJumpBoots,
            CollectedBeams = (SamusBeamFlags)0x100f,
            EquippedBeams = (SamusBeamFlags)0x1006,
        };
        var selection = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0) });
        EnterPauseEquipment(selection);
        SelectPauseBoots(selection);
        selection.Step(0, (SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x100e, samus.EquippedBeams.ToNativeWord(),
            "same-frame Boots Left+A creates the SpaceTime beam word");

        Suite(nameof(VerifySpacetimeBeamGraphics), () => VerifySpacetimeBeamGraphics());

        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.Position = samus.Position.WithWhole(new(128, 128));
        samus.SelectedHudItem = HudItem.Beam;
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
        projectiles.Overreads = InstalledFixtures.SamusOverreads;
        var mirror = new SaveMirrorImage();
        system.WritePersistentMirror(mirror);
        SamusProjectileSlotObservation slotsBeforeHeld = projectiles.ObserveSlots();
        SamusProjectileFrameResult held = projectiles.StepFrame(new SamusTerrain { Level = level, ProgressionMirror = mirror }, samus, new ProjectileButtonInput { ControllerInput = SnesButton.X, ControllerNewInput = SnesButton.X }, new(0, 0), shared);
        AssertEqual((int?)0, slotsBeforeHeld.FiredSlot(projectiles),
            "SpaceTime initial shot allocation");
        AssertTrue(!held.PersistentMemoryCorrupted,
            "initial inherited Y=$0038 does not enter the palette-copy loop");
        AssertEqual((ushort)0x9137, projectiles.Slots[0].InstructionPointer,
            "native frame-zero next animation record");
        AssertEqual(0xa11546b9u, HashProgression(mirror),
            "native frame-zero progression hash");

        system.WritePersistentMirror(mirror);
        SamusProjectileSlotObservation slotsBeforeRelease = projectiles.ObserveSlots();
        SamusProjectileFrameResult released = projectiles.StepFrame(new SamusTerrain { Level = level, ProgressionMirror = mirror }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), shared);
        AssertEqual((int?)1, slotsBeforeRelease.FiredSlot(projectiles),
            "Charge release allocates the second uncharged SpaceTime shot");
        AssertTrue(released.PersistentMemoryCorrupted,
            "prior shot's inherited Y=$912F crosses the persistent mirror");
        system.LoadPersistentMirror(mirror);
        AssertEqual(0x601b9b2cu, HashProgression(mirror),
            "retail frame-one progression hash");
        AssertEqual((ushort)0x0880, system.SavedLoadingGameState,
            "retail frame-one loading_game_state corruption");
        AssertEqual((ushort)0x100e, samus.EquippedBeams.ToNativeWord(),
            "SpaceTime corruption does not rewrite live equipment");

        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        saveRam.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Area = AreaId.Crateria, SaveStation = 7 }));
        SuperMetroidSaveSlot saved = saveRam.ReadSlot(0)
            ?? throw new InvalidOperationException("SpaceTime save did not survive checksum validation.");
        AssertEqual((ushort)0x100e, saved.EquippedBeams.ToNativeWord(),
            "save retains invalid SpaceTime equipment");
        AssertEqual((ushort)0x0880, saved.LoadingGameState,
            "save retains corrupted startup dispatcher word");
        AssertTrue(saved.EventBytes.SequenceEqual(mirror.Events.ToArray()),
            "save retains corrupted event bytes");
        AssertTrue(saved.BossBytes.SequenceEqual(mirror.BossBits.ToArray()),
            "save retains corrupted boss bytes");
        AssertTrue(saved.CollectedItemBytes.SequenceEqual(mirror.CollectedItemBits.ToArray()),
            "save retains corrupted item bytes");
        AssertTrue(saved.OpenedDoorBytes.SequenceEqual(mirror.OpenedDoorBits.ToArray()),
            "save retains corrupted door bytes");

        var restarted = new SamusState();
        FrontendSession.RestoreSpacetimeRestartInventory(restarted, saved);
        AssertEqual((ushort)0x100e, restarted.EquippedBeams.ToNativeWord(),
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

    /// <summary>Checks each installed copy-source byte against the supported ROM before replaying the beam.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the installed SpaceTime copy source")]
    private static void VerifySpacetimeCopySourceAgainstRetailRom(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        AssertEqual(SpacetimeBeamCopyDefinitions.LastSourceAddress - SpacetimeBeamCopyDefinitions.FirstSourceAddress + 1,
            SamusOverreadDefinitions.SpacetimeCopySourceByteCount, "bounded native copy source size");
        for (int offset = 0; offset < SamusOverreadDefinitions.SpacetimeCopySourceByteCount; offset++)
            AssertEqual(bus.ReadCartridgeByte(SpacetimeBeamCopyDefinitions.FirstSourceAddress + offset),
                InstalledFixtures.SamusOverreads.SpacetimeCopySourceByte(offset),
                "installed SpaceTime copy source agrees with supported cartridge");
    }

    /// <summary>
    /// FNV-1a hash of the owned progression bytes from the event bitset through the map stations, in native mirror
    /// order. The retail probe's hash also covered the eight unowned bytes after the door bits, which no game logic reads.
    /// </summary>
    private static uint HashProgression(SaveMirrorImage mirror)
    {
        uint hash = 2166136261;
        foreach (byte value in (byte[])[.. mirror.Events, .. mirror.BossBits, .. mirror.RoomChozoBits,
            .. mirror.CollectedItemBits, .. mirror.OpenedDoorBits, .. mirror.UsedSaveStations, .. mirror.MapStations])
        {
            hash ^= value;
            hash *= 16777619;
        }
        return hash;
    }

    /// <summary>Loads a SpaceTime-equipped save through both opening paths and checks Ceres restart inventory and progression.</summary>
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
                EquippedBeams = (SamusBeamFlags)0x100e,
                CollectedBeams = (SamusBeamFlags)0x100f,
                EquippedItems = SamusEquipmentFlags.GravitySuit,
                CollectedItems = SamusEquipmentFlags.GravitySuit,
                LoadingGameState = SaveLoadingGameStates.OpeningCinematic,
            };
            Array.Fill(snapshot.EventBytes, byte.MaxValue);
            return snapshot;
        }

        SuperMetroidGame CreateGame(bool skipOpening)
        {
            var restartBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            var restartSaves = new SaveSlotStore(RetailPresentationFixture());
            restartSaves.SaveSlot(0, CreateResetSnapshot());
            restartSaves.SelectSlot(0);
            var game = CreateRetailGameFixture(
                restartSaves,
                new SuperMetroidGameOptions { SkipOpeningCinematic = skipOpening },
                renderGameplayFrames: false);
            game.BindArtwork(game.Artwork with
            {
                MapPresentation = RetailPresentationFixture(),
            });
            return game;
        }

        static void ConfirmSelectedSave(SuperMetroidGame game)
        {
            FrontendFrame frame = game.Step(0);
            frame = game.Step(SnesButton.Start);
            Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen),
                () => frame = game.Step(0), 150, "SpaceTime title setup");
            frame = game.Step(SnesButton.Start);
            Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus,
                () => frame = game.Step(0), 150, "SpaceTime file menu");
            // Native entry dispatches and fade-in end on the 35th update.
            Until(() => frame.Phase == nameof(FileSelectPhase.Main),
                () => frame = game.Step(0), 40, "SpaceTime file menu interactive");
            frame = game.Step(SnesButton.A);
            Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu,
                () => frame = game.Step(0), 200, "SpaceTime options menu");
            // Options indexes zero and one, fifteen fade-in levels, then the interactive index.
            for (int frameIndex = 0; frameIndex < 18; frameIndex++)
                frame = game.Step(0);
            game.Step(SnesButton.A);
        }

        var cinematicGame = CreateGame(skipOpening: false);
        ConfirmSelectedSave(cinematicGame);
        Until(() => cinematicGame.GameState == SuperMetroidGameState.IntroCinematic,
            () => cinematicGame.Step(0), 200, "SpaceTime opening-cinematic dispatch");

        var skippedGame = CreateGame(skipOpening: true);
        ConfirmSelectedSave(skippedGame);
        Until(() => skippedGame.RuntimeForVerification is not null,
            () => skippedGame.Step(0), 200, "SpaceTime Ceres restart");
        SuperMetroidRuntime runtime = skippedGame.GameplayRuntimeForVerification;
        AssertEqual(AreaId.Ceres, runtime.Room.LoadedRoom.AreaIndex,
            "SpaceTime save restarts at Ceres");
        AssertEqual((ushort)0x100e, runtime.Player.IntroducedSamus.EquippedBeams.ToNativeWord(),
            "SpaceTime Ceres restart retains equipped beams");
        AssertEqual((ushort)0, runtime.Player.IntroducedSamus.MaxMissiles,
            "SpaceTime Ceres restart loses Missiles after the intro flashback");
        AssertEqual((ushort)7, runtime.Player.IntroducedSamus.SuperMissiles,
            "SpaceTime Ceres restart retains Super Missiles");
        AssertEqual((ushort)3, runtime.Player.IntroducedSamus.PowerBombs,
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
    /// <summary>Checks direct and queued beam uploads while the aliased WRAM palette changes between runs.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    [CpuBusBoundary("exact bus test: the SpaceTime palette pointer $90:19FF reads live enemy-projectile WRAM at $7E:19FF")]
    private static void VerifySpacetimeBeamGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var gameplayBus = NewSpacetimeMutableOnlyBus(bus);
        var artwork = InstalledFixtures.Projectiles.BeamTiles;
        byte[] nativeTiles = ReadRetailSpacetimeGraphicsOracle(bus);
        AssertTrue(artwork.TryResolve(SpacetimeBeamGraphicsDefinitions.TileSource, 256, out _),
            "legacy SpaceTime graphics queue resolves through installed artwork");
        // The page SpaceTime's equipped word binds shows the bytes its native DMA reads.
        var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, artwork,
            InstalledFixtures.Projectiles.Trails.Tiles ?? throw new InvalidOperationException("The installation has no trail characters."));
        ReadOnlySpan<byte> page = pages.For((SamusBeamFlags)((ushort)0x100e)).Characters;
        int first = ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount;
        for (int i = 0; i < 256; i++) AssertEqual(nativeTiles[i], page[first + i], "SpaceTime native tile bytes");
        foreach (int seed in new[] { 5, 19 })
        {
            // The palette is a defined one (#1318), whatever the enemy-projectile WRAM it once aliased holds.
            for (int i = 0; i < 32; i++) bus.WriteByte(0x7e19ff + i, (byte)(i * 7 + seed));
            var cgram = new SnesCgram();
            SamusProjectileSystem.LoadBeamPalette(cgram, (SamusBeamFlags)0x100e, artwork.Palettes);
            for (int i = 0; i < 16; i++) AssertEqual(SpacetimeBeamGraphicsDefinitions.PaletteWord, cgram.Colors[0xe0 + i],
                "SpaceTime colors are its defined palette");
        }
    }

    /// <summary>Checks the SpaceTime graphics pointers against their native table words and returns the native tiles.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the SpaceTime adjacent-table graphics pointers and tiles")]
    private static byte[] ReadRetailSpacetimeGraphicsOracle(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        AssertEqual(SpacetimeBeamGraphicsDefinitions.TileSource & 0xffff,
            bus.ReadCartridgeWord(0x90c3cd),
            "SpaceTime tile pointer comes from adjacent native palette table");
        var tiles = new byte[256];
        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = bus.ReadCartridgeByte(0x9ac401 + i);
        return tiles;
    }

    /// <summary>Forbids cartridge-byte reads during the production projectile path while allowing mutable bus memory.</summary>
    private static MutableCartridgeReadGuard NewSpacetimeMutableOnlyBus(SuperMetroidAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(address)) == SnesDmaSourceKind.Cartridge,
            address => $"SpaceTime gameplay read cartridge byte ${address:X6}.");
        return guard;
    }

    /// <summary>Runs the SpaceTime beam checks from the verification command line.</summary>
    [VerificationCommand("--spacetime-beam")]
    internal static int SpacetimeBeamCommand()
    {
        Suite(nameof(VerifySpacetimeBeam), () => VerifySpacetimeBeam());
        return 0;
    }
}

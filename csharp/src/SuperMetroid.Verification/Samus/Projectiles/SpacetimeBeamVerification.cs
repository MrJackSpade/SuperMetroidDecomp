using SuperMetroid.AssetExtraction.Progression;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
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
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.Core.Programs;
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
        VerifySpacetimeOverwritesAgainstRetailRom(bus);
        var gameplayBus = NewSpacetimeMutableOnlyBus(bus);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(16, 16), new RoomLevelPlanes(new RoomTerrainBlock[256], new RoomBlockBehavior[256], new RoomBlockAppearance[256]));

        // Exercise the production pause input that creates Charge + Ice + Spazer + Plasma
        // without Wave; accepting the raw word only in the projectile test would miss the
        // technique's actual player-accessible setup.
        var samus = new SamusState(InstalledFixtures.AnimationContinuation)
        {
            CollectedItems = SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = SamusEquipmentFlags.HiJumpBoots,
            CollectedBeams = (SamusBeamFlags)0x100f,
            EquippedBeams = (SamusBeamFlags)0x1006,
        };
        var selection = CreateRetailPauseFixture(new PauseMenuEntry { Samus = samus, System = new SharedGameState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(0, 0) });
        EnterPauseEquipment(selection);
        SelectPauseBoots(selection);
        selection.Step(0, (ControllerButton.Left | ControllerButton.A));
        AssertEqual((ushort)0x100e, samus.EquippedBeams.ToEquipmentWord(),
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

        // Progress as the native probe's sentinel-filled tables decode, with the opening cinematic as the loading state.
        var system = new SharedGameState();
        system.Progress.CopyFrom(SentinelProgression().Progress);
        system.LoadSavedLoadingGameState(SaveLoadingGameStateCodec.Word(SaveLoadingGameState.OpeningCinematic));

        var shared = CreateBombFixture();
        var projectiles = CreateProjectileFixture();
        SamusProjectileSlotObservation slotsBeforeHeld = projectiles.ObserveSlots();
        SamusProjectileFrameResult held = projectiles.StepFrame(new SamusTerrain { Level = level, Progression = system }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = ControllerButton.X }, new(0, 0), shared);
        AssertEqual(0, slotsBeforeHeld.FiredSlot(projectiles),
            "SpaceTime initial shot allocation");
        AssertEqual(0x9137, projectiles.Slots[0].InstructionPointer,
            "native frame-zero next animation record");

        SamusProjectileSlotObservation slotsBeforeRelease = projectiles.ObserveSlots();
        SamusProjectileFrameResult released = projectiles.StepFrame(new SamusTerrain { Level = level, Progression = system }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, new(0, 0), shared);
        AssertEqual(1, slotsBeforeRelease.FiredSlot(projectiles),
            "Charge release allocates the second uncharged SpaceTime shot");
        AssertEqual(0x0880, system.SavedLoadingGameState,
            "retail frame-one loading_game_state corruption");
        AssertEqual((ushort)0x100e, samus.EquippedBeams.ToEquipmentWord(),
            "SpaceTime corruption does not rewrite live equipment");

        var saveRam = new SaveSlotStore(RetailPresentationFixture());
        saveRam.SaveSlot(0, SuperMetroidSaveSnapshot.Capture(samus, system, new SaveSnapshotContext { Area = AreaId.Crateria, SaveStation = 7 }));
        SuperMetroidSaveSlot saved = saveRam.ReadSlot(0)
            ?? throw new InvalidOperationException("SpaceTime save did not survive checksum validation.");
        AssertEqual((ushort)0x100e, saved.EquippedBeams.ToEquipmentWord(),
            "save retains invalid SpaceTime equipment");
        AssertEqual(0x0880, saved.LoadingGameState,
            "save retains corrupted startup dispatcher word");
        AssertTrue(saved.Progression.SameAs(system.Progress),
            "save retains the corrupted events, bosses, orbs, items, doors, stations and maps");

        var restarted = new SamusState(InstalledFixtures.AnimationContinuation);
        FrontendSession.RestoreSpacetimeRestartInventory(restarted, saved);
        AssertEqual((ushort)0x100e, restarted.EquippedBeams.ToEquipmentWord(),
            "Ceres restart retains the SpaceTime equipment word");
        AssertEqual(0, restarted.Missiles,
            "intro flashback removes current missiles");
        AssertEqual(0, restarted.MaxMissiles,
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

    /// <summary>
    /// Runs the original copy over the supported ROM for every animation position a SpaceTime shot can hold and every
    /// slot, and requires the installed service's overwrite, applied to progress decoded from sentinel-filled tables, to
    /// leave every event, boss, orb, item, door, station, map and the loading state as the tables the copy writes over the
    /// sentinel decode (#1475). What no game routine reads (the padding after the doors, owner decision on #1330, the
    /// eighth area slot, unnamed boss bits and the map-station padding) is not progress and is not compared.
    /// </summary>
    private static void VerifySpacetimeOverwritesAgainstRetailRom(
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(bus);
        var service = InstalledFixtures.SamusOverreads.SpaceTimeOverwrite;
        int reaching = 0;
        int situations = 0;
        foreach (SamusProjectileProgram program in SamusOverreadDecoding.SpaceTimeShotPrograms())
        for (int position = 0; position < SamusProjectileAnimations.Programs.Steps(program).Count; position++)
        for (int slot = 0; slot < SamusProjectileSystem.SlotCount; slot++)
        {
            var cursor = new AnimationProgramCursor<SamusProjectileProgram>(program, position);
            byte?[] native = SpacetimeCopyOracle.ProgressionSlice(cartridge, SamusProjectileProgramAddresses.Address(cursor), slot);
            SharedGameState state = SentinelProgression();
            if (service.For(cursor, slot) is { } overwrite)
            {
                state.Overwrite(overwrite);
                reaching++;
            }
            byte[] expected = [.. native.Select(value => value ?? ProgressionSentinel)];
            AssertTrue(DecodeSlice(expected).SameAs(state.Progress),
                $"SpaceTime from {cursor} in slot {slot} leaves every event, boss, orb, item, door, station and map as the original copy does");
            AssertEqual((int)LittleEndianBytes.Word(expected[ProgressionSliceLayout.LoadingGameStateOffset], expected[ProgressionSliceLayout.LoadingGameStateOffset + 1]),
                state.SavedLoadingGameState, $"SpaceTime from {cursor} in slot {slot} leaves the loading state as the original copy does");
            situations++;
        }
        AssertTrue(reaching > 0, "Some SpaceTime animation positions copy into progression");
        AssertThrows<ArgumentOutOfRangeException>(() => service.For(new AnimationProgramCursor<SamusProjectileProgram>(
            SamusProjectileProgram.PowerUpFacingRight, 0), 0), "A position no SpaceTime shot holds is rejected");
        Console.WriteLine($"SpaceTime overwrite: {situations} positions and slots match the original copy, {reaching} of them reaching progression.");
    }

    /// <summary>The byte every progression table starts with in the overwrite comparison.</summary>
    private const byte ProgressionSentinel = 0xa5;

    /// <summary>
    /// Progress as progression tables filled with the sentinel decode, and the loading state both of whose bytes are the
    /// sentinel: every flag the tables' sentinel bits name.
    /// </summary>
    private static SharedGameState SentinelProgression()
    {
        var state = new SharedGameState();
        state.Progress.CopyFrom(DecodeSlice(Enumerable.Repeat(ProgressionSentinel, SpacetimeCopyOracle.ProgressionSliceByteCount).ToArray()));
        state.LoadSavedLoadingGameState(LittleEndianBytes.Word(ProgressionSentinel, ProgressionSentinel));
        return state;
    }

    /// <summary>
    /// The progress a whole persistent-progress slice stores, read table by table through the cartridge's table codec;
    /// the padding after the doors, the eighth area slot and the map-station padding hold nothing the game reads.
    /// </summary>
    private static PersistentProgress DecodeSlice(byte[] slice)
    {
        var progress = new PersistentProgress();
        ProgressionTableCodec.ReadFlags(slice.AsSpan(ProgressionSliceLayout.EventsOffset, ProgressionTableCodec.EventByteCount), progress.Events);
        ProgressionTableCodec.ReadAreaBosses(slice.AsSpan(ProgressionSliceLayout.BossBitsOffset, ProgressionTableCodec.AreaBossByteCount), progress);
        ProgressionTableCodec.ReadFlags(slice.AsSpan(ProgressionSliceLayout.RoomChozoBitsOffset, ProgressionTableCodec.RoomFlagByteCount), progress.ChozoOrbs);
        ProgressionTableCodec.ReadFlags(slice.AsSpan(ProgressionSliceLayout.CollectedItemBitsOffset, ProgressionTableCodec.RoomFlagByteCount), progress.CollectedItems);
        ProgressionTableCodec.ReadFlags(slice.AsSpan(ProgressionSliceLayout.OpenedDoorBitsOffset, ProgressionTableCodec.RoomFlagByteCount), progress.OpenedDoors);
        ProgressionTableCodec.ReadUsedStations(slice.AsSpan(ProgressionSliceLayout.UsedSaveStationsOffset, ProgressionTableCodec.UsedStationByteCount), progress);
        ProgressionTableCodec.ReadAreaMaps(slice.AsSpan(ProgressionSliceLayout.MapStationsOffset, ProgressionTableCodec.MapStationByteCount), progress);
        return progress;
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
                LoadingGameState = SaveLoadingGameStateCodec.Word(SaveLoadingGameState.OpeningCinematic),
            };
            for (int number = 0; number < PersistentProgress.EventCount; number++)
                snapshot.Progress.Events.Add(number);
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
            frame = game.Step(ControllerButton.Start);
            Until(() => frame.Phase == nameof(TitleSequencePhase.TitleScreen),
                () => frame = game.Step(0), 150, "SpaceTime title setup");
            frame = game.Step(ControllerButton.Start);
            Until(() => frame.GameState == SuperMetroidGameState.FileSelectMenus,
                () => frame = game.Step(0), 150, "SpaceTime file menu");
            // Native entry dispatches and fade-in end on the 35th update.
            Until(() => frame.Phase == nameof(FileSelectPhase.Main),
                () => frame = game.Step(0), 40, "SpaceTime file menu interactive");
            frame = game.Step(ControllerButton.A);
            Until(() => frame.GameState == SuperMetroidGameState.GameOptionsMenu,
                () => frame = game.Step(0), 200, "SpaceTime options menu");
            // Options indexes zero and one, fifteen fade-in levels, then the interactive index.
            for (int frameIndex = 0; frameIndex < 18; frameIndex++)
                frame = game.Step(0);
            game.Step(ControllerButton.A);
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
        AssertEqual((ushort)0x100e, runtime.Player.IntroducedSamus.EquippedBeams.ToEquipmentWord(),
            "SpaceTime Ceres restart retains equipped beams");
        AssertEqual(0, runtime.Player.IntroducedSamus.MaxMissiles,
            "SpaceTime Ceres restart loses Missiles after the intro flashback");
        AssertEqual(7, runtime.Player.IntroducedSamus.SuperMissiles,
            "SpaceTime Ceres restart retains Super Missiles");
        AssertEqual(3, runtime.Player.IntroducedSamus.PowerBombs,
            "SpaceTime Ceres restart retains Power Bombs");
        AssertTrue(!runtime.System.Progress.Events.Members.Any(),
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
    private static void VerifySpacetimeBeamGraphics()
    {
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var artwork = InstalledFixtures.Projectiles.BeamTiles;
        byte[] nativeTiles = ReadRetailSpacetimeGraphicsOracle(bus);
        AssertTrue(artwork.TryResolve(SpacetimeBeamGraphicsDefinitionsConstants.TileSource, 256, out _),
            "legacy SpaceTime graphics queue resolves through installed artwork");
        // The page SpaceTime's equipped word binds shows the bytes its native DMA reads.
        var pages = new ProjectileObjectPages(SpriteFrameTestArtwork.StandardObjects, artwork,
            InstalledFixtures.Projectiles.TrailTiles);
        ReadOnlySpan<byte> page = pages.For((SamusBeamFlags)((ushort)0x100e)).Characters;
        int first = ProjectileObjectPageDefinitions.BeamFirstCharacter * SpriteArtwork.CharacterByteCount;
        for (int i = 0; i < 256; i++) AssertEqual(nativeTiles[i], page[first + i], "SpaceTime native tile bytes");
        foreach (int seed in new[] { 5, 19 })
        {
            // The palette is a defined one (#1318), whatever the enemy-projectile WRAM it once aliased holds.
            for (int i = 0; i < 32; i++) bus.WriteByte(0x7e19ff + i, (byte)(i * 7 + seed));
            var cgram = new SamusColourRig();
            SamusProjectileSystem.LoadBeamPalette(cgram.Samus.Beam, (SamusBeamFlags)0x100e, artwork.Palettes);
            for (int i = 0; i < 16; i++) AssertEqual(SpacetimeBeamGraphicsDefinitions.PaletteColour, cgram.Colors[0xe0 + i],
                "SpaceTime colors are its defined palette");
        }
    }

    /// <summary>Checks the SpaceTime graphics pointers against their native table words and returns the native tiles.</summary>
    private static byte[] ReadRetailSpacetimeGraphicsOracle(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        AssertEqual(SpacetimeBeamGraphicsDefinitionsConstants.TileSource & 0xffff,
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

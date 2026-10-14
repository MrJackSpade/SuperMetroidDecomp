using static RepositoryInputsVerification;
using static SuiteTimingVerification;
using SuperMetroid.Core.Rom;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;
using SuperMetroid.Core.Runtime;
using static FrontendRomFreeCensusVerification;
using static GoldenTorizoRoomSetupVerification;
using static RetailGameFixtureVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Compares installed ROM-free frontend execution with a cartridge-backed reference.</summary>
internal static class FrontendRomFreeVerification
{
    /// <summary>
    /// Drives the actual state-zero dispatcher through the first two menus. This tests
    /// host bindings at state construction, not merely the standalone title renderer.
    /// Mutable SRAM/WRAM remain available; no cartridge byte may be consulted.
    /// </summary>
    internal static void VerifyFrontendRomFreeStartup(GameInstallation installation,
        string sourceRom, bool roomCensus = false)
    {
        var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        // Installed gameplay gets real WRAM/SRAM but no ROM allocation at all.
        // The native reference remains available only to the development probe
        // that identifies the exact BG2 source ranges to forbid.
        var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        AssertEqual(false, MetadataShape.DeclaresProperty(installedMemory.GetType(), "Rom"),
            "installed frontend starts with no cartridge allocation");
        var blockedBg2Bytes = new HashSet<int>();
        var guardedBus = NewFrontendCartridgeReadGuard(installedMemory, blockedBg2Bytes);
        AssertThrows<InvalidOperationException>(
            () => guardedBus.ReadByte(TitleSequenceRomData.Assets.Mode7CharactersAddress),
            "startup ROM guard rejects an otherwise valid title graphics source");

        var native = CreateRetailGameFixture(nativeBus);
        var installed = new SuperMetroidGame(guardedBus);
        Action<SuperMetroidGame, bool> bindInstalled = PrepareRomFreeBindings(installation);
        bindInstalled(installed, true);
        bool titleStartSent = false;
        bool fileSelectStartSent = false;
        var visited = new HashSet<SuperMetroidGameState>();
        for (int frame = 0; frame < 3500; frame++)
        {
            FrontendFrame before = installed.CurrentFrameMetadata;
            ushort input = 0;
            if (before.GameState == SuperMetroidGameState.OpeningCinematic &&
                before.Phase == nameof(TitleSequencePhase.TitleScreen) && !titleStartSent)
            {
                input = (ushort)SnesButton.Start;
                titleStartSent = true;
            }
            else if (before.GameState == SuperMetroidGameState.FileSelectMenus &&
                     before.Phase == nameof(FileSelectPhase.Main) && !fileSelectStartSent)
            {
                input = (ushort)SnesButton.Start;
                fileSelectStartSent = true;
            }

            FrontendFrame expected = native.Step(input);
            FrontendFrame actual = installed.Step(input);
            visited.Add(actual.GameState);
            AssertEqual(expected.GameState, actual.GameState,
                $"installed startup game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed startup native phase at frame {frame}");
            if (frame % 37 == 0 || visited.Count == 1 ||
                actual.GameState != before.GameState)
                AssertTrue(actual.Pixels.SequenceEqual(expected.Pixels),
                    $"installed startup native pixels at frame {frame}, {actual.Phase}");

            if (actual.GameState == SuperMetroidGameState.GameOptionsMenu &&
                actual.Phase == nameof(GameOptionsPhase.Main))
            {
                AssertTrue(titleStartSent && fileSelectStartSent &&
                    visited.Contains(SuperMetroidGameState.OpeningCinematic) &&
                    visited.Contains(SuperMetroidGameState.FileSelectMenus),
                    "ROM-free startup traverses title, file select and options");
                Console.WriteLine($"Frontend ROM-free startup: {frame + 1} native-parity frames through options, all cartridge reads guarded.");
                VerifyFrontendRomFreeIntro(native, installed, blockedBg2Bytes, nativeBus, bindInstalled, roomCensus);
                return;
            }
        }
        throw new InvalidOperationException(
            "ROM-free startup fixture did not reach the options main menu.");
    }

    /// <summary>
    /// Retain the same validated immutable installation catalogs when a diagnostic
    /// room snapshot is restored. Debugger states omit external artwork by design.
    /// </summary>
    internal static Action<SuperMetroidGame, bool> PrepareRomFreeBindings(GameInstallation installation,
        EnemyTileArtworkCatalog? enemyArtwork = null)
    {
        var maps = installation.LoadMaps();
        var palettes = installation.LoadGameplayBasePalettes();
        var enemies = enemyArtwork ?? installation.LoadEnemyTiles();
        var objects = installation.LoadStandardObjects();
        var intro = installation.LoadIntroCinematicArt();
        var samus = installation.LoadSamusBodyArt();
        var characters = installation.LoadRoomCharacters();
        var roomPalettes = installation.LoadRoomPalettes();
        var metatiles = installation.LoadRoomMetatiles();
        var visualLayouts = installation.LoadRoomVisualLayouts();
        var levelStreams = installation.LoadRoomLevelStreams();
        var backgrounds = installation.LoadRoomBackgroundTilemaps();
        var sky = installation.LoadRoomSkyTilemaps();
        var samusEaterVisuals = installation.LoadRoomPlmSamusEaterVisuals();
        var assetRoomPlmShotBlockVisuals = installation.LoadRoomPlmShotBlockVisuals();
        var assetRoomPlmGrappleBlockVisuals = installation.LoadRoomPlmGrappleBlockVisuals();
        var assetRoomPlmStationVisuals = installation.LoadRoomPlmStationVisuals();
        var assetRoomPlmBlueDoorVisuals = installation.LoadRoomPlmBlueDoorVisuals();
        var assetRoomPlmColoredDoorVisuals = installation.LoadRoomPlmColoredDoorVisuals();
        var assetRoomPlmGreyDoorVisuals = installation.LoadRoomPlmGreyDoorVisuals();
        var assetRoomPlmEyeDoorVisuals = installation.LoadRoomPlmEyeDoorVisuals();
        var assetRoomPlmMotherBrainGlassVisuals = installation.LoadRoomPlmMotherBrainGlassVisuals();
        var assetRoomPlmNoobTubeVisuals = installation.LoadRoomPlmNoobTubeVisuals();
        var assetRoomPlmDownwardGateVisuals = installation.LoadRoomPlmDownwardGateVisuals();
        var assetRoomPlmElevatorPlatformVisuals = installation.LoadRoomPlmElevatorPlatformVisuals();
        var assetRoomPlmEscapeGateVisuals = installation.LoadRoomPlmEscapeGateVisuals();
        var assetRoomPlmBombTorizoHandVisuals = installation.LoadRoomPlmBombTorizoHandVisuals();
        var assetRoomPlmDraygonCannonVisuals = installation.LoadRoomPlmDraygonCannonVisuals();
        var assetRoomPlmChozoStatueVisuals = installation.LoadRoomPlmChozoStatueVisuals();
        var assetRoomPlmLinkedRestoreVisuals = installation.LoadRoomPlmLinkedRestoreVisuals();
        var assetRoomPlmTourianAccessVisuals = installation.LoadRoomPlmTourianAccessVisuals();
        var assetRoomPlmSpeedBoosterVisuals = installation.LoadRoomPlmSpeedBoosterVisuals();
        var assetRoomPlmMaridiaElevatubeVisuals = installation.LoadRoomPlmMaridiaElevatubeVisuals();
        var assetRoomPlmSporeSpawnCeilingVisuals = installation.LoadRoomPlmSporeSpawnCeilingVisuals();
        var assetRoomPlmSamusEaterVisuals = installation.LoadRoomPlmSamusEaterVisuals();
        var assetRoomPlmBotwoonWallVisuals = installation.LoadRoomPlmBotwoonWallVisuals();
        var assetRoomPlmKraidVisuals = installation.LoadRoomPlmKraidVisuals();
        var assetRoomPlmCrocomireVisuals = installation.LoadRoomPlmCrocomireVisuals();
        var assetRoomPlmMotherBrainFakeDeathVisuals = installation.LoadRoomPlmMotherBrainFakeDeathVisuals();
        var assetRoomPlmCollectibleVisuals = installation.LoadRoomPlmCollectibleVisuals();
        var assetRoomPlmDynamicCollectibleArt = installation.LoadRoomPlmDynamicCollectibleArt();
        var assetXrayRevealVisuals = installation.LoadXrayRevealVisuals();
        InstalledProjectilePresentation projectiles = installation.LoadProjectiles();
        return (game, bindIntro) =>
        {
            game.BindArtwork(game.Artwork with
            {
                MapPresentation = maps,
                GameplayBasePalettes = palettes,
                EnemyTileArtwork = enemies,
                StandardObjectArt = objects,
                RoomPlmShotBlockVisuals = assetRoomPlmShotBlockVisuals,
                RoomPlmGrappleBlockVisuals = assetRoomPlmGrappleBlockVisuals,
                RoomPlmStationVisuals = assetRoomPlmStationVisuals,
                RoomPlmBlueDoorVisuals = assetRoomPlmBlueDoorVisuals,
                RoomPlmColoredDoorVisuals = assetRoomPlmColoredDoorVisuals,
                RoomPlmGreyDoorVisuals = assetRoomPlmGreyDoorVisuals,
                RoomPlmEyeDoorVisuals = assetRoomPlmEyeDoorVisuals,
                RoomPlmMotherBrainGlassVisuals = assetRoomPlmMotherBrainGlassVisuals,
                RoomPlmNoobTubeVisuals = assetRoomPlmNoobTubeVisuals,
                RoomPlmDownwardGateVisuals = assetRoomPlmDownwardGateVisuals,
                RoomPlmElevatorPlatformVisuals = assetRoomPlmElevatorPlatformVisuals,
                RoomPlmEscapeGateVisuals = assetRoomPlmEscapeGateVisuals,
                RoomPlmBombTorizoHandVisuals = assetRoomPlmBombTorizoHandVisuals,
                RoomPlmDraygonCannonVisuals = assetRoomPlmDraygonCannonVisuals,
                RoomPlmChozoStatueVisuals = assetRoomPlmChozoStatueVisuals,
                RoomPlmLinkedRestoreVisuals = assetRoomPlmLinkedRestoreVisuals,
                RoomPlmTourianAccessVisuals = assetRoomPlmTourianAccessVisuals,
                RoomPlmSpeedBoosterVisuals = assetRoomPlmSpeedBoosterVisuals,
                RoomPlmMaridiaElevatubeVisuals = assetRoomPlmMaridiaElevatubeVisuals,
                RoomPlmSporeSpawnCeilingVisuals = assetRoomPlmSporeSpawnCeilingVisuals,
                RoomPlmSamusEaterVisuals = assetRoomPlmSamusEaterVisuals,
                RoomPlmBotwoonWallVisuals = assetRoomPlmBotwoonWallVisuals,
                RoomPlmKraidVisuals = assetRoomPlmKraidVisuals,
                RoomPlmCrocomireVisuals = assetRoomPlmCrocomireVisuals,
                RoomPlmMotherBrainFakeDeathVisuals = assetRoomPlmMotherBrainFakeDeathVisuals,
                RoomPlmCollectibleVisuals = assetRoomPlmCollectibleVisuals,
                RoomPlmDynamicCollectibleArt = assetRoomPlmDynamicCollectibleArt,
                XrayRevealVisuals = assetXrayRevealVisuals,
                BeamArtwork = projectiles.BeamTiles,
            });
            // An already-finished intro is deliberately not rebound for the
            // isolated gameplay-room census: its restored DMA side effect
            // would test a retired cinematic, not the room under examination.
            if (bindIntro)
                game.BindArtwork(game.Artwork with
                {
                    IntroCinematicArt = intro,
                });
            game.BindArtwork(game.Artwork with
            {
                SamusBodyArt = samus,
                RoomCharacterArt = characters,
                RoomPaletteArt = roomPalettes,
                RoomMetatileArt = metatiles,
                RoomVisualLayouts = visualLayouts,
                RoomLevelStreams = levelStreams,
                RoomBackgroundTilemapArt = backgrounds,
                RoomSkyTilemapArt = sky,
                RoomPlmSamusEaterVisuals = samusEaterVisuals,
                BeamArtwork = projectiles.BeamTiles,
                ProjectileCompositions = projectiles.Catalog,
                ProjectileFrameBindings = projectiles.FrameBindings,
                TrailArtwork = projectiles.Trails,
                ChargeFlarePlacement = projectiles.FlarePlacement,
                ChargeFlareCompositions = projectiles.FlareCompositions,
                GrappleArtwork = projectiles.GrappleTiles,
            });
        };
    }

    /// <summary>
    /// Continue the same host instances through the options dispatcher, narration,
    /// and Mother Brain flashback. This catches missing live content bindings at the
    /// real transition and during the projectile/hurt animation, not just at startup.
    /// </summary>
    private static void VerifyFrontendRomFreeIntro(SuperMetroidGame native,
        SuperMetroidGame installed, HashSet<int> blockedBg2Bytes, ISnesAddressSpace nativeBus,
        Action<SuperMetroidGame, bool> bindInstalled, bool roomCensus)
    {
        bool startSent = false;
        int introFrames = 0;
        int motherBrainFrame = -1;
        for (int frame = 0; frame < 9000; frame++)
        {
            FrontendFrame before = installed.CurrentFrameMetadata;
            // Advance the narrator at a repeatable cadence; the original short
            // fixture ended before its first bank-$93 projectile animation frame.
            ushort input = !startSent ? (ushort)SnesButton.Start :
                frame % 47 == 0 ? (ushort)SnesButton.A : (ushort)0;
            startSent = true;
            FrontendFrame expected = native.Step(input);
            FrontendFrame actual = installed.Step(input);
            AssertEqual(expected.GameState, actual.GameState,
                $"installed intro game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed intro native phase at frame {frame}");
            AssertTrue(actual.Pixels.SequenceEqual(expected.Pixels),
                $"installed intro native pixels at frame {frame}, {actual.Phase}");
            if (before.GameState == SuperMetroidGameState.IntroCinematic &&
                actual.GameState != SuperMetroidGameState.IntroCinematic)
            {
                AssertTrue(motherBrainFrame >= 0,
                    "intro reached its game-state handoff after Mother Brain flashback");
                const int postIntroFrameCount = 1200;
                for (int gameplayFrame = 0; gameplayFrame < postIntroFrameCount; gameplayFrame++)
                {
                    FrontendFrame expectedGameplay = native.Step(0);
                    FrontendFrame actualGameplay = installed.Step(0);
                    AssertEqual(expectedGameplay.GameState, actualGameplay.GameState,
                        $"installed post-intro game state frame {gameplayFrame}");
                    AssertEqual(expectedGameplay.Phase, actualGameplay.Phase,
                        $"installed post-intro phase frame {gameplayFrame}");
                    AssertTrue(actualGameplay.Pixels.SequenceEqual(expectedGameplay.Pixels),
                        $"installed post-intro pixels frame {gameplayFrame}");
                }
                VerifyFrontendRomFreeRestoredKraidHud(native, installed, bindInstalled);
                if (roomCensus)
                    VerifyFrontendRomFreeRoomCensus(native, installed, bindInstalled);
                VerifyFrontendRomFreeCeresInput(native, installed);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.CeresRidleyRoom, RoomName = "Ceres Ridley" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.LandingSite, RoomName = "Landing Site" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.ParlorAndAlcatraz, RoomName = "Parlor" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.WestOcean, RoomName = "West Ocean" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.NorfairRoom1E, RoomName = "Norfair room 1E" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GreenBrinstarMainShaft, RoomName = "Green Brinstar main shaft" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.BlueBrinstarElevatorRoom, RoomName = "Blue Brinstar elevator" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.EastTunnel, RoomName = "Maridia east tunnel" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointersTooling.CrateriaSaveStation, RoomName = "Crateria save station" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.LowerNorfairMainHall, RoomName = "Lower Norfair main hall" });
                // Kraid's arm switches to its rising/sinking extended frame at
                // neutral frame 654. A short room-entry check misses that draw.
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.Kraid, RoomName = "Kraid arm rising/sinking" }, frameCount: 700);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.Phantoon, RoomName = "Phantoon" });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GauntletEast, RoomName = "Gauntlet east Yapping Maw" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.PreMoat, RoomName = "pre-moat KiHunter" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.CrabMaze, RoomName = "Crab Maze Sciser" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.Flyway, RoomName = "Flyway shared fly family" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.CrateriaKagoRoom, RoomName = "Crateria Kago" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.CrateriaFaceBlockRoom, RoomName = "Crateria face block" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.CrateriaMorphBallEyeRoom, RoomName = "Crateria Morph Ball eye" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointersTooling.BrinstarShutterRoom, RoomName = "Brinstar shutters" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.TourianMetroidRoom, RoomName = "Tourian ordinary Metroids" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.DeadTorizoCorpse, RoomName = "Tourian dead-Torizo corpse" }, frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.MotherBrainChamber, RoomName = "Mother Brain chamber" }, frameCount: 90);
                // The long neutral Draygon fight can fatally damage Samus;
                // all shorter room checks must execute while gameplay is live.
                BlockFrontendExtendedBg2Streams(blockedBg2Bytes, nativeBus,
                    DraygonBg2FrameDefinitions.Bank,
                    DraygonBg2FrameDefinitions.Frames);
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.Draygon, RoomName = "Draygon room entry, dance and fight handoff" }, frameCount: 2600, setup: (nativeRoom, installedRoom) =>
                    {
                        // Keep the fight actor alive through the long neutral
                        // comparison without letting damage switch the frontend
                        // into a death screen before the next room is loaded.
                        var options = new SuperMetroidGameOptions { Invincibility = true };
                        nativeRoom.GameplayRuntimeForVerification.ApplyHostOptions(options);
                        installedRoom.GameplayRuntimeForVerification.ApplyHostOptions(options);
                    });
                // Cross the wake/combat handoff into the first linked walking
                // lists. The older 700-frame low-health check reached Game Over
                // and its trailing pixel matches did not exercise the room.
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = "Golden Torizo wake and right-facing attacks" }, frameCount: 850, setup: (nativeRoom, installedRoom) =>
                    {
                        // Enter the authored lower-right wake rectangle on both
                        // independently stepped games, without crossing a door.
                        nativeRoom.GameplayRuntimeForVerification.Player.IntroducedSamus.Position = nativeRoom.GameplayRuntimeForVerification.Player.IntroducedSamus.Position.WithWhole(new(0x0180, 0x0150));
                        installedRoom.GameplayRuntimeForVerification.Player.IntroducedSamus.Position = installedRoom.GameplayRuntimeForVerification.Player.IntroducedSamus.Position.WithWhole(new(0x0180, 0x0150));
                        // The attack choices are random. Pin the seed this scenario was authored
                        // against so earlier frontend timing cannot change which attacks occur.
                        nativeRoom.GameplayRuntimeForVerification.System.SetRandomNumber(0x23fc);
                        installedRoom.GameplayRuntimeForVerification.System.SetRandomNumber(0x23fc);
                        var options = new SuperMetroidGameOptions { Invincibility = false };
                        nativeRoom.GameplayRuntimeForVerification.ApplyHostOptions(options);
                        installedRoom.GameplayRuntimeForVerification.ApplyHostOptions(options);
                    });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = "Golden Torizo left-foot right-sonic program" }, frameCount: 90, setup: (nativeRoom, installedRoom) =>
                    {
                        foreach (SuperMetroidGame game in new[] { nativeRoom, installedRoom })
                        {
                            RoomEnemySlot boss = game.GameplayRuntimeForVerification.Enemies.SlotTable.Slots.Single(
                                slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);
                            boss.CurrentInstruction = GoldenTorizoRightSonicInstructionProgramDefinitions.Start;
                            boss.InstructionTimer = NativeCountdown.Start(1);
                        }
                    }, golden: new GoldenTorizoForcing { ForcedGoldenSonicStart = GoldenTorizoRightSonicInstructionProgramDefinitions.Start });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = "Golden Torizo right-foot right-sonic program" }, frameCount: 90, setup: (nativeRoom, installedRoom) =>
                    {
                        foreach (SuperMetroidGame game in new[] { nativeRoom, installedRoom })
                        {
                            RoomEnemySlot boss = game.GameplayRuntimeForVerification.Enemies.SlotTable.Slots.Single(
                                slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);
                            boss.CurrentInstruction =
                                GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward;
                            boss.InstructionTimer = NativeCountdown.Start(1);
                        }
                    }, golden: new GoldenTorizoForcing { ForcedGoldenSonicStart = GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward });
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = "Golden Torizo stunned tile loop" }, frameCount: 80, setup: (nativeRoom, installedRoom) =>
                    {
                        foreach (SuperMetroidGame game in new[] { nativeRoom, installedRoom })
                        {
                            RoomEnemySystem enemies = game.GameplayRuntimeForVerification.Enemies;
                            RoomEnemySlot boss = enemies.SlotTable.Slots.Single(slot =>
                                slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);
                            boss.CurrentInstruction = GoldenTorizoStunnedInstructionProgramDefinitions.Start;
                            boss.InstructionTimer = NativeCountdown.Start(1);
                            boss.Parameter2 |= GoldenTorizoBehavioralProperties.Stunned;
                            (enemies.BombTorizoBehavior.GoldenTorizo ?? throw new InvalidOperationException("Verification requires enemies.BombTorizoBehavior.GoldenTorizo.")).ReturnInstruction =
                                GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;
                        }
                    }, golden: new GoldenTorizoForcing { ForcedGoldenStun = true });
                foreach (ushort leftTurnStart in new ushort[]
                {
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge,
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.Turn,
                })
                {
                    VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = $"Golden Torizo left turn $AA:{leftTurnStart:X4}" }, frameCount: leftTurnStart ==
                            GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge
                                ? 120 : 8, setup: (nativeRoom, installedRoom) =>
                            ForceGoldenTorizoLeftTurn(nativeRoom, installedRoom,
                                leftTurnStart), golden: new GoldenTorizoForcing { ForcedGoldenLeftTurnStart = leftTurnStart });
                }
                // The left-foot-forward Chozo-orb attack is RNG-selected; start it directly
                // rather than waiting on an incidental AI trajectory to reach it.
                VerifyFrontendRomFreeRoom(native, installed, new RomFreeRoom { RoomPointer = RoomHeaderPointers.GoldenTorizo, RoomName = "Golden Torizo left-foot Chozo orb" }, frameCount: 80, setup: (nativeRoom, installedRoom) =>
                    {
                        foreach (SuperMetroidGame game in new[] { nativeRoom, installedRoom })
                        {
                            RoomEnemySystem enemies = game.GameplayRuntimeForVerification.Enemies;
                            RoomEnemySlot boss = enemies.SlotTable.Slots.Single(slot =>
                                slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo);
                            boss.CurrentInstruction = GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Start;
                            boss.InstructionTimer = NativeCountdown.Start(1);
                            (enemies.BombTorizoBehavior.GoldenTorizo ?? throw new InvalidOperationException("Verification requires enemies.BombTorizoBehavior.GoldenTorizo.")).ReturnInstruction =
                                GoldenTorizoCombatInstructionPointers.WalkingLeftRightLeg;
                        }
                    }, golden: new GoldenTorizoForcing { ForcedGoldenLeftFootOrb = true });
                Console.WriteLine($"Frontend ROM-free intro: {frame + 1} native-parity cinematic frames plus {postIntroFrameCount} post-handoff frames; all cartridge reads guarded in every sampled room.");
                return;
            }
            if (actual.GameState != SuperMetroidGameState.IntroCinematic)
                continue;
            introFrames++;
            if (motherBrainFrame < 0 && actual.Phase == nameof(IntroCinematicPhase.MotherBrainFlashback))
                motherBrainFrame = frame;
        }
        throw new InvalidOperationException(
            $"ROM-free frontend fixture did not complete the opening cinematic in {introFrames} cinematic frames; final phase {installed.CurrentFrameMetadata.Phase}.");
    }

    /// <summary>
    /// A gameplay debugger restore rebinds host artwork after the original HUD DMA.
    /// Kraid relocates BG3 characters to $2000 and owns the ordinary $4000 region as
    /// his BG2 map, so the deferred host refresh must use the room's selected base.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static void VerifyFrontendRomFreeRestoredKraidHud(SuperMetroidGame native,
        SuperMetroidGame installed, Action<SuperMetroidGame, bool> bindInstalled)
    {
        using var nativeSnapshot = new MemoryStream();
        using var installedSnapshot = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(nativeSnapshot, native);
        DebuggerObjectGraphSerializer.Serialize(installedSnapshot, installed);
        nativeSnapshot.Position = 0;
        installedSnapshot.Position = 0;
        SuperMetroidGame nativeRoom =
            DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(nativeSnapshot);
        SuperMetroidGame installedRoom =
            DebuggerObjectGraphSerializer.Deserialize<SuperMetroidGame>(installedSnapshot);
        bindInstalled(nativeRoom, false);
        bindInstalled(installedRoom, false);
        nativeRoom.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(RoomHeaderPointers.Kraid);
        installedRoom.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(RoomHeaderPointers.Kraid);
        AssertEqual(RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord,
            installedRoom.GameplayRuntimeForVerification.Display.GameplayHudCharacterBaseWord,
            "restored Kraid room selects the relocated HUD character base");
        int bg2Byte = KraidBackgroundRomData.LiveBg2TilemapWord * sizeof(ushort);
        int bg2Bytes = KraidBackgroundRomData.WorkingTilemapWords * sizeof(ushort);
        AssertTrue(nativeRoom.GameplayRuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)
                .SequenceEqual(installedRoom.GameplayRuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)),
            "restored Kraid BG2 pages match before the first accepted NMI");
        FrontendFrame expected = nativeRoom.Step(0);
        FrontendFrame actual = installedRoom.Step(0);
        AssertTrue(nativeRoom.GameplayRuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)
                .SequenceEqual(installedRoom.GameplayRuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)),
            "restored Kraid BG2 pages remain intact after the deferred HUD refresh");
        AssertTrue(actual.Pixels.SequenceEqual(expected.Pixels),
            "restored Kraid first frame matches native pixels after HUD artwork rebind");
        Console.WriteLine("Frontend ROM-free Kraid restore: relocated HUD refresh preserves both BG2 pages and stock pixels.");
    }

    /// <summary>
    /// Exercise actual Samus movement and a beam shot after the cinematic handoff.
    /// A neutral-only window cannot expose projectile/pose presentation reads.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    private static void VerifyFrontendRomFreeCeresInput(SuperMetroidGame native,
        SuperMetroidGame installed)
    {
        AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
            "ROM-free input fixture reaches playable Ceres");
        AssertTrue(installed.GameplayMovementEnabled,
            "ROM-free input fixture enables Samus movement");
        ushort initialX = installed.GameplaySamusX;
        ushort? initialRoom = installed.GameplayActiveRoomPointer;
        bool fired = false;
        bool firedDuringSustainedInput = false;
        int verifiedFrames = 0;
        for (int frame = 0; frame < 400; frame++)
        {
            ushort input = frame < 24 ? (ushort)SnesButton.Right :
                frame == 25 ? (ushort)SnesButton.X :
                frame is >= 40 and < 60 ? (ushort)(SnesButton.A | SnesButton.Left) :
                frame >= 60 ? (ushort)(SnesButton.Right | SnesButton.X) : (ushort)0;
            FrontendFrame expected = native.Step(input);
            FrontendFrame actual = installed.Step(input);
            AssertEqual(expected.GameState, actual.GameState,
                $"installed Ceres input state frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed Ceres input phase frame {frame}");
            if (!actual.Pixels.SequenceEqual(expected.Pixels))
            {
                int first = 0;
                while (actual.Pixels[first].Equals(expected.Pixels[first]))
                    first++;
                var nativeSamus = native.GameplayRuntimeForVerification.Player.IntroducedSamus;
                var installedSamus = installed.GameplayRuntimeForVerification.Player.IntroducedSamus;
                int firstVram = 0;
                ReadOnlySpan<byte> nativeVram = native.GameplayRuntimeForVerification.Vram.Bytes;
                ReadOnlySpan<byte> installedVram = installed.GameplayRuntimeForVerification.Vram.Bytes;
                while (firstVram < nativeVram.Length && nativeVram[firstVram] == installedVram[firstVram])
                    firstVram++;
                int firstOam = 0;
                ReadOnlySpan<byte> nativeOam = native.GameplayRuntimeForVerification.DisplayedOam.LowTable;
                ReadOnlySpan<byte> installedOam = installed.GameplayRuntimeForVerification.DisplayedOam.LowTable;
                while (firstOam < nativeOam.Length && nativeOam[firstOam] == installedOam[firstOam])
                    firstOam++;
                throw new InvalidOperationException(
                    $"Installed Ceres input pixels differ at frame {frame}, pixel " +
                    $"({first % FrontendFrame.Width},{first / FrontendFrame.Width}): " +
                    $"native={expected.Pixels[first]}, installed={actual.Pixels[first]}; " +
                    $"Samus native=({native.GameplaySamusX},{native.GameplaySamusY}) " +
                    $"pose ${(int)native.GameplaySamusPose:X2}/frame {nativeSamus.AnimationFrame}/" +
                    $"top {nativeSamus.Presentation.TopSpritemapIndex}/bottom {nativeSamus.Presentation.BottomSpritemapIndex}, installed=" +
                    $"({installed.GameplaySamusX},{installed.GameplaySamusY}) " +
                    $"pose ${(int)installed.GameplaySamusPose:X2}/frame {installedSamus.AnimationFrame}/" +
                    $"top {installedSamus.Presentation.TopSpritemapIndex}/bottom {installedSamus.Presentation.BottomSpritemapIndex}; " +
                    $"first VRAM diff={firstVram}, first OAM diff={firstOam}.");
            }
            fired |= installed.GameplayLastFiredProjectileSlot is not null;
            firedDuringSustainedInput |= frame >= 60 &&
                installed.GameplayLastFiredProjectileSlot is not null;
            verifiedFrames++;
            // Keep this controller fixture within the first room boundary. A
            // later room belongs to its own focused installed-runtime check.
            if (initialRoom is not null &&
                installed.GameplayActiveRoomPointer is ushort currentRoom &&
                currentRoom != initialRoom)
                break;
        }
        AssertTrue(installed.GameplaySamusX != initialX,
            "ROM-free Ceres input moves Samus in world space");
        AssertTrue(fired, "ROM-free Ceres input produces a beam shot");
        AssertTrue(firedDuringSustainedInput,
            "ROM-free Ceres input produces a beam shot during sustained movement");
        Console.WriteLine($"Frontend ROM-free Ceres input: {verifiedFrames} direction and firing frames through at most one room boundary match stock pixels without cartridge reads.");
    }

    /// <summary>
    /// Compare representative directly loaded room states without crossing a door.
    /// This covers Ceres and ordinary enemy composition, scrolling skies,
    /// elevators, save rooms, Norfair FX, and Phantoon's BG2 frames. It tests room
    /// initialization and neutral-frame presentation, not incoming doors or travel.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native VRAM words")]
    internal static void VerifyFrontendRomFreeRoom(SuperMetroidGame native, SuperMetroidGame installed, RomFreeRoom room, int frameCount = 90, Action<SuperMetroidGame, SuperMetroidGame>? setup = null, GoldenTorizoForcing golden = default, ushort heldInput = 0)
    {
        native.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(
            room.RoomPointer);
        installed.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(
            room.RoomPointer);
        AssertEqual(SuperMetroidGameState.MainGameplay, native.GameState,
            $"native {room.RoomName} fixture begins in active gameplay");
        AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
            $"installed {room.RoomName} fixture begins in active gameplay");
        // These isolated rooms are not one continuous playthrough. Refill on
        // entry so damage in an earlier room cannot turn a later comparison
        // into two identical death screens.
        native.GameplayRuntimeForVerification.Player.IntroducedSamus.MaxHealth = 1499;
        native.GameplayRuntimeForVerification.Player.IntroducedSamus.Health = 1499;
        installed.GameplayRuntimeForVerification.Player.IntroducedSamus.MaxHealth = 1499;
        installed.GameplayRuntimeForVerification.Player.IntroducedSamus.Health = 1499;
        setup?.Invoke(native, installed);
        RoomEnemySlot? awakenedGoldenTorizo = room.RoomPointer == RoomHeaderPointers.GoldenTorizo &&
            setup is not null
            ? installed.GameplayRuntimeForVerification.Enemies.SlotTable.Slots.FirstOrDefault(slot =>
                slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo)
            : null;
        if (room.RoomPointer == RoomHeaderPointers.GoldenTorizo && setup is not null)
        {
            AssertTrue(awakenedGoldenTorizo is not null,
                "Golden Torizo wake-up fixture loaded its live boss slot");
            VerifyTorizoOperandFreeCallbackDoesNotReadNextWord(
                installed.GameplayRuntimeForVerification.Enemies, (awakenedGoldenTorizo ?? throw new InvalidOperationException("Verification requires awakenedGoldenTorizo.")));
        }
        bool goldenWakeObserved = false;
        bool goldenWalkingObserved = false;
        bool goldenRightwardObserved = false;
        bool goldenJumpBackObserved = false;
        bool goldenRightOrbObserved = false;
        bool goldenEyeBeamAttackObserved = false;
        bool forcedGoldenSonicObserved = false;
        bool forcedGoldenStunObserved = false;
        bool forcedGoldenStunReleased = false;
        bool forcedGoldenLeftTurnObserved = false;
        bool forcedGoldenFallingLeftObserved = false;
        bool goldenLeftFootOrbProgramObserved = false;
        ReadOnlySpan<byte> nativeLoadedVram = native.GameplayRuntimeForVerification.Vram.Bytes;
        ReadOnlySpan<byte> installedLoadedVram = installed.GameplayRuntimeForVerification.Vram.Bytes;
        int firstLoadVram = 0;
        while (firstLoadVram < nativeLoadedVram.Length &&
               nativeLoadedVram[firstLoadVram] == installedLoadedVram[firstLoadVram])
            firstLoadVram++;
        for (int frame = 0; frame < frameCount; frame++)
        {
            FrontendFrame expected = native.Step(heldInput);
            FrontendFrame actual;
            try
            {
                actual = installed.Step(heldInput);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    $"Installed {room.RoomName} failed at neutral frame {frame}.", error);
            }
            AssertEqual(room.RoomPointer,
                installed.GameplayActiveRoomPointer,
                $"installed {room.RoomName} fixture remains in one room at frame {frame}");
            AssertEqual(expected.GameState, actual.GameState,
                $"installed {room.RoomName} game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed {room.RoomName} phase at frame {frame}");
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >
                    GoldenTorizoInitialInstructionProgramDefinitions.Sleep)
                goldenWakeObserved = true;
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    GoldenTorizoWalkingInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    GoldenTorizoWalkingInstructionProgramDefinitions.End)
                goldenWalkingObserved = true;
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    GoldenTorizoRightwardInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    GoldenTorizoRightwardInstructionProgramDefinitions.End)
                goldenRightwardObserved = true;
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    TorizoJumpBackInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    TorizoJumpBackInstructionProgramDefinitions.End)
                goldenJumpBackObserved = true;
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    GoldenTorizoRightOrbInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    GoldenTorizoRightOrbInstructionProgramDefinitions.End)
                goldenRightOrbObserved = true;
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    GoldenTorizoEyeBeamAttackInstructionProgramDefinitions.End)
                goldenEyeBeamAttackObserved = true;
            if (awakenedGoldenTorizo is not null &&
                golden.ForcedGoldenSonicStart is ushort sonicStart &&
                awakenedGoldenTorizo.CurrentInstruction >= sonicStart &&
                awakenedGoldenTorizo.CurrentInstruction <
                    (sonicStart == GoldenTorizoRightSonicInstructionProgramDefinitions.Start
                        ? GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward
                        : GoldenTorizoRightSonicInstructionProgramDefinitions.End))
                forcedGoldenSonicObserved = true;
            if (awakenedGoldenTorizo is not null && golden.ForcedGoldenStun &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    GoldenTorizoStunnedInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    GoldenTorizoStunnedInstructionProgramDefinitions.End)
                forcedGoldenStunObserved = true;
            // The stun's ClearAnimationLock and its return path's UnmarkStunned release the
            // boss; later attacks may lock again, so observe the release, not the last frame.
            else if (forcedGoldenStunObserved && !forcedGoldenStunReleased &&
                     (installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.GoldenTorizo ?? throw new InvalidOperationException("Verification requires installed.GameplayRuntimeForVerification.Enemies.BombTorizoBehavior.GoldenTorizo.")).ShotGuard == 0 &&
                     ((awakenedGoldenTorizo ?? throw new InvalidOperationException("Verification requires awakenedGoldenTorizo.")).Parameter2 & GoldenTorizoBehavioralProperties.Stunned) == 0)
                forcedGoldenStunReleased = true;
            if (awakenedGoldenTorizo is not null &&
                golden.ForcedGoldenLeftTurnStart is not null &&
                awakenedGoldenTorizo.SpritemapPointer ==
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.FacingScreenFrame)
                forcedGoldenLeftTurnObserved = true;
            if (awakenedGoldenTorizo is not null &&
                golden.ForcedGoldenLeftTurnStart ==
                    GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge &&
                awakenedGoldenTorizo.SpritemapPointer ==
                    TorizoFallingLeftInstructionProgramDefinitions.FallingFrame)
                forcedGoldenFallingLeftObserved = true;
            if (awakenedGoldenTorizo is not null &&
                awakenedGoldenTorizo.CurrentInstruction >=
                    GoldenTorizoLeftFootOrbInstructionProgramDefinitions.Start &&
                awakenedGoldenTorizo.CurrentInstruction <
                    GoldenTorizoLeftFootOrbInstructionProgramDefinitions.End)
                goldenLeftFootOrbProgramObserved = true;
            if (!actual.Pixels.SequenceEqual(expected.Pixels))
            {
                int first = -1;
                int count = 0;
                int minX = FrontendFrame.Width;
                int minY = FrontendFrame.Height;
                int maxX = -1;
                int maxY = -1;
                for (int pixel = 0; pixel < actual.Pixels.Length; pixel++)
                {
                    if (actual.Pixels[pixel].Equals(expected.Pixels[pixel]))
                        continue;
                    if (first < 0) first = pixel;
                    count++;
                    int x = pixel % FrontendFrame.Width;
                    int y = pixel / FrontendFrame.Width;
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
                ReadOnlySpan<byte> nativeVram = native.GameplayRuntimeForVerification.Vram.Bytes;
                ReadOnlySpan<byte> installedVram = installed.GameplayRuntimeForVerification.Vram.Bytes;
                int firstVram = 0;
                while (firstVram < nativeVram.Length &&
                       nativeVram[firstVram] == installedVram[firstVram])
                    firstVram++;
                var changedVramPages = new List<string>();
                for (int page = 0; page < nativeVram.Length; page += 0x1000)
                {
                    int changed = 0;
                    for (int offset = page; offset < page + 0x1000; offset++)
                        if (nativeVram[offset] != installedVram[offset]) changed++;
                    if (changed != 0) changedVramPages.Add($"${page:X4}:{changed}");
                }
                ReadOnlySpan<Bgr555> nativeColors = native.GameplayRuntimeForVerification.Cgram.Colors;
                ReadOnlySpan<Bgr555> installedColors = installed.GameplayRuntimeForVerification.Cgram.Colors;
                int changedColors = 0;
                for (int color = 0; color < nativeColors.Length; color++)
                    if (nativeColors[color] != installedColors[color]) changedColors++;
                throw new InvalidOperationException(
                    $"Installed {room.RoomName} pixels differ at frame {frame}: " +
                    $"{count} pixels, bounds ({minX},{minY})..({maxX},{maxY}), " +
                    $"first ({first % FrontendFrame.Width},{first / FrontendFrame.Width}) " +
                    $"native={expected.Pixels[first]}, installed={actual.Pixels[first]}; " +
                    $"first loaded VRAM difference byte ${firstLoadVram:X4}, " +
                    $"first displayed VRAM difference byte ${firstVram:X4}; " +
                    $"VRAM pages=[{string.Join(", ", changedVramPages)}], " +
                    $"CGRAM changed colors={changedColors}.");
            }
            if (room.RoomPointer == RoomHeaderPointers.Draygon && frame == 0)
            {
                ReadOnlySpan<byte> nativeEvir = native.GameplayRuntimeForVerification.Vram.Bytes
                    .Slice(DraygonIntroPresentationDefinitions.EvirTilesVramByteAddress,
                        DraygonIntroPresentationDefinitions.EvirTilesByteCount);
                ReadOnlySpan<byte> installedEvir = installed.GameplayRuntimeForVerification.Vram.Bytes
                    .Slice(DraygonIntroPresentationDefinitions.EvirTilesVramByteAddress,
                        DraygonIntroPresentationDefinitions.EvirTilesByteCount);
                AssertTrue(installedEvir.SequenceEqual(nativeEvir),
                    "installed Draygon Evir intro upload matches native VRAM with bank-$B1 reads forbidden");
            }
        }
        if (awakenedGoldenTorizo is not null)
        {
            if (golden.ForcedGoldenSonicStart is not null)
            {
                AssertTrue(forcedGoldenSonicObserved &&
                           installed.GameState == SuperMetroidGameState.MainGameplay,
                    "forced Golden Torizo fixture executes the sonic program in active gameplay");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            if (golden.ForcedGoldenStun)
            {
                AssertTrue(forcedGoldenStunObserved, "forced Golden Torizo stun loops its tiles");
                AssertTrue(forcedGoldenStunReleased, "forced Golden Torizo stun then clears its shot lock and stun flag");
                AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
                    "forced Golden Torizo stun ends in active gameplay");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            if (golden.ForcedGoldenLeftFootOrb)
            {
                AssertTrue(goldenLeftFootOrbProgramObserved, "forced Golden Torizo runs its left-foot Chozo-orb program");
                AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
                    "forced Golden Torizo Chozo orb ends in active gameplay");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            if (golden.ForcedGoldenLeftTurnStart is not null)
            {
                AssertTrue(forcedGoldenLeftTurnObserved &&
                           (golden.ForcedGoldenLeftTurnStart !=
                                GoldenTorizoLeftTurnInstructionProgramDefinitions.Dodge ||
                            forcedGoldenFallingLeftObserved) &&
                           installed.GameState == SuperMetroidGameState.MainGameplay,
                    "forced Golden Torizo left turn reaches required poses: " +
                    $"turn={forcedGoldenLeftTurnObserved}, " +
                    $"fall={forcedGoldenFallingLeftObserved}, " +
                    $"state={installed.GameState}");
                Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            AssertTrue(goldenWakeObserved,
                "Golden Torizo wake-up fixture actually advances beyond the initial sleep");
            AssertTrue(goldenWalkingObserved && goldenRightwardObserved &&
                       goldenJumpBackObserved && goldenRightOrbObserved &&
                       goldenEyeBeamAttackObserved &&
                       installed.GameState == SuperMetroidGameState.MainGameplay,
                "Golden Torizo wake-up parity crosses walking-left and turning-right " +
                "and both orb and eye-beam attacks while gameplay remains active: " +
                $"walk={goldenWalkingObserved}, right={goldenRightwardObserved}, " +
                $"jumpBack={goldenJumpBackObserved}, rightOrb={goldenRightOrbObserved}, " +
                $"eyeBeam={goldenEyeBeamAttackObserved}, state={installed.GameState}");
        }
        Console.WriteLine($"Frontend {room.RoomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
    }

    /// <summary>
    /// A synthetic return cursor intentionally sits outside all compiled
    /// Torizo lists. A previous eager operand prefetch tried to read the
    /// nonexistent following word even though this opcode has no operand.
    /// The installed room's zero-ROM bus makes that exact mistake observable.
    /// </summary>
    private static void VerifyTorizoOperandFreeCallbackDoesNotReadNextWord(
        RoomEnemySystem enemies, RoomEnemySlot torizo)
    {
        ushort cursor = 0xf000;
        AssertTrue(enemies.BombTorizoBehavior.TryProcessBombTorizoInstruction(
                torizo, new TorizoInstructionInputs { Samus = null, Level = null, ControllerInput = 0, FrameCounterLow = 0 },
                (ushort)TorizoInstruction.Instruction_Torizo_Return, ref cursor, out _),
            "operand-free Torizo return executes without reading $AA:F002");
    }

    /// <summary>Wraps installed memory with a guard rejecting cartridge reads and marked BG2 visual streams.</summary>
    internal static MutableCartridgeReadGuard NewFrontendCartridgeReadGuard(
        ISnesAddressSpace source, HashSet<int>? blockedPresentationBytes = null)
    {
        HashSet<int> blocked = blockedPresentationBytes ?? new();
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(blocked.Contains,
                address => $"Installed frontend reread BG2 visual stream byte ${address:X6}.")
            .Forbid(address => SnesAddressMath.BankNumber(address) is not (0x7e or 0x7f) && (address & 0x8000) != 0,
                address => $"Installed frontend reread cartridge byte ${address:X6}.");
        return guard;
    }

    /// <summary>Marks native extended-frame BG2 source bytes so installed play cannot reread them.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for block frontend extended BG2 streams")]
    private static void BlockFrontendExtendedBg2Streams(HashSet<int> blockedPresentationBytes,
        ISnesAddressSpace catalogueSource, byte bank, EnemyBg2FrameDefinitionSequence frames)
    {
        foreach (EnemyBg2FrameDefinition frame in frames)
        {
            int root = ((int)new SnesAddress(bank, frame.Pointer));
            int components = catalogueSource.ReadCartridgeByte(root);
            for (int component = 0; component < components; component++)
            {
                int record = root + 2 + component * 8;
                ushort stream = CartridgeImportSource.Require(catalogueSource).ReadCartridgeWord(record + 4);
                ushort cursor = stream;
                for (int command = 0;
                     command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                {
                    int address = ((int)new SnesAddress(bank, cursor));
                    ushort first = CartridgeImportSource.Require(catalogueSource).ReadCartridgeWord(address);
                    if (command == 0)
                    {
                        AssertEqual(EnemyBg2FrameLayout.StreamMarker, first,
                            $"{frame.Name} has a native BG2 stream");
                        blockedPresentationBytes.Add(address);
                        blockedPresentationBytes.Add(address + 1);
                        cursor = unchecked((ushort)(cursor + 2));
                        continue;
                    }
                    blockedPresentationBytes.Add(address);
                    blockedPresentationBytes.Add(address + 1);
                    if (first == 0xffff)
                        break;
                    int count = CartridgeImportSource.Require(catalogueSource).ReadCartridgeWord(address + 2);
                    for (int offset = 2; offset < 4 + count * 2; offset++)
                        blockedPresentationBytes.Add(address + offset);
                    cursor = unchecked((ushort)(cursor + 4 + count * 2));
                }
            }
        }
    }

    /// <summary>Runs installed frontend parity from reset through title, options, intro, and room checks.</summary>
    [VerificationCommand("--rom-free-startup")]
    internal static int RomFreeStartupCommand()
    {
        Suite(nameof(VerifyFrontendRomFreeStartup), () => VerifyFrontendRomFreeStartup(RepositoryInstallation.Installation, RepositoryRomPath));
        return 0;
    }

    /// <summary>Runs the same startup parity with the optional configured room census.</summary>
    [VerificationCommand("--rom-free-room-census")]
    internal static int RomFreeRoomCensusCommand()
    {
        Suite(nameof(VerifyFrontendRomFreeStartup), () => VerifyFrontendRomFreeStartup(RepositoryInstallation.Installation, RepositoryRomPath, roomCensus: true));
        return 0;
    }
}

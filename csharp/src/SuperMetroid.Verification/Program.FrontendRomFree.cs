using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;
using System.Reflection;

internal static partial class Program
{
    /// <summary>
    /// Drives the actual state-zero dispatcher through the first two menus. This tests
    /// host bindings at state construction, not merely the standalone title renderer.
    /// Mutable SRAM/WRAM remain available; no cartridge byte may be consulted.
    /// </summary>
    private static void VerifyFrontendRomFreeStartup(GameInstallation installation,
        string sourceRom)
    {
        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
        // Installed gameplay gets real WRAM/SRAM but no ROM allocation at all.
        // The native reference remains available only to the development probe
        // that identifies the exact BG2 source ranges to forbid.
        var installedMemory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        AssertEqual(0, installedMemory.Rom.Length,
            "installed frontend starts with no cartridge allocation");
        var guardedBus = new FrontendCartridgeReadGuard(installedMemory, nativeBus);
        AssertThrows<InvalidOperationException>(
            () => guardedBus.ReadByte(TitleSequenceRomData.Assets.Mode7CharactersAddress),
            "startup ROM guard rejects an otherwise valid title graphics source");

        var native = new SuperMetroidGame(nativeBus);
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
                AssertTrue(actual.Pixels.AsSpan().SequenceEqual(expected.Pixels),
                    $"installed startup native pixels at frame {frame}, {actual.Phase}");

            if (actual.GameState == SuperMetroidGameState.GameOptionsMenu &&
                actual.Phase == nameof(GameOptionsPhase.Main))
            {
                AssertTrue(titleStartSent && fileSelectStartSent &&
                    visited.Contains(SuperMetroidGameState.OpeningCinematic) &&
                    visited.Contains(SuperMetroidGameState.FileSelectMenus),
                    "ROM-free startup traverses title, file select and options");
                Console.WriteLine($"Frontend ROM-free startup: {frame + 1} native-parity frames through options, all cartridge reads guarded.");
                VerifyFrontendRomFreeIntro(native, installed, guardedBus, bindInstalled);
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
    private static Action<SuperMetroidGame, bool> PrepareRomFreeBindings(GameInstallation installation)
    {
        var maps = installation.LoadMaps();
        var palettes = installation.LoadGameplayBasePalettes();
        var enemies = installation.LoadEnemyTiles();
        var objects = installation.LoadStandardObjects();
        var intro = installation.LoadIntroCinematicArt();
        var samus = installation.LoadSamusBodyArt();
        var characters = installation.LoadRoomCharacters();
        var roomPalettes = installation.LoadRoomPalettes();
        var metatiles = installation.LoadRoomMetatiles();
        var visualLayouts = installation.LoadRoomVisualLayouts();
        var backgrounds = installation.LoadRoomBackgroundTilemaps();
        var sky = installation.LoadRoomSkyTilemaps();
        var samusEaterVisuals = installation.LoadRoomPlmSamusEaterVisuals();
        InstalledProjectilePresentation projectiles = installation.LoadProjectiles();
        return (game, bindIntro) =>
        {
            game.BindMapPresentation(maps);
            game.BindCompiledRoomFxRecords(true);
            game.BindGameplayBasePalettes(palettes);
            game.BindEnemyTileArtwork(enemies);
            game.BindStandardObjectArt(objects);
            // An already-finished intro is deliberately not rebound for the
            // isolated gameplay-room census: its restored DMA side effect
            // would test a retired cinematic, not the room under examination.
            if (bindIntro)
                game.BindIntroCinematicArt(intro);
            game.BindSamusBodyArt(samus);
            game.BindRoomCharacterArt(characters);
            game.BindRoomPaletteArt(roomPalettes);
            game.BindRoomMetatileArt(metatiles);
            game.BindRoomVisualLayouts(visualLayouts);
            game.BindRoomBackgroundTilemapArt(backgrounds);
            game.BindRoomSkyTilemapArt(sky);
            game.BindRoomPlmSamusEaterVisuals(samusEaterVisuals);
            game.BindBeamArtwork(projectiles.BeamTiles);
            game.BindProjectileCompositions(projectiles.Catalog);
            game.BindProjectileFrameBindings(projectiles.FrameBindings);
            game.BindTrailArtwork(projectiles.Trails);
        };
    }

    /// <summary>
    /// Continue the same host instances through the options dispatcher, narration,
    /// and Mother Brain flashback. This catches missing live content bindings at the
    /// real transition and during the projectile/hurt animation, not just at startup.
    /// </summary>
    private static void VerifyFrontendRomFreeIntro(SuperMetroidGame native,
        SuperMetroidGame installed, FrontendCartridgeReadGuard guardedBus,
        Action<SuperMetroidGame, bool> bindInstalled)
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
            AssertTrue(actual.Pixels.AsSpan().SequenceEqual(expected.Pixels),
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
                    AssertTrue(actualGameplay.Pixels.AsSpan().SequenceEqual(expectedGameplay.Pixels),
                        $"installed post-intro pixels frame {gameplayFrame}");
                }
                VerifyFrontendRomFreeRestoredKraidHud(native, installed, bindInstalled);
                if (Environment.GetEnvironmentVariable("SM_ROM_FREE_ROOM_CENSUS") == "1")
                    VerifyFrontendRomFreeRoomCensus(native, installed, bindInstalled);
                VerifyFrontendRomFreeCeresInput(native, installed);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.CeresRidleyRoom, "Ceres Ridley");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.LandingSite, "Landing Site");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.ParlorAndAlcatraz, "Parlor");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.WestOcean, "West Ocean");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.NorfairRoom1E, "Norfair room 1E");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.GreenBrinstarMainShaft,
                    "Green Brinstar main shaft");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.BlueBrinstarElevatorRoom,
                    "Blue Brinstar elevator");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.EastTunnel, "Maridia east tunnel");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.CrateriaSaveStation, "Crateria save station");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.LowerNorfairMainHall,
                    "Lower Norfair main hall");
                // Kraid's arm switches to its rising/sinking extended frame at
                // neutral frame 654. A short room-entry check misses that draw.
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.Kraid, "Kraid arm rising/sinking",
                    frameCount: 700);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.Phantoon, "Phantoon");
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.GauntletEast, "Gauntlet east Yapping Maw", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.PreMoat, "pre-moat KiHunter", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.CrabMaze, "Crab Maze Sciser", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.Flyway, "Flyway shared fly family", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.CrateriaKagoRoom, "Crateria Kago", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.CrateriaFaceBlockRoom, "Crateria face block", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.CrateriaMorphBallEyeRoom, "Crateria Morph Ball eye",
                    frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.BrinstarShutterRoom, "Brinstar shutters", frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.TourianMetroidRoom, "Tourian ordinary Metroids",
                    frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.DeadTorizoCorpse, "Tourian dead-Torizo corpse",
                    frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.MotherBrainChamber, "Mother Brain chamber",
                    frameCount: 90);
                // The long neutral Draygon fight can fatally damage Samus;
                // all shorter room checks must execute while gameplay is live.
                guardedBus.BlockExtendedBg2Streams(
                    DraygonBg2FrameDefinitions.Bank,
                    DraygonBg2FrameDefinitions.Frames);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.Draygon, "Draygon room entry, dance and fight handoff",
                    frameCount: 2600,
                    setup: (nativeRoom, installedRoom) =>
                    {
                        // Keep the fight actor alive through the long neutral
                        // comparison without letting damage switch the frontend
                        // into a death screen before the next room is loaded.
                        var options = new SuperMetroidGameOptions { Invincibility = true };
                        nativeRoom.RuntimeForVerification!.ApplyHostOptions(options);
                        installedRoom.RuntimeForVerification!.ApplyHostOptions(options);
                    });
                // Cross the wake/combat handoff into the first linked walking
                // lists. The older 700-frame low-health check reached Game Over
                // and its trailing pixel matches did not exercise the room.
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.GoldenTorizo, "Golden Torizo wake and right-facing attacks",
                    frameCount: 850,
                    setup: (nativeRoom, installedRoom) =>
                    {
                        // Enter the authored lower-right wake rectangle on both
                        // independently stepped games, without crossing a door.
                        nativeRoom.RuntimeForVerification!.Samus!.XPosition = 0x0180;
                        nativeRoom.RuntimeForVerification.Samus.YPosition = 0x0150;
                        installedRoom.RuntimeForVerification!.Samus!.XPosition = 0x0180;
                        installedRoom.RuntimeForVerification.Samus.YPosition = 0x0150;
                        var options = new SuperMetroidGameOptions { Invincibility = false };
                        nativeRoom.RuntimeForVerification.ApplyHostOptions(options);
                        installedRoom.RuntimeForVerification.ApplyHostOptions(options);
                    });
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.GoldenTorizo, "Golden Torizo left-foot right-sonic program",
                    frameCount: 90,
                    setup: (nativeRoom, installedRoom) =>
                    {
                        foreach (SuperMetroidGame game in new[] { nativeRoom, installedRoom })
                        {
                            RoomEnemySlot boss = game.RuntimeForVerification!.Enemies.Slots.Single(
                                slot => slot.EnemyDefinitionPointer == RoomEnemySystem.GoldenTorizoDefinition);
                            boss.CurrentInstruction = GoldenTorizoRightSonicInstructionProgramDefinitions.Start;
                            boss.InstructionTimer = 1;
                        }
                    },
                    forcedGoldenSonicStart:
                        GoldenTorizoRightSonicInstructionProgramDefinitions.Start);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.GoldenTorizo, "Golden Torizo right-foot right-sonic program",
                    frameCount: 90,
                    setup: (nativeRoom, installedRoom) =>
                    {
                        foreach (SuperMetroidGame game in new[] { nativeRoom, installedRoom })
                        {
                            RoomEnemySlot boss = game.RuntimeForVerification!.Enemies.Slots.Single(
                                slot => slot.EnemyDefinitionPointer == RoomEnemySystem.GoldenTorizoDefinition);
                            boss.CurrentInstruction =
                                GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward;
                            boss.InstructionTimer = 1;
                        }
                    },
                    forcedGoldenSonicStart:
                        GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward);
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
        bindInstalled(installedRoom, false);
        nativeRoom.RuntimeForVerification!.LoadCartridgeRoomForDebug(RoomHeaderPointers.Kraid);
        installedRoom.RuntimeForVerification!.LoadCartridgeRoomForDebug(RoomHeaderPointers.Kraid);
        AssertEqual(RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord,
            installedRoom.RuntimeForVerification.GameplayHudCharacterBaseWord,
            "restored Kraid room selects the relocated HUD character base");
        int bg2Byte = KraidBackgroundRomData.LiveBg2TilemapWord * sizeof(ushort);
        int bg2Bytes = KraidBackgroundRomData.WorkingTilemapWords * sizeof(ushort);
        AssertTrue(nativeRoom.RuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)
                .SequenceEqual(installedRoom.RuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)),
            "restored Kraid BG2 pages match before the first accepted NMI");
        FrontendFrame expected = nativeRoom.Step(0);
        FrontendFrame actual = installedRoom.Step(0);
        AssertTrue(nativeRoom.RuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)
                .SequenceEqual(installedRoom.RuntimeForVerification.Vram.Bytes.Slice(bg2Byte, bg2Bytes)),
            "restored Kraid BG2 pages remain intact after the deferred HUD refresh");
        AssertTrue(actual.Pixels.AsSpan().SequenceEqual(expected.Pixels),
            "restored Kraid first frame matches native pixels after HUD artwork rebind");
        Console.WriteLine("Frontend ROM-free Kraid restore: relocated HUD refresh preserves both BG2 pages and stock pixels.");
    }

    /// <summary>
    /// Exercise actual Samus movement and a beam shot after the cinematic handoff.
    /// A neutral-only window cannot expose projectile/pose presentation reads.
    /// </summary>
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
            if (!actual.Pixels.AsSpan().SequenceEqual(expected.Pixels))
            {
                int first = 0;
                while (actual.Pixels[first].Equals(expected.Pixels[first]))
                    first++;
                var nativeSamus = native.RuntimeForVerification!.Samus!;
                var installedSamus = installed.RuntimeForVerification!.Samus!;
                int firstVram = 0;
                ReadOnlySpan<byte> nativeVram = native.RuntimeForVerification.Vram.Bytes;
                ReadOnlySpan<byte> installedVram = installed.RuntimeForVerification.Vram.Bytes;
                while (firstVram < nativeVram.Length && nativeVram[firstVram] == installedVram[firstVram])
                    firstVram++;
                int firstOam = 0;
                ReadOnlySpan<byte> nativeOam = native.RuntimeForVerification.DisplayedOam.LowTable;
                ReadOnlySpan<byte> installedOam = installed.RuntimeForVerification.DisplayedOam.LowTable;
                while (firstOam < nativeOam.Length && nativeOam[firstOam] == installedOam[firstOam])
                    firstOam++;
                throw new InvalidOperationException(
                    $"Installed Ceres input pixels differ at frame {frame}, pixel " +
                    $"({first % FrontendFrame.Width},{first / FrontendFrame.Width}): " +
                    $"native={expected.Pixels[first]}, installed={actual.Pixels[first]}; " +
                    $"Samus native=({native.GameplaySamusX},{native.GameplaySamusY}) " +
                    $"pose ${native.GameplaySamusPose:X2}/frame {nativeSamus.AnimationFrame}/" +
                    $"top {nativeSamus.TopSpritemapIndex}/bottom {nativeSamus.BottomSpritemapIndex}, installed=" +
                    $"({installed.GameplaySamusX},{installed.GameplaySamusY}) " +
                    $"pose ${installed.GameplaySamusPose:X2}/frame {installedSamus.AnimationFrame}/" +
                    $"top {installedSamus.TopSpritemapIndex}/bottom {installedSamus.BottomSpritemapIndex}; " +
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
    private static void VerifyFrontendRomFreeRoom(
        SuperMetroidGame native, SuperMetroidGame installed,
        ushort roomPointer, string roomName, int frameCount = 90,
        Action<SuperMetroidGame, SuperMetroidGame>? setup = null,
        ushort? forcedGoldenSonicStart = null)
    {
        native.RuntimeForVerification!.LoadCartridgeRoomForDebug(
            roomPointer);
        installed.RuntimeForVerification!.LoadCartridgeRoomForDebug(
            roomPointer);
        AssertEqual(SuperMetroidGameState.MainGameplay, native.GameState,
            $"native {roomName} fixture begins in active gameplay");
        AssertEqual(SuperMetroidGameState.MainGameplay, installed.GameState,
            $"installed {roomName} fixture begins in active gameplay");
        // These isolated rooms are not one continuous playthrough. Refill on
        // entry so damage in an earlier room cannot turn a later comparison
        // into two identical death screens.
        native.RuntimeForVerification.Samus!.MaxHealth = 1499;
        native.RuntimeForVerification.Samus.Health = 1499;
        installed.RuntimeForVerification.Samus!.MaxHealth = 1499;
        installed.RuntimeForVerification.Samus.Health = 1499;
        setup?.Invoke(native, installed);
        RoomEnemySlot? awakenedGoldenTorizo = roomPointer == RoomHeaderPointers.GoldenTorizo &&
            setup is not null
            ? installed.RuntimeForVerification.Enemies.Slots.FirstOrDefault(slot =>
                slot.EnemyDefinitionPointer == RoomEnemySystem.GoldenTorizoDefinition)
            : null;
        if (roomPointer == RoomHeaderPointers.GoldenTorizo && setup is not null)
        {
            AssertTrue(awakenedGoldenTorizo is not null,
                "Golden Torizo wake-up fixture loaded its live boss slot");
            VerifyTorizoOperandFreeCallbackDoesNotReadNextWord(
                installed.RuntimeForVerification.Enemies, awakenedGoldenTorizo!);
        }
        bool goldenWakeObserved = false;
        bool goldenWalkingObserved = false;
        bool goldenRightwardObserved = false;
        bool goldenJumpBackObserved = false;
        bool goldenRightOrbObserved = false;
        bool goldenEyeBeamAttackObserved = false;
        bool forcedGoldenSonicObserved = false;
        ReadOnlySpan<byte> nativeLoadedVram = native.RuntimeForVerification.Vram.Bytes;
        ReadOnlySpan<byte> installedLoadedVram = installed.RuntimeForVerification.Vram.Bytes;
        int firstLoadVram = 0;
        while (firstLoadVram < nativeLoadedVram.Length &&
               nativeLoadedVram[firstLoadVram] == installedLoadedVram[firstLoadVram])
            firstLoadVram++;
        for (int frame = 0; frame < frameCount; frame++)
        {
            FrontendFrame expected = native.Step(0);
            FrontendFrame actual;
            try
            {
                actual = installed.Step(0);
            }
            catch (Exception error)
            {
                throw new InvalidOperationException(
                    $"Installed {roomName} failed at neutral frame {frame}.", error);
            }
            AssertEqual(roomPointer,
                installed.GameplayActiveRoomPointer,
                $"installed {roomName} fixture remains in one room at frame {frame}");
            AssertEqual(expected.GameState, actual.GameState,
                $"installed {roomName} game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed {roomName} phase at frame {frame}");
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
                forcedGoldenSonicStart is ushort sonicStart &&
                awakenedGoldenTorizo.CurrentInstruction >= sonicStart &&
                awakenedGoldenTorizo.CurrentInstruction <
                    (sonicStart == GoldenTorizoRightSonicInstructionProgramDefinitions.Start
                        ? GoldenTorizoRightSonicInstructionProgramDefinitions.RightFootForward
                        : GoldenTorizoRightSonicInstructionProgramDefinitions.End))
                forcedGoldenSonicObserved = true;
            if (!actual.Pixels.AsSpan().SequenceEqual(expected.Pixels))
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
                ReadOnlySpan<byte> nativeVram = native.RuntimeForVerification!.Vram.Bytes;
                ReadOnlySpan<byte> installedVram = installed.RuntimeForVerification!.Vram.Bytes;
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
                ReadOnlySpan<ushort> nativeColors = native.RuntimeForVerification.Cgram.Colors;
                ReadOnlySpan<ushort> installedColors = installed.RuntimeForVerification.Cgram.Colors;
                int changedColors = 0;
                for (int color = 0; color < nativeColors.Length; color++)
                    if (nativeColors[color] != installedColors[color]) changedColors++;
                if (Environment.GetEnvironmentVariable("SM_ROM_FREE_CENSUS_ROOM") is not null)
                {
                    string output = Path.GetFullPath(Path.Combine("csharp", "test-temp",
                        "rom-free-room-census-compare"));
                    Directory.CreateDirectory(output);
                    PngWriter.WriteRgba(Path.Combine(output, $"{roomPointer:X4}-native.png"),
                        FrontendFrame.Width, FrontendFrame.Height, expected.Pixels);
                    PngWriter.WriteRgba(Path.Combine(output, $"{roomPointer:X4}-installed.png"),
                        FrontendFrame.Width, FrontendFrame.Height, actual.Pixels);
                }
                throw new InvalidOperationException(
                    $"Installed {roomName} pixels differ at frame {frame}: " +
                    $"{count} pixels, bounds ({minX},{minY})..({maxX},{maxY}), " +
                    $"first ({first % FrontendFrame.Width},{first / FrontendFrame.Width}) " +
                    $"native={expected.Pixels[first]}, installed={actual.Pixels[first]}; " +
                    $"first loaded VRAM difference byte ${firstLoadVram:X4}, " +
                    $"first displayed VRAM difference byte ${firstVram:X4}; " +
                    $"VRAM pages=[{string.Join(", ", changedVramPages)}], " +
                    $"CGRAM changed colors={changedColors}.");
            }
            if (roomPointer == RoomHeaderPointers.Draygon && frame == 0)
            {
                ReadOnlySpan<byte> nativeEvir = native.RuntimeForVerification!.Vram.Bytes
                    .Slice(DraygonIntroPresentationDefinitions.EvirTilesVramByteAddress,
                        DraygonIntroPresentationDefinitions.EvirTilesByteCount);
                ReadOnlySpan<byte> installedEvir = installed.RuntimeForVerification!.Vram.Bytes
                    .Slice(DraygonIntroPresentationDefinitions.EvirTilesVramByteAddress,
                        DraygonIntroPresentationDefinitions.EvirTilesByteCount);
                AssertTrue(installedEvir.SequenceEqual(nativeEvir),
                    "installed Draygon Evir intro upload matches native VRAM with bank-$B1 reads forbidden");
            }
        }
        if (awakenedGoldenTorizo is not null)
        {
            if (forcedGoldenSonicStart is not null)
            {
                AssertTrue(forcedGoldenSonicObserved &&
                           installed.GameState == SuperMetroidGameState.MainGameplay,
                    "forced Golden Torizo fixture executes the sonic program in active gameplay");
                Console.WriteLine($"Frontend {roomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
                return;
            }
            AssertTrue(goldenWakeObserved,
                "Golden Torizo wake-up fixture actually advances beyond the initial sleep");
            AssertTrue(goldenWalkingObserved && goldenRightwardObserved &&
                       goldenJumpBackObserved && goldenRightOrbObserved &&
                       goldenEyeBeamAttackObserved &&
                       installed.GameState == SuperMetroidGameState.MainGameplay,
                "Golden Torizo wake-up parity crosses walking-left and turning-right " +
                "and both orb and eye-beam attacks while gameplay remains active");
        }
        Console.WriteLine($"Frontend {roomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
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
        MethodInfo callback = typeof(RoomEnemySystem).GetMethod(
            "TryProcessBombTorizoInstruction",
            BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Torizo callback dispatcher was not found.");
        object?[] arguments =
        [
            torizo, null, null, TorizoInstructionCodes.Instruction_Torizo_Return,
            (ushort)0xf000, (ushort)0, (byte)0, false,
        ];
        AssertTrue(callback.Invoke(enemies, arguments) is true,
            "operand-free Torizo return executes without reading $AA:F002");
    }

    private sealed class FrontendCartridgeReadGuard(
        ISnesAddressSpace source, ISnesAddressSpace? lookupSource = null) :
        ISnesAddressSpace
    {
        private readonly HashSet<int> blockedPresentationBytes = [];

        internal void BlockExtendedBg2Streams(byte bank,
            ReadOnlySpan<EnemyBg2FrameDefinition> frames)
        {
            ISnesAddressSpace catalogueSource = lookupSource ?? source;
            foreach (EnemyBg2FrameDefinition frame in frames)
            {
                int root = (bank << 16) | frame.Pointer;
                int components = catalogueSource.ReadByte(root);
                for (int component = 0; component < components; component++)
                {
                    int record = root + 2 + component * 8;
                    ushort stream = (ushort)(catalogueSource.ReadByte(record + 4) |
                        catalogueSource.ReadByte(record + 5) << 8);
                    ushort cursor = stream;
                    for (int command = 0;
                         command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                    {
                        int address = (bank << 16) | cursor;
                        ushort first = (ushort)(catalogueSource.ReadByte(address) |
                            catalogueSource.ReadByte(address + 1) << 8);
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
                        int count = catalogueSource.ReadByte(address + 2) |
                            catalogueSource.ReadByte(address + 3) << 8;
                        for (int offset = 2; offset < 4 + count * 2; offset++)
                            blockedPresentationBytes.Add(address + offset);
                        cursor = unchecked((ushort)(cursor + 4 + count * 2));
                    }
                }
            }
        }

        public byte ReadByte(int address)
        {
            if (blockedPresentationBytes.Contains(address))
                throw new InvalidOperationException(
                    $"Installed frontend reread BG2 visual stream byte ${address:X6}.");
            int bank = address >> 16;
            if (bank is not (0x7e or 0x7f) &&
                (address & 0x8000) != 0)
                throw new InvalidOperationException(
                    $"Installed frontend reread cartridge byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

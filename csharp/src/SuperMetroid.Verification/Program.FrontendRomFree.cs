using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

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
        var guardedBus = new FrontendCartridgeReadGuard(
            SuperMetroidAddressSpace.LoadRetailRom(sourceRom));
        AssertThrows<InvalidOperationException>(
            () => guardedBus.ReadByte(TitleSequenceRomData.Assets.Mode7CharactersAddress),
            "startup ROM guard rejects an otherwise valid title graphics source");

        var native = new SuperMetroidGame(nativeBus);
        var installed = new SuperMetroidGame(guardedBus);
        installed.BindMapPresentation(installation.LoadMaps());
        installed.BindCompiledRoomFxRecords(true);
        installed.BindGameplayBasePalettes(installation.LoadGameplayBasePalettes());
        installed.BindEnemyTileArtwork(installation.LoadEnemyTiles());
        installed.BindStandardObjectArt(installation.LoadStandardObjects());
        installed.BindIntroCinematicArt(installation.LoadIntroCinematicArt());
        installed.BindSamusBodyArt(installation.LoadSamusBodyArt());
        installed.BindRoomCharacterArt(installation.LoadRoomCharacters());
        installed.BindRoomPaletteArt(installation.LoadRoomPalettes());
        installed.BindRoomMetatileArt(installation.LoadRoomMetatiles());
        installed.BindRoomVisualLayouts(installation.LoadRoomVisualLayouts());
        installed.BindRoomBackgroundTilemapArt(installation.LoadRoomBackgroundTilemaps());
        installed.BindRoomSkyTilemapArt(installation.LoadRoomSkyTilemaps());
        InstalledProjectilePresentation projectiles = installation.LoadProjectiles();
        installed.BindBeamArtwork(projectiles.BeamTiles);
        installed.BindProjectileCompositions(projectiles.Catalog);
        installed.BindProjectileFrameBindings(projectiles.FrameBindings);
        installed.BindTrailArtwork(projectiles.Trails);
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
                VerifyFrontendRomFreeIntro(native, installed, guardedBus);
                return;
            }
        }
        throw new InvalidOperationException(
            "ROM-free startup fixture did not reach the options main menu.");
    }

    /// <summary>
    /// Continue the same host instances through the options dispatcher, narration,
    /// and Mother Brain flashback. This catches missing live content bindings at the
    /// real transition and during the projectile/hurt animation, not just at startup.
    /// </summary>
    private static void VerifyFrontendRomFreeIntro(SuperMetroidGame native,
        SuperMetroidGame installed, FrontendCartridgeReadGuard guardedBus)
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
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.Phantoon, "Phantoon");
                // The previous room fixture exempted all bank-$A5 reads while
                // Draygon's extended hitboxes were still ROM-backed. Both its
                // BG2 and ordinary-frame collision are now compiled. Require
                // the same all-bank read guard used by every other room.
                guardedBus.BlockExtendedBg2Streams(
                    DraygonBg2FrameDefinitions.Bank,
                    DraygonBg2FrameDefinitions.Frames);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.Draygon, "Draygon room entry, dance and fight handoff",
                    frameCount: 2600);
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
                    RoomHeaderPointers.DeadTorizoCorpse, "Tourian dead-Torizo corpse",
                    frameCount: 90);
                VerifyFrontendRomFreeRoom(native, installed,
                    RoomHeaderPointers.MotherBrainChamber, "Mother Brain chamber",
                    frameCount: 90);
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
        ushort roomPointer, string roomName, int frameCount = 90)
    {
        native.RuntimeForVerification!.LoadCartridgeRoomForDebug(
            roomPointer);
        installed.RuntimeForVerification!.LoadCartridgeRoomForDebug(
            roomPointer);
        for (int frame = 0; frame < frameCount; frame++)
        {
            FrontendFrame expected = native.Step(0);
            FrontendFrame actual = installed.Step(0);
            AssertEqual(roomPointer,
                installed.GameplayActiveRoomPointer,
                $"installed {roomName} fixture remains in one room at frame {frame}");
            AssertEqual(expected.GameState, actual.GameState,
                $"installed {roomName} game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed {roomName} phase at frame {frame}");
            AssertTrue(actual.Pixels.AsSpan().SequenceEqual(expected.Pixels),
                $"installed {roomName} pixels at frame {frame}");
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
        Console.WriteLine($"Frontend {roomName} room: {frameCount} native-parity frames; all cartridge reads guarded.");
    }

    private sealed class FrontendCartridgeReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        private readonly HashSet<int> blockedPresentationBytes = [];

        internal void BlockExtendedBg2Streams(byte bank,
            ReadOnlySpan<EnemyBg2FrameDefinition> frames)
        {
            foreach (EnemyBg2FrameDefinition frame in frames)
            {
                int root = (bank << 16) | frame.Pointer;
                int components = source.ReadByte(root);
                for (int component = 0; component < components; component++)
                {
                    int record = root + 2 + component * 8;
                    ushort stream = (ushort)(source.ReadByte(record + 4) |
                        source.ReadByte(record + 5) << 8);
                    ushort cursor = stream;
                    for (int command = 0;
                         command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
                    {
                        int address = (bank << 16) | cursor;
                        ushort first = (ushort)(source.ReadByte(address) |
                            source.ReadByte(address + 1) << 8);
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
                        int count = source.ReadByte(address + 2) |
                            source.ReadByte(address + 3) << 8;
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

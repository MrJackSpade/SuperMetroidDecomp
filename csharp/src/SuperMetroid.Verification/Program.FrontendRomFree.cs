using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

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
        installed.BindStandardObjectArt(installation.LoadStandardObjects());
        installed.BindIntroCinematicArt(installation.LoadIntroCinematicArt());
        installed.BindSamusBodyArt(installation.LoadSamusBodyArt());
        installed.BindRoomCharacterArt(installation.LoadRoomCharacters());
        installed.BindRoomPaletteArt(installation.LoadRoomPalettes());
        installed.BindRoomMetatileArt(installation.LoadRoomMetatiles());
        installed.BindRoomVisualLayouts(installation.LoadRoomVisualLayouts());
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
                VerifyFrontendRomFreeIntro(native, installed);
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
        SuperMetroidGame installed)
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
                Console.WriteLine($"Frontend ROM-free intro: {frame + 1} native-parity frames through game-state handoff; all cartridge reads guarded.");
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

    private sealed class FrontendCartridgeReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            int bank = address >> 16;
            if (bank is not (0x7e or 0x7f) && (address & 0x8000) != 0)
                throw new InvalidOperationException(
                    $"Installed frontend reread cartridge byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}

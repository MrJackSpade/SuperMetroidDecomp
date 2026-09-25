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
        installed.BindIntroCinematicArt(installation.LoadIntroCinematicArt());
        installed.BindBeamArtwork(installation.LoadProjectiles().BeamTiles);
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
    /// Continue the same host instances through the options dispatcher and opening
    /// narration. This catches a missing installation binding at the real transition.
    /// </summary>
    private static void VerifyFrontendRomFreeIntro(SuperMetroidGame native,
        SuperMetroidGame installed)
    {
        bool startSent = false;
        int introFrames = 0;
        for (int frame = 0; frame < 2500; frame++)
        {
            FrontendFrame before = installed.CurrentFrameMetadata;
            ushort input = !startSent ? (ushort)SnesButton.Start : (ushort)0;
            startSent = true;
            FrontendFrame expected = native.Step(input);
            FrontendFrame actual = installed.Step(input);
            AssertEqual(expected.GameState, actual.GameState,
                $"installed intro game state at frame {frame}");
            AssertEqual(expected.Phase, actual.Phase,
                $"installed intro native phase at frame {frame}");
            if (frame % 37 == 0 || actual.GameState != before.GameState)
                AssertTrue(actual.Pixels.AsSpan().SequenceEqual(expected.Pixels),
                    $"installed intro native pixels at frame {frame}, {actual.Phase}");
            if (actual.GameState != SuperMetroidGameState.IntroCinematic)
                continue;
            introFrames++;
            if (introFrames < 1500)
                continue;
            Console.WriteLine($"Frontend ROM-free intro: {frame + 1} native-parity frames, including {introFrames} cinematic frames; all cartridge reads guarded.");
            return;
        }
        throw new InvalidOperationException(
            "ROM-free frontend fixture did not reach 1500 opening-cinematic frames.");
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

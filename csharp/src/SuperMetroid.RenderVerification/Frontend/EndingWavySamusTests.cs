using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.FrameRendering;

/// <summary>Checks the post-credits Samus wave's description (#1468), its visible color addition, and repeatable rendering.</summary>
internal static class EndingWavySamusTests
{
    /// <summary>
    /// Checks all 180 wave frames' described wave and vertical scroll (#1468): its parameters, and that the wave the shader
    /// evaluates gives every line the cartridge's scroll; and that selected frames visibly add and move the wave.
    /// </summary>
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var installed = RepositoryInstallation.Installation;
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var maps = installed.LoadMaps();
        var scene = new EndingCreditsState(new AudioQueues(), new EndingCreditsSetup { GameTimeHours = 2, GameTimeMinutes = 0 });
        scene.InstalledArtwork.BindFixtureObjectArtwork(installed.LoadEndingObjectArt());
        scene.InstalledArtwork.BindPaletteArtwork(installed.LoadEndingPalettes());
        scene.InstalledArtwork.BindPaletteFxColors(maps.RoomPaletteFx);
        scene.Stage.BindEndingFont(maps.EndingFont);
        scene.InstalledArtwork.BindEndingText(maps.EndingText);
        scene.InstalledArtwork.LoadCreditsAndPostCreditsAssets();
        scene.CreditsRoll.SetupPostCreditsBlank();
        scene.Stage.Phase = EndingCreditsPhase.PostCreditsShootingStars;
        scene.Stage.PhaseTimer = 1;
        scene.Stage.Brightness = 15;
        Require(!EndingCreditsPresentation.CaptureRenderSnapshot(scene).LayersOf<AddedPicturedPlaneLayer>().Any(), "Wave must not start during the palette fade");
        scene.Step();
        RgbaPixel[]? previous = null;
        for (int age = 0; age < 180; age++)
        {
            Require(scene.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop, "Native wave lasts exactly the 180-frame hold");
            var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
            var added = snapshot.LayersOf<AddedPicturedPlaneLayer>().Single();
            Require(added.VerticalScroll == age * 2, $"Native vertical scroll age {age}");
            if (age < 2) Require(added.HorizontalWave is null, $"The setup-only frames show no wave, age {age}");
            else
            {
                ScrollWave wave = added.HorizontalWave ?? throw new InvalidDataException($"The wave shows from age 2, age {age}");
                Require(wave.BaseScroll == 0 && wave.Amplitude == 0x4000 && wave.Period == 128, $"Wave base, amplitude and period, age {age}");
                Require(wave.FirstLine == 0 && wave.EndLine == 224, $"Wave band, age {age}");
                Require(wave.Phase.RawValue == (byte)((-2 + (age - 1) * 16) >> 1), $"Wave phase advances eight sine steps a frame, age {age}");
                for (int line = 0; line < 224; line++)
                    Require(ScrollShapeLines.WaveScroll(wave, line) == NativeX(age, line), $"Described wave gives native HDMA X age {age}, line {line}");
            }
            if (age is 0 or 1 or 2 or 3 or 179)
            {
                // The same frame without the wave's addition shows what the addition changes.
                DisplayLayer[] unwaved = [.. snapshot.Layers.ToArray().Where(layer => layer is not AddedPicturedPlaneLayer)];
                RgbaPixel[] withoutWave = DisplayRendering.Render(new LayeredScene(snapshot.Paint, unwaved, snapshot.Brightness));
                RgbaPixel[] expected = DisplayRendering.Render(snapshot);
                Require(!expected.SequenceEqual(withoutWave), "Native artwork must visibly add the blue transformation effect");
                var packet = new DisplayPacket(new(age + 1, 1, (ushort)age), snapshot);
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "Ending wave frame on this device matches the WARP reference device");
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "Repeated wave rendering does not advance simulation");
                if (age == 3) Require(previous is not null && !expected.SequenceEqual(previous), "Wave changes visible pixels between consecutive frames");
                previous = expected;
            }
            scene.Step();
        }
        Require(scene.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingSamus, "Result panel replaces the wave after its wait");
        Require(!EndingCreditsPresentation.CaptureRenderSnapshot(scene).LayersOf<AddedPicturedPlaneLayer>().Any(), "Result panel disables BG3 addition");
        Console.WriteLine($"{device.Kind}: ending wave description and native line scrolls, moving blue pixels and 180-frame lifecycle passed.");

        ushort NativeX(int age, int line)
        {
            if (age < 2) return 0;
            int offset = (-2 + (age - 1) * 16 + (line % 64) * 4) & 0x1ff;
            short sample = NativeWaveSample(rom, offset);
            int displacement = ScrollShapeLines.TimesSixteenSixteen(Math.Abs((int)sample), 0x4000);
            if (sample < 0) displacement = -displacement;
            if ((line / 64 & 1) != 0) displacement = -displacement;
            return unchecked((ushort)displacement);
        }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
    }

    /// <summary>Reads a signed native sine sample used by the independent wave-scroll calculation.</summary>
    private static short NativeWaveSample(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, int offset) =>
        unchecked((short)rom.ReadCartridgeWord(0xa0b443 + offset));
}

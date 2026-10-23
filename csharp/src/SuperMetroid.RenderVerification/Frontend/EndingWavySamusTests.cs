using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.RenderVerification.Frontend;

using SuperMetroid.TestSupport.FrameCapture;
using SuperMetroid.TestSupport.RepositoryInstallation;

/// <summary>Checks post-credits Samus wave HDMA, color addition, and serialized rendering.</summary>
internal static class EndingWavySamusTests
{
    /// <summary>Compares all 180 wave frames with native scroll math and selected frames with independent pixels.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native VRAM words")]
    internal static void Run(D3D11RenderDevice device, D3D11FrameRenderer renderer)
    {
        var installed = RepositoryInstallation.Installation;
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var maps = installed.LoadMaps();
        var scene = new EndingCreditsState(new CartridgeAudioState(), new EndingCreditsSetup { GameTimeHours = 2, GameTimeMinutes = 0 });
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
        Require(!EndingCreditsPresentation.CaptureRenderSnapshot(scene).LayersOf<BgSubscreenAddRenderLayer>().Any(), "Wave must not start during the palette fade");
        scene.Step();
        Rgba32[]? previous = null;
        for (int age = 0; age < 180; age++)
        {
            Require(scene.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingBackdrop, "Native wave lasts exactly the 180-frame hold");
            var snapshot = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
            byte[] snapshotVram = scene.ShownVram();
            var wave = snapshot.LayersOf<BgSubscreenAddRenderLayer>().Single();
            for (int line = 0; line < 224; line++)
            {
                Require(wave.Scrolls[line].X == NativeX(age, line), $"Native HDMA X age {age}, line {line}");
                Require(wave.Scrolls[line].Y == age * 2, $"Native vertical scroll age {age}");
            }
            if (age is 0 or 1 or 2 or 3 or 179)
            {
                var vram = new SnesVram(); vram.LoadBytes(0, snapshotVram);
                var cgram = new SnesCgram();
                for (int i = 0; i < 256; i++) cgram.SetColor(i, snapshot.Paint.Colors[i]);
                var main = SnesBgTilemapRenderer.Render4BppViewport(vram, cgram, new BgVramAddresses { TilemapBaseWord = 0x4c00, CharacterBaseWord = 0x5000 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = new Extent16<ScreenPixels>(256, 224), TilemapSize = new Extent16<TilemapCells>(32, 32) });
                var sub = SnesBgTilemapRenderer.Render2Bpp(vram, cgram, 0x2400, 0x2000, rowCount: 32, transparentColorZero: true);
                var expected = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
                SnesLayerCompositor.Composite(expected, SnesObjRenderer.Render(snapshot.Paint.SpriteDraws, cgram));
                int changed = 0;
                for (int y = 0; y < 224; y++)
                for (int x = 0; x < 256; x++)
                {
                    int index = y * 256 + x;
                    if (main[index].A == 0) continue; // CGADSUB enables BG2, not backdrop.
                    Rgba32 first = main[index], second = sub[unchecked((byte)(y + age * 2)) * 256 + unchecked((byte)(x + NativeX(age, y)))];
                    expected[index] = second.A == 0 ? first : new(Add(first.R, second.R), Add(first.G, second.G), Add(first.B, second.B));
                    if (expected[index] != first) changed++;
                }
                Require(changed > 0, "Native artwork must visibly add the blue transformation effect");
                var packet = new RenderFrameSnapshot(new(age + 1, 1, (ushort)age), snapshot);
                PixelComparison.Verify(packet, expected, scene.Render(), "Ending direct render matches native BG3 addition");
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "Ending wave frame on the GPU matches native BG3 addition");
                PixelComparison.Verify(packet, expected, renderer.RenderForReadback(packet), "Repeated wave rendering does not advance simulation");
                if (age == 0)
                {
                    var supplied = wave.Scrolls.ToArray();
                    var owned = wave.WithScrolls(supplied);
                    supplied[0] = new(123, 123);
                    Require(owned.Scrolls[0] == new Position16<BgPlanePixels>(0, 0), "Captured line scrolls own their input memory");
                }
                if (age == 3) Require(previous is not null && !expected.SequenceEqual(previous), "Wave changes visible pixels between consecutive frames");
                previous = expected;
            }
            scene.Step();
        }
        Require(scene.Stage.Phase == EndingCreditsPhase.PostCreditsWaitingSamus, "Result panel replaces the wave after its wait");
        Require(!EndingCreditsPresentation.CaptureRenderSnapshot(scene).LayersOf<BgSubscreenAddRenderLayer>().Any(), "Result panel disables BG3 addition");
        Console.WriteLine($"{device.Kind}: ending wave native scrolls, moving blue pixels, BG2-only addition and 180-frame lifecycle passed.");

        ushort NativeX(int age, int line)
        {
            if (age < 2) return 0;
            int offset = (-2 + (age - 1) * 16 + (line % 64) * 4) & 0x1ff;
            short sample = NativeWaveSample(rom, offset);
            int displacement = SnesSixteenSixteen.Multiply(Math.Abs((int)sample), 0x4000);
            if (sample < 0) displacement = -displacement;
            if ((line / 64 & 1) != 0) displacement = -displacement;
            return unchecked((ushort)displacement);
        }
        static byte Add(byte first, byte second) { int value = Math.Min(31, (first >> 3) + (second >> 3)); return (byte)((value << 3) | (value >> 2)); }
        static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
    }

    /// <summary>Reads a signed native sine sample used by the independent wave-scroll calculation.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the ending wave sine table at $A0:B443")]
    private static short NativeWaveSample(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace rom, int offset) =>
        unchecked((short)rom.ReadCartridgeWord(0xa0b443 + offset));
}

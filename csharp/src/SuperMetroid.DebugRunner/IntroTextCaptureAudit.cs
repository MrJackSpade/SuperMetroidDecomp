using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

/// <summary>Native-resolution first illustrated narration evidence; no sharpening or palette adjustment.</summary>
internal static class IntroTextCaptureAudit
{
    public static int Run(string rom, string directory)
    {
        Directory.CreateDirectory(directory);
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var intro = new IntroCinematicState(bus);
        for (int frame = 0; frame < 8192; frame++)
        {
            intro.Step(0);
            if (intro.Phase != IntroCinematicPhase.PageOneText || intro.IntroCaretY != 40) continue;
            var snapshot = intro.CaptureTranslatedRenderSnapshot();
            var pixels = SoftwareLayeredSnapshotRenderer.Render(snapshot);
            PngWriter.WriteRgba(Path.Combine(directory, "first-battled.png"), 256, 224, pixels);
            File.WriteAllBytes(Path.Combine(directory, "first-battled.smframe"),
                RenderFrameSnapshotCodec.Serialize(new(new(frame, 1, (ushort)frame), snapshot)));
            Console.WriteLine($"First illustrated narration: frame {frame}, caret ({intro.IntroCaretX},{intro.IntroCaretY}).");
            for (int i = 0; i < 32; i++)
            {
                int address = IntroCinematicRomData.Assets.Palette + i * 2;
                ushort source = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                Console.WriteLine($"CGRAM {i:D2}: displayed {snapshot.Memory.Cgram[i]:X4}, ROM {source:X4}");
            }
            return 0;
        }
        throw new InvalidDataException("Intro did not reach the first illustrated narration's second row.");
    }
}

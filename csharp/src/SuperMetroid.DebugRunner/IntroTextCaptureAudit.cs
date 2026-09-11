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
            int firstGlyph = IntroCinematicRomData.Vram.NarrationTilemapDestinationByte + (4 * 32 + 1) * 2;
            ushort glyphWord = (ushort)(snapshot.Memory.Vram[firstGlyph] | snapshot.Memory.Vram[firstGlyph + 1] << 8);
            if (((glyphWord >> 10) & 7) != 3)
                throw new InvalidDataException($"Mature first narration glyph retains palette {(glyphWord >> 10) & 7}; native glow must have reached palette 3.");
            var greens = new HashSet<byte>();
            for (int y = 24; y < 32; y++)
            for (int x = 8; x < 16; x++)
            {
                var pixel = pixels[y * 256 + x];
                if (pixel.R == 0 && pixel.B == 0 && pixel.G != 0) greens.Add(pixel.G);
            }
            if (!greens.SetEquals(new byte[] { 90, 255 }))
                throw new InvalidDataException("Rendered mature glyph lacks its native green-11 outline and green-31 interior.");
            Console.WriteLine("Mature glyph pixels: green-11 outline (90), green-31 interior (255).");
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

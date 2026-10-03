using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static byte[] OriginalCeresHazeRamp(ISnesAddressSpace rom, int counter)
    {
        // Execute the original descending table-fill semantics using its LDX operand.
        int lastIndex = rom.ReadByte(0x88de54);
        AssertEqual(lastIndex, (int)rom.ReadByte(0x88deba), "Native fade-in/out use identical fill lengths");
        var table = new byte[lastIndex + 1];
        int value = counter;
        for (int index = lastIndex; index >= 0; index--)
        {
            table[index] = (byte)value;
            if (value > 0) value--;
        }
        ushort tableBase = ReadVerificationWord(rom, 0x88de5a);
        var rows = new byte[224];
        int cursor = 0x88df03, y = 0;
        byte held = 0;
        while (true)
        {
            byte count = rom.ReadByte(cursor++);
            if (count == 0) break;
            AssertTrue(count < 128 && y + count <= rows.Length, "Native nonrepeating indirect band geometry");
            int offset = ReadVerificationWord(rom, cursor) - tableBase;
            cursor += 2;
            AssertTrue((uint)offset < table.Length, "Native HDMA pointer addresses filled color byte");
            held = table[offset];
            for (int line = 0; line < count; line++) rows[y++] = held;
        }
        AssertEqual(0x88df34, cursor, "Complete native HDMA table consumed through terminator");
        while (y < rows.Length) rows[y++] = held;
        return rows;
    }

    private static void VerifyCeresHazeNativeRamp(ISnesAddressSpace rom)
    {
        int fadeLimit = ReadVerificationWord(rom, 0x88de43);
        for (int counter = 0; counter <= fadeLimit; counter++)
        {
            byte[] original = OriginalCeresHazeRamp(rom, counter);
            foreach (bool dead in new[] { false, true })
            {
                int flags = ReadVerificationWord(rom, dead ? 0x88de16 : 0x88de11);
                var captured = SnesGameplayFrameRenderer.CaptureCeresHaze(dead, counter);
                var software = new Rgba32[256 * 224];
                SnesGameplayFrameRenderer.ApplyCeresHaze(software, dead, counter);
                for (int y = 0; y < original.Length; y++)
                {
                    var actual = CeresHazeRenderDefinitions.ResolveComponents(y, counter, dead, null);
                    byte red = (byte)((flags & 0x20) != 0 ? original[y] : 0);
                    byte green = (byte)((flags & 0x40) != 0 ? original[y] : 0);
                    byte blue = (byte)((flags & 0x80) != 0 ? original[y] : 0);
                    AssertEqual((red, green, blue), actual, "Every native fade counter and physical scanline");
                    byte Expand(byte component) => y < 32 ? (byte)0 : (byte)((component << 3) | (component >> 2));
                    AssertEqual(Expand(red), captured.Windows[y].Red, "Captured red preserves native ramp/HUD mask");
                    AssertEqual(Expand(green), captured.Windows[y].Green, "Captured green preserves native ramp/HUD mask");
                    AssertEqual(Expand(blue), captured.Windows[y].Blue, "Captured blue preserves native ramp/HUD mask");
                    for (int x = 0; x < 256; x++)
                    {
                        var pixel = software[y * 256 + x];
                        AssertEqual((Expand(red), Expand(green), Expand(blue)), (pixel.R, pixel.G, pixel.B), "Software projection preserves native row across width");
                    }
                }
            }
        }
    }

    private static void VerifyCeresHazeTintScaling(ISnesAddressSpace rom)
    {
        int lastCounter = ReadVerificationWord(rom, 0x88de43);
        var document = new RoomFxPaletteBlendDocument { Version = 1, Blends = new() };
        foreach (byte id in OriginalFxBlendIds())
            document.Blends.Add($"blend-{id:X2}", [new() { Red = 0, Green = 0, Blue = 0 },
                new() { Red = 0, Green = 0, Blue = 0 }, new() { Red = 0, Green = 0, Blue = 0 }]);
        for (int channel = 0; channel <= 31; channel++)
        {
            var edited = document with
            {
                CeresHazeBlue = new() { Red = channel, Green = channel, Blue = channel },
                CeresHazeRed = new() { Red = 31 - channel, Green = 31 - channel, Blue = 31 - channel },
            };
            var colors = RoomFxPaletteBlendCatalog.Load(new MemoryStream(RoomFxPaletteBlendCatalog.Write(edited)));
            for (int counter = 0; counter <= lastCounter; counter++)
            foreach (bool dead in new[] { false, true })
            {
                int original = OriginalCeresHazeRamp(rom, counter)[200];
                int selected = dead ? 31 - channel : channel;
                // Independent nearest-integer definition of the pre-existing RGB5 scaling contract.
                byte expected = (byte)Math.Min(31, Math.Round((double)original * selected / (lastCounter - 1), MidpointRounding.AwayFromZero));
                var actual = CeresHazeRenderDefinitions.ResolveComponents(200, counter, dead, colors);
                AssertEqual((expected, expected, expected), actual, "All reachable amplitudes and RGB5 channel values scale exactly");
            }
        }
    }
}

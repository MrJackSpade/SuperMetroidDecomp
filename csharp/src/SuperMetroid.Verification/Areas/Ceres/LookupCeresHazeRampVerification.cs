using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupFxBlendSelectorsVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>Checks Ceres haze HDMA scanline amplitudes against the native ramp.</summary>
internal static class LookupCeresHazeRampVerification
{
    /// <summary>Replays the native descending fill and HDMA bands to produce 224 expected haze rows.</summary>
    private static byte[] OriginalCeresHazeRamp(ISnesAddressSpace rom, int counter)
    {
        // Execute the original descending table-fill semantics using its LDX operand.
        int lastIndex = rom.ReadCartridgeByte(0x88de54);
        AssertEqual(lastIndex, (int)rom.ReadCartridgeByte(0x88deba), "Native fade-in/out use identical fill lengths");
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
            byte count = rom.ReadCartridgeByte(cursor++);
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

    /// <summary>Compares every fade counter and scanline with captured windows and rendered RGB channels.</summary>
    internal static void VerifyCeresHazeNativeRamp(ISnesAddressSpace rom)
    {
        int fadeLimit = ReadVerificationWord(rom, 0x88de43);
        for (int counter = 0; counter <= fadeLimit; counter++)
        {
            byte[] original = OriginalCeresHazeRamp(rom, counter);
            foreach (bool dead in new[] { false, true })
            {
                int flags = ReadVerificationWord(rom, dead ? 0x88de16 : 0x88de11);
                var captured = GameplayEffectCapture.CaptureCeresHaze(dead, counter);
                // The haze is its tint stepped down the gameplay area (#1468): no colour above line 64, one step more
                // every eight lines up to fifteen, shifted by the fade.
                AssertEqual(new LineColourSteps(64, 8, 15, counter - 15), captured.Steps, "the haze's bands and fade");
                AssertEqual(ScreenWindow.Of(WindowRectangle.Spanning(0, 32, 255, 223), 32), captured.Window, "the haze covers the gameplay area");
                // The haze over a black screen, as the display renderer draws it.
                RgbaPixel[] drawn = DisplayRendering.Render(new LayeredScene(FramePaint.FromNative(new RgbColor[SnesCgram.ColorCount], []),
                    new DisplayLayer[] { captured }, DisplayBrightness.Full));
                for (int y = 0; y < original.Length; y++)
                {
                    byte red = (byte)((flags & 0x20) != 0 ? original[y] : 0);
                    byte green = (byte)((flags & 0x40) != 0 ? original[y] : 0);
                    byte blue = (byte)((flags & 0x80) != 0 ? original[y] : 0);
                    byte Expand(byte component) => y < 32 ? (byte)0 : (byte)((component << 3) | (component >> 2));
                    for (int x = 0; x < 256; x++)
                    {
                        var pixel = drawn[y * 256 + x];
                        AssertEqual(new VerifyCeresHazeNativeRampCase(Expand(red), Expand(green), Expand(blue)), new VerifyCeresHazeNativeRampCase(pixel.R, pixel.G, pixel.B), "The drawn haze preserves native row across width");
                    }
                }
            }
        }
    }
}

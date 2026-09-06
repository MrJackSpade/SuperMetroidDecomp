using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;

internal static partial class Program
{
    private static void VerifyFileSelectMapEntry()
    {
        var bus = new TestAddressSpace();
        for (int color = 0; color < 256; color++)
            WriteTestWord(bus, FileSelectMapRomData.EntryPalette + color * 2, 0x7fff);
        var entry = new FileSelectMapEntry(bus);
        Rgba32 visible = new(255, 0, 0), black = new(0, 0, 0);
        Rgba32[] scene = Enumerable.Repeat(visible, 256 * 224).ToArray();
        AssertEqual(0x7fff, entry.Cgram.Colors[14], "entry setup does not prematurely fade palette");
        AssertTrue(entry.Render(scene).All(pixel => pixel == black), "entry starts at forced-black presentation");
        entry.Step();
        AssertEqual(0x739c, entry.Cgram.Colors[14], "gradual palette begins at step one, not global-fade no-op");
        AssertEqual(0x739c, entry.Cgram.Colors[30], "both blank-cell palette entries fade together");
        for (int tick = 2; tick <= 16; tick++) entry.Step();
        AssertEqual(0, entry.Cgram.Colors[14], "sixteenth gradual step reaches exact black");
        AssertEqual(FileSelectMapEntryPhase.PaletteFade, entry.Phase, "palette endpoint retains native completion boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.LoadForeground, entry.Phase, "seventeenth call completes gradual palette fade");
        for (int color = 0; color < 256; color++)
            AssertEqual(color is 14 or 30 ? 0 : 0x7fff, entry.Cgram.Colors[color], "entry changes only the two target colors");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.LoadBackground, entry.Phase, "foreground transfer has its own entry boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.SetupWindow, entry.Phase, "background transfer has its own entry boundary");
        entry.Step();
        AssertEqual(FileSelectMapEntryPhase.Revealing, entry.Phase, "window setup precedes first expansion update");
        for (int tick = 0; tick <= 27; tick++)
        {
            int left = 127 - 4 * tick, right = 129 + 4 * tick;
            int top = 111 - 4 * tick, bottom = 113 + 4 * tick;
            Rgba32[] pixels = entry.Render(scene);
            for (int y = 0; y < 224; y++)
            for (int x = 0; x < 256; x++)
                AssertEqual(x >= left && x <= right && y >= top && y < bottom ? visible : black,
                    pixels[y * 256 + x], "entry reveal exact per-frame rectangle");
            AssertTrue(!entry.IsComplete, "entry window remains active before signed margin underflow");
            entry.Step();
        }
        AssertTrue(entry.IsComplete, "twenty-eighth reveal update disables the window");
        AssertTrue(entry.Render(scene).SequenceEqual(scene), "complete entry reveals entire scene without clipped border");
    }
}

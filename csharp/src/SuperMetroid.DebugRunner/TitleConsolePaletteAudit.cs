using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Verifies #310's actual console palette writes and visible flashing with retail title assets.</summary>
internal static class TitleConsolePaletteAudit
{
    public static int Run(string romPath)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var title = new TitleSequenceState(bus);
        title.Step(0);
        // Independent first records at $8D:C7FE and $8D:C866, destinations $54/$5C bytes.
        ushort[] expected = [0x0113, 0x000f, 0x175c, 0x0299, 0x13ff, 0x0bb1];
        for (int index = 0; index < expected.Length; index++)
            if (title.PaletteColors[42 + index] != expected[index])
                throw new InvalidDataException($"Title console color {42 + index}: expected ${expected[index]:X4}, actual ${title.PaletteColors[42 + index]:X4}.");
        title.Step(0);
        if (title.PaletteColors[46] != 0x00ac || title.PaletteColors[47] != 0x0145)
            throw new InvalidDataException("One-frame console lights failed to alternate.");
        for (int frame = 2; frame < 10; frame++) title.Step(0);
        if (title.PaletteColors[42] != 0x0113)
            throw new InvalidDataException("Ten-frame console palette changed too early.");
        title.Step(0);
        if (title.PaletteColors[42] != 0x00b0)
            throw new InvalidDataException("Ten-frame console palette failed to advance.");
        title.Step((ushort)SnesButton.Start);
        for (int frame = 0; title.Phase != TitleSequencePhase.TitleScreen && frame < 40; frame++) title.Step(0);
        if (title.Phase != TitleSequencePhase.TitleScreen)
            throw new InvalidDataException("Title skip did not reach the stationary title screen.");
        int skipPixels = VerifyVisibleConsoleFlash(title);

        var naturalTitle = new TitleSequenceState(bus);
        for (int frame = 0; naturalTitle.Phase != TitleSequencePhase.TitleScreen && frame < 5000; frame++)
            naturalTitle.Step(0);
        if (naturalTitle.Phase != TitleSequencePhase.TitleScreen)
            throw new InvalidDataException("Natural title sequence did not reach the stationary title screen.");
        int naturalPixels = VerifyVisibleConsoleFlash(naturalTitle);
        Console.WriteLine($"PASS: console palette values, 1/10-frame cadence, skip/natural routes, {skipPixels}/{naturalPixels} flashing pixels.");
        return 0;
    }

    private static int VerifyVisibleConsoleFlash(TitleSequenceState title)
    {
        var before = title.Render();
        var beforeColors = title.PaletteColors.ToArray();
        title.Step(0);
        var after = title.Render();
        int consolePixelChanges = 0;
        for (int pixel = 0; pixel < before.Length; pixel++)
        {
            if (before[pixel] == after[pixel]) continue;
            // Palette 46/47 colors are the one-frame console blink. Filter out the
            // independent baby animation so its movement cannot satisfy this assertion.
            for (int color = 46; color <= 47; color++)
                if (before[pixel] == SnesGraphics.DecodeBgr555Color(beforeColors[color])) consolePixelChanges++;
        }
        if (consolePixelChanges == 0)
            throw new InvalidDataException("Console palette alternated without changing visible console pixels.");
        return consolePixelChanges;
    }
}

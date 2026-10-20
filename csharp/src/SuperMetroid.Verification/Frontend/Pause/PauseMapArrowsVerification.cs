using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailPresentationFixtureVerification;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Diagnostics.SaveStates;
namespace SuperMetroid.Verification.Frontend.Pause;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Assets;

/// <summary>Checks pause map-arrow OAM, palette timing, and visibility at scroll boundaries.</summary>
internal static class PauseMapArrowsVerification
{
    /// <summary>Compares native arrow pixels and checks page fade and restored snapshot behavior.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots, native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the pause map arrow tiles, palette and spritemaps")]
    internal static void VerifyPauseMapArrows()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(bus);
        var pause = CreateRetailPauseFixture(new PauseMenuEntry { Samus = new SamusState(), System = new Bank80SystemState(), AreaIndex = AreaId.Crateria, RoomMapPosition = new(10, 10) });
        pause.mapScroll = new PauseMapScroll(new(0, 0), new(512, 256));
        pause.mapArrows = null;
        pause.BindMapPresentation(RetailPresentationFixture());
        var nativeColors = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++) nativeColors.SetColor(color, Bgr555.FromWord(checked(Read(0xb6f000 + color * 2))));
        int nativePaletteTimer = 1, nativePaletteFrame = 0;
        int allArrowParts = 0;
        var arrowAttributes = new HashSet<ushort>();
        foreach (var (x, y) in new (int, int)[] { (100, 70), (-24, 70), (-23, 70), (280, 70), (281, 70), (100, -56), (100, -55), (100, 79), (100, 80) })
        {
            pause.mapScrollPosition = new Position16<MapPixels>(unchecked((ushort)x), unchecked((ushort)y));
            pause.Step(0, 0);
            // Literal $82:A92B increment-before-read palette timing and raw colors.
            if (nativePaletteTimer != 0 && --nativePaletteTimer == 0)
            {
                nativePaletteFrame++;
                if (bus.ReadCartridgeByte(0x82c10c + nativePaletteFrame * 3) == 255) nativePaletteFrame = 0;
                nativePaletteTimer = bus.ReadCartridgeByte(0x82c10c + nativePaletteFrame * 3);
                for (int color = 0; color < 16; color++)
                    nativeColors.SetColor(176 + color, Bgr555.FromWord(checked(Read(0x82a987 + nativePaletteFrame * 32 + color * 2))));
            }
            var expected = new OamBuffer();
            expected.BeginFrame();
            bool[] visible = [unchecked((short)(-24 - x)) < 0, unchecked((short)(512 - 232 - x)) >= 0,
                unchecked((short)(-56 - y)) < 0, unchecked((short)(256 - 177 - y)) >= 0];
            for (int index = 0; index < 4; index++)
            {
                if (!visible[index]) continue;
                int record = 0x82b9a0 + index * 10;
                int animation = Read(record + 4) - 1;
                int bases = 0x820000 | Read(0x82c1e4 + animation * 2);
                int program = 0x820000 | Read(0x82c0e8 + animation * 2);
                int sprite = Read(bases) + bus.ReadCartridgeByte(program + 2);
                int pointer = 0x820000 | Read(0x82c569 + sprite * 2);
                DrawImportedSpritemap(bus, expected, pointer, new(Read(record), unchecked((ushort)(Read(record + 2) - 1))), Read(0x82c100));
            }
            int expectedBytes = expected.NextByteOffset;
            expected.FinalizeFrame();
            if (x == 100 && y == 70)
            {
                allArrowParts = expectedBytes / 4;
                for (int offset = 2; offset < expectedBytes; offset += 4)
                    arrowAttributes.Add(SnesLittleEndian.Word(expected.LowTable[offset], expected.LowTable[offset + 1]));
            }
            var pixels = pause.Render();
            var actual = pause.screen.Objects;
            AssertTrue(actual.LowTable[..expectedBytes].SequenceEqual(expected.LowTable[..expectedBytes]),
                $"pause arrows match raw cartridge OAM coordinates/shape/palette at scroll {x},{y}");
            int drawnArrowParts = 0;
            for (int offset = 2; offset < actual.LastFinalizedSpriteCount * 4; offset += 4)
                if (arrowAttributes.Contains(SnesLittleEndian.Word(actual.LowTable[offset], actual.LowTable[offset + 1]))) drawnArrowParts++;
            AssertEqual(expectedBytes / 4, drawnArrowParts, "unavailable arrows emit no leftover OAM parts");
            // The raw cartridge OBJ tiles form an independent pixel oracle. Require
            // every opaque arrow pixel to survive the complete pause compositor.
            var rawVram = new SnesVram();
            var tiles = new byte[0x2000];
            for (int i = 0; i < tiles.Length; i++) tiles[i] = bus.ReadCartridgeByte(0xb6c000 + i);
            rawVram.LoadBytes(MapSpriteFormat.PauseDestination, tiles);
            var rawPixels = SnesObjRenderer.Render(expected, rawVram, nativeColors, PauseMenuLayout.ObjectSelection);
            int opaque = 0;
            for (int i = 0; i < pixels.Length; i++)
                if (rawPixels[i].A != 0) { opaque++; AssertEqual(rawPixels[i], pixels[i], "native arrow pixel survives pause layers"); }
            AssertTrue(opaque > 0, "native reference contains visible arrow pixels");
            AssertTrue(pixels.SequenceEqual(pause.Render()), "repainting does not advance arrows");
        }
        var beforeRestore = pause.Render();
        using (var stream = new MemoryStream())
        {
            DebuggerObjectGraphSerializer.Serialize(stream, pause);
            stream.Position = 0;
            pause = DebuggerObjectGraphSerializer.Deserialize<PauseMenuState>(stream);
            pause.BindMapPresentation(RetailPresentationFixture());
        }
        AssertTrue(beforeRestore.SequenceEqual(pause.Render()), "debugger restore/rebind retains the current arrow display");
        pause.mapScrollPosition = new Position16<MapPixels>(100, 70);
        pause.Step(0, 0); _ = pause.Render(); int stable = pause.LastRenderedSpriteCount;
        pause.Step(SnesButton.R, 0); _ = pause.Render();
        AssertEqual(stable, pause.LastRenderedSpriteCount, "R press retains arrows on its native stable-page dispatch");
        pause.Step(0, 0); _ = pause.Render();
        AssertEqual(stable - allArrowParts, pause.LastRenderedSpriteCount, "page-fade dispatcher does not draw map arrows");
        Console.WriteLine("Pause arrows: native ROM OAM and opaque pixels match at all four scroll edges; R/fade visibility and pure repaint pass.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }

    /// <summary>Runs the retail pause map-arrow visual comparison.</summary>
    [VerificationCommand("--pause-map-arrows")]
    internal static int PauseMapArrowsCommand()
    {
        Suite(nameof(VerifyPauseMapArrows), () => VerifyPauseMapArrows());
        return 0;
    }
}

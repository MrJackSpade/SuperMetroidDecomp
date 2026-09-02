using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

internal static partial class Program
{
    /// <summary>
    /// Exercises pause-page transitions and live equipment mutation against a deliberately
    /// tiny synthetic ROM table. The private-ROM route separately proves the same table
    /// interpreter against retail assets; this fixture makes input/bit semantics part of
    /// the always-runnable suite without bundling copyrighted graphics.
    /// </summary>
    static void VerifyPauseMenuEquipmentInteraction()
    {
        var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];

        // Area zero's pause map is a literal 64x32 tilemap at $B5:9000, and its area label
        // uses a harmless zero-filled bank-$82 source. The visual bytes may remain zero for
        // this state/bit test; the private audit verifies nonempty retail rendering.
        WriteRomLong(rom, 0x82964a, 0xb59000);
        WriteRomWord(rom, 0x82965f, 0x8000);

        // Make every cartridge map cell exist and use low-priority character one. Put a
        // high-priority BG2 holder tile at screen tile (10,10). The pause PPU must leave
        // that green holder pixel above the red map even after the area-map upgrade makes
        // the complete BG1 map visible.
        WriteRomWord(rom, 0x829717, 0x8500);
        for (int mapByte = 0; mapByte < 0x0100; mapByte++)
            WriteRomByte(rom, 0x828500 + mapByte, 0xff);
        for (int mapWord = 0; mapWord < 0x0800; mapWord++)
            WriteRomWord(rom, 0xb59000 + mapWord * 2, 0x0001);
        WriteRomWord(rom, 0xb6e000 + (10 * 32 + 10) * 2, 0x2002);
        for (int row = 0; row < 8; row++)
        {
            WriteRomByte(rom, 0xb68000 + 1 * 32 + row * 2, 0xff);
            WriteRomByte(rom, 0xb68000 + 2 * 32 + row * 2 + 1, 0xff);
        }

        // Both pause indicators are ordinary bank-$82 menu spritemaps. One harmless
        // single-entry record lets this synthetic test inspect OAM placement without
        // embedding any retail graphics. The private-ROM route below exercises the real
        // records and pixels.
        WriteRomWord(rom, 0x82c569 + 0x5f * 2, 0x8100);
        WriteRomWord(rom, 0x82c569 + 0x10 * 2, 0x8100);
        WriteRomWord(rom, 0x828100, 1);
        WriteRomWord(rom, 0x82c100, 0x0e00);

        // DrawPauseScreenSpriteAnim(3) reads an eight-bit category from WRAM $0755,
        // then uses the category to select the base ID at $82:C202. Populate several
        // identical animation records because the 32-frame page fade advances the timer.
        WriteRomWord(rom, 0x82c0da, 0x0755);
        WriteRomWord(rom, 0x82c0ec, 0x8200);
        for (int frame = 0; frame < 8; frame++)
        {
            WriteRomByte(rom, 0x828200 + frame * 3, 8);
            WriteRomByte(rom, 0x828202 + frame * 3, 0);
        }
        WriteRomByte(rom, 0x828200 + 8 * 3, 0xff);
        WriteRomWord(rom, 0x82c1e8, 0xc300);
        WriteRomWord(rom, 0x82c300 + 2 * 2, 0x0010);
        WriteRomWord(rom, 0x82c18e + 2 * 2, 0xc400);
        WriteRomWord(rom, 0x82c400 + 2 * 4, 0x0091);
        WriteRomWord(rom, 0x82c402 + 2 * 4, 0x0071);
        WriteRomWord(rom, 0x82c400 + 3 * 4, 0x00a1);
        WriteRomWord(rom, 0x82c402 + 3 * 4, 0x0081);

        // Four wireframe comparison entries cover no suit, Varia, Gravity, and both.
        ushort[] wireframeComparisons = [0x0000, 0x0001, 0x0100, 0x0101];
        for (int index = 0; index < wireframeComparisons.Length; index++)
        {
            WriteRomWord(rom, 0x82b257 + index * 2, wireframeComparisons[index]);
            WriteRomWord(rom, 0x82b25f + index * 2, 0x8000);
        }

        // The real tables contain WRAM byte destinations, bank-$82 source pointers, and
        // inventory masks. Give every synthetic label a disjoint valid destination while
        // retaining the real category counts and early-game Morph/Bombs masks.
        PopulateEquipmentCategory(
            rom,
            offsetTable: 0x82c06c,
            tilemapPointerTable: 0x82c08c,
            bitmaskTable: 0x82c04c,
            destinationStart: 0x3800,
            masks: [0x1000, 0x0002, 0x0001, 0x0004, 0x0008],
            labelWords: 5);
        PopulateEquipmentCategory(
            rom,
            offsetTable: 0x82c076,
            tilemapPointerTable: 0x82c096,
            bitmaskTable: 0x82c056,
            destinationStart: 0x3900,
            masks: [0x0001, 0x0020, 0x0004, 0x1000, 0x0002, 0x0008],
            labelWords: 9);
        PopulateEquipmentCategory(
            rom,
            offsetTable: 0x82c082,
            tilemapPointerTable: 0x82c0a2,
            bitmaskTable: 0x82c062,
            destinationStart: 0x3a00,
            masks: [0x0100, 0x0200, 0x2000],
            labelWords: 9);

        // Pause keeps the existing BG3 HUD tilemap but replaces its character sheet with
        // $9A:B200. Give character one one visible pixel and palette color one a white
        // value, then publish that character in the retained gameplay VRAM image.
        WriteRomByte(rom, 0x9ab210, 0x80);
        WriteRomWord(rom, 0xb6f002, 0x7fff);
        WriteRomWord(rom, 0xb6f004, 0x03e0);
        var gameplayVram = new SnesVram();
        gameplayVram.ExecuteWordTransfer([0x0001], 0x5800, 1);
        var bus = new SuperMetroidAddressSpace(rom);
        var samus = new SamusState
        {
            CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            XPosition = 0x0200,
            YPosition = 0x0300,
        };
        var system = new Bank80SystemState();
        system.SetAreaMapAcquired(0);
        system.MarkExploredMapTile(0, mapX: 30, mapY: 5);
        var pause = new PauseMenuState(
            bus,
            samus,
            system,
            areaIndex: 0,
            roomMapX: 28,
            roomMapY: 1,
            gameplayVram: gameplayVram);

        AssertEqual(0, pause.ScreenMode, "pause begins on map page");
        Rgba32[] firstPauseFrame = pause.Render();
        AssertEqual(new Rgba32(255, 255, 255, 255), firstPauseFrame[0],
            "pause retains and draws gameplay BG3 HUD tilemap");
        AssertEqual(new Rgba32(0, 255, 0, 255), firstPauseFrame[80 * 256 + 80],
            "high-priority pause holder clips the fully revealed low-priority map");
        AssertEqual(124, pause.MapHorizontalScroll,
            "downloaded pause map horizontal centering");
        AssertEqual(unchecked((ushort)-24), pause.MapVerticalScroll,
            "downloaded pause map vertical centering");
        AssertEqual(116, pause.LastIndicatorOriginX, "pause map marker X origin");
        AssertEqual(64, pause.LastIndicatorOriginY, "pause map marker Y origin");
        AssertEqual(0x5f, pause.LastIndicatorSpritemapId, "pause map marker initial frame");
        AssertEqual(1, pause.LastRenderedSpriteCount, "pause map marker OAM count");
        AssertEqual(0x1400, pause.ReadPauseButtonLabelWord(805) & 0x1c00,
            "pause map label bright palette reached BG2");
        AssertEqual(0x0800, pause.ReadPauseButtonLabelWord(822) & 0x1c00,
            "pause equipment label dim palette reached BG2");
        pause.Step((ushort)SnesButton.R, 0);
        for (int frame = 0; frame < 32; frame++)
            pause.Step(0, 0);
        AssertEqual(1, pause.ScreenMode, "pause R transition reaches equipment page");
        AssertEqual(2, pause.SelectedCategory, "pause selects suits/misc category");
        AssertEqual(2, pause.SelectedItem, "pause selects first collected Morph Ball item");
        pause.Render();
        AssertEqual(0x10, pause.LastIndicatorSpritemapId, "pause selector category base ID");
        AssertEqual(0x90, pause.LastIndicatorOriginX, "pause Morph selector X origin");
        AssertEqual(0x70, pause.LastIndicatorOriginY, "pause Morph selector Y origin");
        AssertEqual(1, pause.LastRenderedSpriteCount, "pause equipment selector OAM count");
        AssertEqual(0x0800, pause.ReadPauseButtonLabelWord(805) & 0x1c00,
            "pause map label dims on equipment page");
        AssertEqual(0x1400, pause.ReadPauseButtonLabelWord(822) & 0x1c00,
            "pause equipment label brightens on equipment page");

        // D-pad and A use joypad1_newkeys, not the delayed-held word used by L/R/Start.
        pause.Step(0, (ushort)SnesButton.Down);
        AssertEqual(3, pause.SelectedItem, "pause Down selects collected Bombs");
        pause.Render();
        AssertEqual(0xa0, pause.LastIndicatorOriginX, "pause Bombs selector X origin");
        AssertEqual(0x80, pause.LastIndicatorOriginY, "pause Bombs selector Y origin");
        pause.Step(0, (ushort)SnesButton.A);
        AssertTrue(!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs),
            "pause A unequips live Bombs bit");
        pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.A);
        AssertTrue(samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs),
            "pause A re-equips live Bombs bit");
        AssertTrue(pause.Step((ushort)SnesButton.Start, 0),
            "pause delayed Start requests outer unpause state");
        AssertEqual(0x0800, pause.ReadPauseButtonLabelWord(812) & 0x1c00,
            "pause Start label switches to the native unpause palette");

        Console.WriteLine(
            "  Pause menu: ROM tables, native map centering, OAM indicators, page transition, Bomb toggle, and Start agree.");
    }

    private static void PopulateEquipmentCategory(
        byte[] rom,
        int offsetTable,
        int tilemapPointerTable,
        int bitmaskTable,
        ushort destinationStart,
        ReadOnlySpan<ushort> masks,
        int labelWords)
    {
        for (int item = 0; item < masks.Length; item++)
        {
            WriteRomWord(rom, offsetTable + item * 2,
                unchecked((ushort)(destinationStart + item * labelWords * 2)));
            WriteRomWord(rom, tilemapPointerTable + item * 2, 0x8000);
            WriteRomWord(rom, bitmaskTable + item * 2, masks[item]);
        }
    }

    private static void WriteRomLong(byte[] rom, int snesAddress, int value)
    {
        int offset = SuperMetroidAddressSpace.ToRomOffset(snesAddress);
        rom[offset] = unchecked((byte)value);
        rom[offset + 1] = unchecked((byte)(value >> 8));
        rom[offset + 2] = unchecked((byte)(value >> 16));
    }

    private static void WriteRomByte(byte[] rom, int snesAddress, byte value) =>
        rom[SuperMetroidAddressSpace.ToRomOffset(snesAddress)] = value;
}

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

        var bus = new SuperMetroidAddressSpace(rom);
        var samus = new SamusState
        {
            CollectedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
            EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.Bombs),
        };
        var pause = new PauseMenuState(bus, samus, new Bank80SystemState(), areaIndex: 0);

        AssertEqual(0, pause.ScreenMode, "pause begins on map page");
        pause.Step((ushort)SnesButton.R, 0);
        for (int frame = 0; frame < 32; frame++)
            pause.Step(0, 0);
        AssertEqual(1, pause.ScreenMode, "pause R transition reaches equipment page");
        AssertEqual(2, pause.SelectedCategory, "pause selects suits/misc category");
        AssertEqual(2, pause.SelectedItem, "pause selects first collected Morph Ball item");

        // D-pad and A use joypad1_newkeys, not the delayed-held word used by L/R/Start.
        pause.Step(0, (ushort)SnesButton.Down);
        AssertEqual(3, pause.SelectedItem, "pause Down selects collected Bombs");
        pause.Step(0, (ushort)SnesButton.A);
        AssertTrue(!samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs),
            "pause A unequips live Bombs bit");
        pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.A);
        AssertTrue(samus.EquippedItems.HasAny(SamusEquipmentFlags.Bombs),
            "pause A re-equips live Bombs bit");
        AssertTrue(pause.Step((ushort)SnesButton.Start, 0),
            "pause delayed Start requests outer unpause state");

        Console.WriteLine(
            "  Pause menu: ROM tables, page transition, selector, Bomb toggle, and Start agree.");
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
}

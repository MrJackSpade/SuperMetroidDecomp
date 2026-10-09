using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Registers checks for Crocomire's native BG2 spritemap pointers and pose-dependent scroll corrections.</summary>
    /// <param name="rom">Cartridge address space supplying the native pointer and pose tables.</param>
    private static void VerifyCrocomireBg2ScrollDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyCrocomireBg2FramePositions), () => VerifyCrocomireBg2FramePositions(rom));
        Suite(nameof(VerifyCrocomireBg2PoseCorrections), () => VerifyCrocomireBg2PoseCorrections(rom));
    }

    /// <summary>Reads a little-endian word from cartridge data without carrying its low-word increment into another bank.</summary>
    /// <param name="rom">Cartridge address space containing the word.</param>
    /// <param name="address">Banked address of the word's low byte.</param>
    /// <returns>The two adjacent bytes combined as an unsigned word.</returns>
    private static ushort CrocomireBg2NativeWord(SuperMetroidAddressSpace rom, int address) =>
        (ushort)(rom.ReadByte(address) |
            rom.ReadByte((address & 0xff0000) | unchecked((ushort)(address + 1))) << 8);

    /// <summary>Checks all seventeen BG2 frame pointers against the cartridge table and verifies their order and uniqueness.</summary>
    /// <param name="rom">Cartridge address space containing the native frame-pointer table.</param>
    private static void VerifyCrocomireBg2FramePositions(SuperMetroidAddressSpace rom)
    {
        AssertEqual(17, CrocomireBg2ScrollDefinitions.EntryCount, "all native BG2 frame pointers");
        var seen = new HashSet<ushort>();
        for (int index = 0; index < 17; index++)
        {
            ushort expected = CrocomireBg2NativeWord(rom, 0xa48b79 + 2 * index);
            ushort actual = CrocomireBg2ScrollDefinitions.Entry(index).SpritemapPointer;
            AssertEqual(expected, actual, "Crocomire BG2 native pointer order");
            AssertTrue(seen.Add(actual), "Crocomire BG2 pointers are unique");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBg2ScrollDefinitions.Entry(-1), "negative BG2 entry");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireBg2ScrollDefinitions.Entry(17), "BG2 entry past end");
    }

    /// <summary>Checks native pose offsets and wrapped vertical-scroll results across all body positions and pointer identities.</summary>
    /// <param name="rom">Cartridge address space containing each frame's pointer and pose-correction word.</param>
    private static void VerifyCrocomireBg2PoseCorrections(SuperMetroidAddressSpace rom)
    {
        var originalOffsets = new Dictionary<ushort, ushort>();
        for (int index = 0; index < 17; index++)
        {
            ushort pointer = CrocomireBg2NativeWord(rom, 0xa48b79 + 2 * index);
            ushort offset = CrocomireBg2NativeWord(rom, 0xa40000 | unchecked((ushort)(pointer + 0x1c)));
            originalOffsets[pointer] = offset;
            AssertEqual(offset, CrocomireBg2ScrollDefinitions.Entry(index).Offset, "Crocomire BG2 native pose correction");
            foreach (ushort bodyY in new ushort[] { 0, 0x43, 0x90, 0xfffc })
            {
                ushort expected = unchecked((ushort)(0x43 - bodyY + offset));
                AssertEqual(expected, CrocomireBg2ScrollDefinitions.VerticalScroll(bodyY, pointer),
                    "Crocomire BG2 mapped pose and wrapped body position");
            }
        }
        // The new cases must match the native search for every possible identity,
        // including misaligned pointers and other poses in the same frame bank.
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            ushort offset = originalOffsets.GetValueOrDefault((ushort)pointer);
            AssertEqual(unchecked((ushort)(0x43 - 0x90 + offset)),
                CrocomireBg2ScrollDefinitions.VerticalScroll(0x90, (ushort)pointer),
                "Crocomire BG2 full pointer selection domain");
        }
    }
}

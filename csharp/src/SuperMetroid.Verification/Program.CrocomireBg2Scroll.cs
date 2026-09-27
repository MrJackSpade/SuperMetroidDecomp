using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCrocomireBg2ScrollDefinitions(SuperMetroidAddressSpace rom)
    {
        ReadOnlySpan<CrocomireBg2VerticalCorrection> entries =
            CrocomireBg2ScrollDefinitions.Entries;
        AssertEqual(17, entries.Length,
            "Crocomire BG2 scroll has all seventeen native map entries");
        var seen = new HashSet<ushort>();
        for (int index = 0; index < entries.Length; index++)
        {
            CrocomireBg2VerticalCorrection entry = entries[index];
            ushort pointer = ReadWord(CrocomireBg2ScrollDefinitions.NativeMapTable + index * 2);
            ushort offset = ReadWord(0xa40000 | unchecked((ushort)(pointer +
                CrocomireBg2ScrollDefinitions.ThirdEntryYOffset)));
            AssertEqual(pointer, entry.SpritemapPointer,
                $"Crocomire BG2 correction map entry {index} matches cartridge");
            AssertEqual(offset, entry.Offset,
                $"Crocomire BG2 correction Y offset {index} matches cartridge");
            AssertTrue(seen.Add(entry.SpritemapPointer),
                $"Crocomire BG2 correction pointer {index} is unique");
            foreach (ushort bodyY in new ushort[] { 0x0000, 0x0043, 0x0090, 0xfffc })
            {
                ushort expected = unchecked((ushort)(
                    CrocomireBg2ScrollDefinitions.VerticalOrigin - bodyY + offset));
                AssertEqual(expected,
                    CrocomireBg2ScrollDefinitions.VerticalScroll(
                        bodyY, entry.SpritemapPointer),
                    $"Crocomire BG2 scroll for frame ${pointer:X4} at Y=${bodyY:X4}");
            }
        }
        AssertEqual(unchecked((ushort)(
                CrocomireBg2ScrollDefinitions.VerticalOrigin - 0x0090)),
            CrocomireBg2ScrollDefinitions.VerticalScroll(0x0090, 0xffff),
            "unmapped Crocomire frame retains the uncorrected native scroll");
        Console.WriteLine(
            "  Crocomire BG2 scroll: seventeen map pointers and frame Y offsets match the cartridge; mapped and unmapped positions are compiled without runtime ROM reads.");

        ushort ReadWord(int address) => unchecked((ushort)(rom.ReadByte(address) |
            rom.ReadByte((address & 0xff0000) | unchecked((ushort)(address + 1))) << 8));
    }
}

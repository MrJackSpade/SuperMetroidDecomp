using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEndingLogoPaletteSources(ISnesAddressSpace rom)
    {
        for (int palette = 0; palette < 2; palette++)
        for (int step = 0; step < 16; step++)
        {
            int address = 0x8be5e7 + step * 4 + palette * 2;
            AssertEqual((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8),
                EndingLogoPalettePointerDefinitions.Source(step, palette), "original ending palette source");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 16, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoPalettePointerDefinitions.Source(invalid, 0), "palette step bounds");
        foreach (int invalid in new[] { int.MinValue, -1, 2, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingLogoPalettePointerDefinitions.Source(0, invalid), "palette selector bounds");
    }

    private static void VerifyEndingPostShotTransferFields(ISnesAddressSpace rom)
    {
        for (int index = 0; index < 6; index++)
        {
            int address = 0x8be45a + index * 8;
            var actual = EndingPostShotUploadDefinitions.Get(index);
            AssertEqual(Word(address), actual.Length, "native transfer length");
            AssertEqual(Word(address + 2) | rom.ReadByte(address + 4) << 16, actual.SourceAddress, "native transfer source");
            AssertEqual(Word(address + 6), actual.DestinationWord, "native transfer destination word");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => EndingPostShotUploadDefinitions.Get(invalid), "transfer bounds");
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }
}
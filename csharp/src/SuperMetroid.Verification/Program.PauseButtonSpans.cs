using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPauseButtonSpanWords(ISnesAddressSpace rom)
    {
        // Independent native LDA long,X sites, in the original six-span order.
        int[] sites = [0x82a633, 0x82a651, 0x82a66f, 0x82a68d, 0x82a6ab, 0x82a6c9];
        var spans = PauseMenuLayout.ButtonLabelSpans.ToArray();
        AssertEqual(6, spans.Length, "exactly six pause button rows");
        for (int index = 0; index < sites.Length; index++)
        {
            int address = sites[index];
            AssertEqual((byte)0xbf, rom.ReadByte(address), "native button LDA long,X");
            AssertEqual((byte)0x7e, rom.ReadByte(address + 3), "native button WRAM bank");
            int expected = (ReadVerificationWord(rom, address + 1) - 0x3000) / 2;
            AssertEqual(expected, spans[index].Word, "native button tilemap word in enumeration order");
            AssertEqual(expected, PauseMenuLayout.ButtonLabelSpan((PauseButtonLabel)(index / 2), index % 2).Word,
                "named button row projection");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PauseMenuLayout.ButtonLabelSpan((PauseButtonLabel)invalid, 0),
                "unsupported button identity");
        foreach (int invalid in new[] { int.MinValue, -1, 2, 256, int.MaxValue })
            AssertThrows<ArgumentOutOfRangeException>(() => PauseMenuLayout.ButtonLabelSpan(PauseButtonLabel.Map, invalid),
                "unsupported button row");
    }

    private static void VerifyPauseButtonSpanCounts(ISnesAddressSpace rom)
    {
        int[] sites = [0x82a62d, 0x82a64b, 0x82a669, 0x82a687, 0x82a6a5, 0x82a6c3];
        var spans = PauseMenuLayout.ButtonLabelSpans.ToArray();
        for (int index = 0; index < sites.Length; index++)
        {
            AssertEqual((byte)0xa0, rom.ReadByte(sites[index]), "native button LDY byte count");
            int expected = ReadVerificationWord(rom, sites[index] + 1) / 2;
            AssertEqual(expected, spans[index].Count, "native button recolor width");
            AssertEqual(expected, PauseMenuLayout.ButtonLabelSpan((PauseButtonLabel)(index / 2), index % 2).Count,
                "named button width");
        }
    }
}

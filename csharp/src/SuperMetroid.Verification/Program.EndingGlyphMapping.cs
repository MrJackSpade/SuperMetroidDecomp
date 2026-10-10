using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks ending text-region coordinates and decoded labels against the retail tilemaps and typewriter data.</summary>
    /// <param name="bus">Address space for the pinned cartridge's text and glyph records.</param>
    private static void VerifyEndingTextRegions(ISnesAddressSpace bus)
    {
        ushort Read(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        void Panel(EndingTextRegionDefinition actual, int source, string expectedText, int startColumn)
        {
            AssertEqual(0, actual.Row, "ending panel row");
            AssertEqual(startColumn, actual.Column, "ending panel first native text column");
            AssertEqual(expectedText.Length, actual.Width, "ending panel text width");
            string text = new(Enumerable.Range(0, actual.Width).Select(i =>
                EndingTextDefinitions.DecodeGlyph(Read(source + 2 * (startColumn + i)), actual.Style)).ToArray());
            AssertEqual(expectedText, text, "ending native panel label");
        }
        // Row0 of the two original tilemaps; the blank-run boundaries locate text independently.
        int producerStart = Enumerable.Range(0, 32).First(i => Read(0x8cdc9b + i * 2) != 0x004f);
        int producerEnd = Enumerable.Range(0, 32).Last(i => Read(0x8cdc9b + i * 2) != 0x004f);
        Panel(EndingTextDefinitions.ResultProducedBy, 0x8cdc9b, "PRODUCED BY", producerStart);
        AssertEqual(producerEnd - producerStart + 1, EndingTextDefinitions.ResultProducedBy.Width, "producer native span");
        int yearStart = Enumerable.Range(0, 32).First(i => Read(0x8cdedb + i * 2) != 0x007f);
        int yearEnd = yearStart;
        while (Read(0x8cdedb + yearEnd * 2) != 0x007f) yearEnd++;
        int companyStart = yearEnd;
        while (Read(0x8cdedb + companyStart * 2) == 0x007f) companyStart++;
        Panel(EndingTextDefinitions.CopyrightYear, 0x8cdedb, "1994", yearStart);
        Panel(EndingTextDefinitions.CopyrightCompany, 0x8cdedb, "NINTENDO", companyStart);
        AssertEqual(EndingTextStyle.ResultSmall, EndingTextDefinitions.ResultProducedBy.Style, "producer style");
        AssertEqual(EndingTextStyle.CopyrightLarge, EndingTextDefinitions.CopyrightYear.Style, "copyright year style");
        AssertEqual(EndingTextStyle.CopyrightLarge, EndingTextDefinitions.CopyrightCompany.Style, "copyright company style");
        void Line(int start, EndingTextRegionDefinition actual, string expectedText, EndingTextStyle style)
        {
            int cursor = start, count = 0;
            int nativeRow = bus.ReadByte(start + 3);
            AssertEqual(nativeRow, actual.Row, "native typewriter row");
            AssertEqual((int)bus.ReadByte(start + 2), actual.Column, "native first typewriter column");
            AssertEqual(style, actual.Style, "typewriter style");
            var text = new System.Text.StringBuilder();
            while (count < 32 && Read(cursor) < 0x8000 && bus.ReadByte(cursor + 3) == nativeRow)
            {
                AssertEqual(actual.Column + count, (int)bus.ReadByte(cursor + 2), "native consecutive typewriter column");
                int glyph = 0x8c0000 | Read(cursor + 4);
                text.Append(EndingTextDefinitions.DecodeGlyph(Read(glyph + 4), style));
                count++;
                cursor += 6;
            }
            AssertEqual(count, actual.Width, "native typewriter width");
            AssertEqual(expectedText, text.ToString(), "native typewriter label");
        }
        Line(0x8cdfe1, EndingTextDefinitions.PercentageHeading, "YOUR RATE FOR", EndingTextStyle.PercentageSmall);
        Line(0x8ce02f, EndingTextDefinitions.PercentageDetail, "COLLECTING ITEMS IS", EndingTextStyle.PercentageSmall);
        Line(0x8ce0b5, EndingTextDefinitions.FinalMessage, "SEE YOU NEXT MISSION", EndingTextStyle.FinalLarge);
    }

    /// <summary>
    /// Checks compiled glyph words against the independently transcribed Font 3 atlas and
    /// verifies that decoding accepts only mapped glyphs while compilation rejects invalid inputs.
    /// </summary>
    /// <param name="bus">Address space containing the pinned cartridge's named ending glyph records.</param>
    private static void VerifyEndingGlyphMapping(ISnesAddressSpace bus)
    {
        // Transcribed from the SHA-pinned Font3 atlas, viewed with --lookup-ending-font-layout.
        // These are visual cell identities, not outputs from CompileGlyph/DecodeGlyph.
        ushort[] small = [0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0a,0x0b,0x0c,
            0x0d,0x0e,0x0f,0x10,0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19];
        ushort[] top = [0x20,0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,0x2a,0x2b,0x2c,
            0x2d,0x2e,0x2f,0x40,0x41,0x42,0x43,0x44,0x45,0x46,0x47,0x48,0x49];
        ushort[] lower = [0x30,0x31,0x32,0x33,0x34,0x35,0x36,0x37,0x38,0x39,0x3a,0x3b,0x3c,
            0x3d,0x3e,0x3f,0x50,0x51,0x52,0x53,0x54,0x55,0x56,0x57,0x58,0x59];
        ushort[] digitsTop = [0x60,0x61,0x62,0x63,0x64,0x65,0x66,0x67,0x68,0x69];
        ushort[] digitsBottom = [0x70,0x71,0x72,0x73,0x74,0x75,0x76,0x77,0x78,0x79];
        ushort Read(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        (char Glyph, int Address)[] largeRecords = [
            (' ', 0x8ce135),
            ('S', 0x8ce13d),
            ('E', 0x8ce145),
            ('Y', 0x8ce14d),
            ('O', 0x8ce155),
            ('U', 0x8ce15d),
            ('N', 0x8ce165),
            ('X', 0x8ce16d),
            ('T', 0x8ce175),
            ('M', 0x8ce17d),
            ('I', 0x8ce185),
        ];
        foreach (var record in largeRecords)
        {
            AssertEqual(Read(record.Address), EndingTextDefinitions.CompileGlyph(record.Glyph, EndingTextStyle.FinalLarge), "native named glyph record");
            AssertEqual(Read(record.Address + 2), EndingTextDefinitions.CompileGlyph(record.Glyph, EndingTextStyle.FinalLarge, bottom: true), "native named lower glyph record");
        }
        (char Glyph, int Address)[] smallRecords = [
            ('A', 0x8ce18d),
            ('C', 0x8ce193),
            ('E', 0x8ce199),
            ('F', 0x8ce19f),
            ('G', 0x8ce1a5),
            ('I', 0x8ce1ab),
            ('L', 0x8ce1b1),
            ('M', 0x8ce1b7),
            ('N', 0x8ce1bd),
            ('O', 0x8ce1c3),
            ('R', 0x8ce1c9),
            ('S', 0x8ce1cf),
            ('T', 0x8ce1d5),
            ('U', 0x8ce1db),
            ('Y', 0x8ce1e1),
            (' ', 0x8ce1e7),
        ];
        foreach (var record in smallRecords)
        {
            AssertEqual(Read(record.Address), EndingTextDefinitions.CompileGlyph(record.Glyph, EndingTextStyle.PercentageSmall), "native named glyph record");
        }
        // Original indirect text records and producer/copyright panels establish attributes/blanks.
        ushort percentageAttributes = (ushort)(Read(0x8ce18d) & 0xfc00);
        ushort finalAttributes = (ushort)(Read(0x8ce13d) & 0xfc00);
        foreach (EndingTextStyle style in Enum.GetValues<EndingTextStyle>())
        foreach (bool bottom in new[] { false, true })
        {
            bool large = style is EndingTextStyle.CopyrightLarge or EndingTextStyle.FinalLarge;
            ushort attributes = style == EndingTextStyle.PercentageSmall ? percentageAttributes :
                style == EndingTextStyle.FinalLarge ? finalAttributes : (ushort)0;
            ushort blank = Read(style switch
            {
                EndingTextStyle.ResultSmall => 0x8cdc9b,
                EndingTextStyle.CopyrightLarge => 0x8cdedb,
                EndingTextStyle.PercentageSmall => 0x8ce1e7,
                _ => bottom ? 0x8ce137 : 0x8ce135,
            });
            var expected = new Dictionary<ushort, char> { [blank] = ' ' };
            for (int letter = 0; letter < 26; letter++)
                expected.Add((ushort)(attributes | (large ? (bottom ? lower[letter] : top[letter]) : small[letter])), (char)('A' + letter));
            if (style == EndingTextStyle.CopyrightLarge)
                for (int digit = 0; digit < 10; digit++)
                    expected.Add(bottom ? digitsBottom[digit] : digitsTop[digit], (char)('0' + digit));
            foreach (var entry in expected)
                AssertEqual(entry.Key, EndingTextDefinitions.CompileGlyph(entry.Value, style, bottom), $"ending {style} glyph {entry.Value} half {bottom}");
            for (int word = 0; word <= ushort.MaxValue; word++)
            {
                if (expected.TryGetValue((ushort)word, out char glyph))
                    AssertEqual(glyph, EndingTextDefinitions.DecodeGlyph((ushort)word, style, bottom), "ending glyph inverse");
                else
                    AssertThrows<InvalidDataException>(() => EndingTextDefinitions.DecodeGlyph((ushort)word, style, bottom), "ending glyph rejects non-glyph word");
            }
            foreach (char invalid in new[] { '\0', '@', '[', 'a', '/', ':', '\uffff' })
                AssertThrows<ArgumentOutOfRangeException>(() => EndingTextDefinitions.CompileGlyph(invalid, style, bottom), "ending glyph character bounds");
            if (style != EndingTextStyle.CopyrightLarge)
                for (char digit = '0'; digit <= '9'; digit++)
                    AssertThrows<ArgumentOutOfRangeException>(() => EndingTextDefinitions.CompileGlyph(digit, style, bottom), "ending glyph digits restricted to copyright");
        }
        foreach (EndingTextStyle invalid in new[] { (EndingTextStyle)4, (EndingTextStyle)255 })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingTextDefinitions.CompileGlyph('A', invalid), "ending glyph style bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingTextDefinitions.DecodeGlyph(0, invalid), "ending decoder style bounds");
        }
    }
}

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending text placement and glyph encoding against native tilemaps and text records.</summary>
internal static class EndingGlyphMappingVerification
{
    /// <summary>Locates panel text and typewriter lines in the cartridge and checks compiled region geometry.</summary>
    internal static void VerifyEndingTextRegions(ISnesAddressSpace bus)
    {
        void Panel(EndingTextRegionDefinition actual, int source, string expectedText, int startColumn)
        {
            AssertEqual(0, actual.Row, "ending panel row");
            AssertEqual(startColumn, actual.Column, "ending panel first native text column");
            AssertEqual(expectedText.Length, actual.Width, "ending panel text width");
            string text = new(Enumerable.Range(0, actual.Width).Select(i =>
                EndingTextGlyphs.DecodeGlyph(BackgroundTileWordBytes.Tile(CartridgeImportSource.Require(bus).ReadCartridgeWord((source + 2 * (startColumn + i)))), actual.Style)).ToArray());
            AssertEqual(expectedText, text, "ending native panel label");
        }
        // Row0 of the two original tilemaps; the blank-run boundaries locate text independently.
        int producerStart = Enumerable.Range(0, 32).First(i => CartridgeImportSource.Require(bus).ReadCartridgeWord((0x8cdc9b + i * 2)) != 0x004f);
        int producerEnd = Enumerable.Range(0, 32).Last(i => CartridgeImportSource.Require(bus).ReadCartridgeWord((0x8cdc9b + i * 2)) != 0x004f);
        Panel(EndingTextDefinitions.ResultProducedBy, 0x8cdc9b, "PRODUCED BY", producerStart);
        AssertEqual(producerEnd - producerStart + 1, EndingTextDefinitions.ResultProducedBy.Width, "producer native span");
        int yearStart = Enumerable.Range(0, 32).First(i => CartridgeImportSource.Require(bus).ReadCartridgeWord((0x8cdedb + i * 2)) != 0x007f);
        int yearEnd = yearStart;
        while (CartridgeImportSource.Require(bus).ReadCartridgeWord((0x8cdedb + yearEnd * 2)) != 0x007f) yearEnd++;
        int companyStart = yearEnd;
        while (CartridgeImportSource.Require(bus).ReadCartridgeWord((0x8cdedb + companyStart * 2)) == 0x007f) companyStart++;
        Panel(EndingTextDefinitions.CopyrightYear, 0x8cdedb, "1994", yearStart);
        Panel(EndingTextDefinitions.CopyrightCompany, 0x8cdedb, "NINTENDO", companyStart);
        AssertEqual(EndingTextStyle.ResultSmall, EndingTextDefinitions.ResultProducedBy.Style, "producer style");
        AssertEqual(EndingTextStyle.CopyrightLarge, EndingTextDefinitions.CopyrightYear.Style, "copyright year style");
        AssertEqual(EndingTextStyle.CopyrightLarge, EndingTextDefinitions.CopyrightCompany.Style, "copyright company style");
        void Line(int start, EndingTextRegionDefinition actual, string expectedText, EndingTextStyle style)
        {
            int cursor = start, count = 0;
            int nativeRow = bus.ReadCartridgeByte(start + 3);
            AssertEqual(nativeRow, actual.Row, "native typewriter row");
            AssertEqual(bus.ReadCartridgeByte(start + 2), actual.Column, "native first typewriter column");
            AssertEqual(style, actual.Style, "typewriter style");
            var text = new System.Text.StringBuilder();
            while (count < 32 && CartridgeImportSource.Require(bus).ReadCartridgeWord(cursor) < 0x8000 && bus.ReadCartridgeByte(cursor + 3) == nativeRow)
            {
                AssertEqual(actual.Column + count, (int)bus.ReadCartridgeByte(cursor + 2), "native consecutive typewriter column");
                int glyph = 0x8c0000 | CartridgeImportSource.Require(bus).ReadCartridgeWord((cursor + 4));
                text.Append(EndingTextGlyphs.DecodeGlyph(BackgroundTileWordBytes.Tile(CartridgeImportSource.Require(bus).ReadCartridgeWord((glyph + 4))), style));
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

    /// <summary>Checks every supported glyph word and rejects non-glyph words, invalid characters, and styles.</summary>
    internal static void VerifyEndingGlyphMapping(ISnesAddressSpace bus)
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
        LargeRecord[] largeRecords = [
            new LargeRecord(' ', 0x8ce135),
            new LargeRecord('S', 0x8ce13d),
            new LargeRecord('E', 0x8ce145),
            new LargeRecord('Y', 0x8ce14d),
            new LargeRecord('O', 0x8ce155),
            new LargeRecord('U', 0x8ce15d),
            new LargeRecord('N', 0x8ce165),
            new LargeRecord('X', 0x8ce16d),
            new LargeRecord('T', 0x8ce175),
            new LargeRecord('M', 0x8ce17d),
            new LargeRecord('I', 0x8ce185),
        ];
        foreach (var record in largeRecords)
        {
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(record.Address), EndingTextGlyphs.CompileGlyph(record.Glyph, EndingTextStyle.FinalLarge).PackedWord, "native named glyph record");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord((record.Address + 2)), EndingTextGlyphs.CompileGlyph(record.Glyph, EndingTextStyle.FinalLarge, bottom: true).PackedWord, "native named lower glyph record");
        }
        LargeRecord[] smallRecords = [
            new LargeRecord('A', 0x8ce18d),
            new LargeRecord('C', 0x8ce193),
            new LargeRecord('E', 0x8ce199),
            new LargeRecord('F', 0x8ce19f),
            new LargeRecord('G', 0x8ce1a5),
            new LargeRecord('I', 0x8ce1ab),
            new LargeRecord('L', 0x8ce1b1),
            new LargeRecord('M', 0x8ce1b7),
            new LargeRecord('N', 0x8ce1bd),
            new LargeRecord('O', 0x8ce1c3),
            new LargeRecord('R', 0x8ce1c9),
            new LargeRecord('S', 0x8ce1cf),
            new LargeRecord('T', 0x8ce1d5),
            new LargeRecord('U', 0x8ce1db),
            new LargeRecord('Y', 0x8ce1e1),
            new LargeRecord(' ', 0x8ce1e7),
        ];
        foreach (var record in smallRecords)
        {
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(record.Address), EndingTextGlyphs.CompileGlyph(record.Glyph, EndingTextStyle.PercentageSmall).PackedWord, "native named glyph record");
        }
        // Original indirect text records and producer/copyright panels establish attributes/blanks.
        ushort percentageAttributes = (ushort)(CartridgeImportSource.Require(bus).ReadCartridgeWord(0x8ce18d) & 0xfc00);
        ushort finalAttributes = (ushort)(CartridgeImportSource.Require(bus).ReadCartridgeWord(0x8ce13d) & 0xfc00);
        foreach (EndingTextStyle style in Enum.GetValues<EndingTextStyle>())
        foreach (bool bottom in new[] { false, true })
        {
            bool large = style is EndingTextStyle.CopyrightLarge or EndingTextStyle.FinalLarge;
            ushort attributes = style == EndingTextStyle.PercentageSmall ? percentageAttributes :
                style == EndingTextStyle.FinalLarge ? finalAttributes : (ushort)0;
            ushort blank = CartridgeImportSource.Require(bus).ReadCartridgeWord((style switch
            {
                EndingTextStyle.ResultSmall => 0x8cdc9b,
                EndingTextStyle.CopyrightLarge => 0x8cdedb,
                EndingTextStyle.PercentageSmall => 0x8ce1e7,
                EndingTextStyle.FinalLarge => bottom ? 0x8ce137 : 0x8ce135,
                _ => bottom ? 0x8ce137 : 0x8ce135,
            }));
            var expected = new Dictionary<ushort, char> { [blank] = ' ' };
            for (int letter = 0; letter < 26; letter++)
                expected.Add((ushort)(attributes | (large ? (bottom ? lower[letter] : top[letter]) : small[letter])), (char)('A' + letter));
            if (style == EndingTextStyle.CopyrightLarge)
                for (int digit = 0; digit < 10; digit++)
                    expected.Add(bottom ? digitsBottom[digit] : digitsTop[digit], (char)('0' + digit));
            foreach (var entry in expected)
                AssertEqual(entry.Key, EndingTextGlyphs.CompileGlyph(entry.Value, style, bottom).PackedWord, $"ending {style} glyph {entry.Value} half {bottom}, encoded through the codec");
            for (int word = 0; word <= ushort.MaxValue; word++)
            {
                if (expected.TryGetValue((ushort)word, out char glyph))
                    AssertEqual(glyph, EndingTextGlyphs.DecodeGlyph(BackgroundTileWordBytes.Tile(word), style, bottom), "ending glyph inverse over the tile each word places");
                else
                    AssertThrows<InvalidDataException>(() => EndingTextGlyphs.DecodeGlyph(BackgroundTileWordBytes.Tile(word), style, bottom), "ending glyph rejects the tile of a non-glyph word");
            }
            foreach (char invalid in new[] { '\0', '@', '[', 'a', '/', ':', '\uffff' })
                AssertThrows<ArgumentOutOfRangeException>(() => EndingTextGlyphs.CompileGlyph(invalid, style, bottom), "ending glyph character bounds");
            if (style != EndingTextStyle.CopyrightLarge)
                for (char digit = '0'; digit <= '9'; digit++)
                    AssertThrows<ArgumentOutOfRangeException>(() => EndingTextGlyphs.CompileGlyph(digit, style, bottom), "ending glyph digits restricted to copyright");
        }
        foreach (EndingTextStyle invalid in new[] { (EndingTextStyle)4, (EndingTextStyle)255 })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingTextGlyphs.CompileGlyph('A', invalid), "ending glyph style bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingTextGlyphs.DecodeGlyph(BackgroundTileWordBytes.Tile(0), invalid), "ending decoder style bounds");
        }
    }
}

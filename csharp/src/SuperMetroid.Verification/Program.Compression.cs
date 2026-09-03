using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifySmCompressionFormat()
    {
        foreach (SmCompressionCommand command in Enum.GetValues<SmCompressionCommand>())
        {
            // $E0-$FE are the expanded-header marker range and $FF terminates the stream,
            // so command seven has no short form at all.
            if (command != SmCompressionCommand.RelativeCopyInverted)
            {
                byte shortHeader = unchecked((byte)(
                    (byte)command | (SmCompressionFormat.MaximumShortLength - 1)));
                SmCompressionHeader shortDecoded = SmCompressionHeader.Decode(shortHeader);
                AssertEqual(command, shortDecoded.Command, $"{command} short command field");
                AssertEqual(SmCompressionFormat.MaximumShortLength, shortDecoded.Length,
                    $"{command} short maximum length");
            }

            const int longLength = 33;
            int encodedLength = longLength - 1;
            byte longHeader = unchecked((byte)(
                SmCompressionFormat.LongHeaderMarker |
                ((byte)command >> 5 << 2) |
                (encodedLength >> 8)));
            SmCompressionHeader longDecoded = SmCompressionHeader.Decode(
                longHeader,
                unchecked((byte)encodedLength));
            AssertEqual(command, longDecoded.Command, $"{command} long command field");
            AssertEqual(longLength, longDecoded.Length, $"{command} long minimum length");
        }

        SmCompressionHeader maximum = SmCompressionHeader.Decode(0xe3, 0xff);
        AssertEqual(SmCompressionFormat.MaximumLongLength, maximum.Length,
            "long compression header maximum length");
        AssertTrue(SmCompressionHeader.Decode(SmCompressionFormat.Terminator).IsTerminator,
            "compression terminator is distinct from command-seven long header");

        byte[] stream =
        [
            0x03, 0x10, 0x20, 0x30, 0x40,       // literal four
            0x22, 0xaa,                         // byte fill three
            0x43, 0x01, 0x02,                   // alternating fill four
            0x62, 0x05,                         // incrementing fill three
            0x83, 0x00, 0x00,                   // absolute copy four
            0xa1, 0x00, 0x00,                   // inverted absolute copy two
            0xc2, 0x04,                         // relative copy three
            0xfc, 0x01, 0x01,                   // long inverted relative copy two
            SmCompressionFormat.Terminator,
        ];
        AssertSequenceEqual(
            new byte[]
            {
                0x10, 0x20, 0x30, 0x40,
                0xaa, 0xaa, 0xaa,
                0x01, 0x02, 0x01, 0x02,
                0x05, 0x06, 0x07,
                0x10, 0x20, 0x30, 0x40,
                0xef, 0xdf,
                0x30, 0x40, 0xef,
                0x10, 0xef,
            },
            SmCompression.Decompress(stream),
            "all eight compression command forms decode in one stream");

        AssertThrows<InvalidDataException>(
            () => SmCompression.Decompress([0xe0]),
            "truncated long header fails loudly");
        AssertThrows<InvalidDataException>(
            () => SmCompression.Decompress([0xc0, 0x00, SmCompressionFormat.Terminator]),
            "zero-distance relative copy fails loudly");
        AssertThrows<InvalidDataException>(
            () => SmCompression.Decompress(
                [0x20, 0xaa, SmCompressionFormat.Terminator],
                maximumOutputBytes: 0),
            "compression output ceiling fails loudly");

        Console.WriteLine(
            "  Compression: typed headers, all eight commands, length boundaries, and strict failures agree.");
    }
}

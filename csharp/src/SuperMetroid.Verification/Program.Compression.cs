using SuperMetroid.Core.Assets;

internal static partial class Program
{
    private static void VerifySmCompressionFormat()
    {
        // All eight three-bit command codes are named members.
        AssertEqual(8, Enum.GetValues<SmCompressionCommand>().Length, "compression command domain size");
        for (int code = 0; code < 8; code++)
        {
            var command = (SmCompressionCommand)(code << 5);
            AssertTrue(Enum.IsDefined(command), $"command {code} is a named member");
            // $E0-$FE are the expanded-header marker range and $FF terminates the stream,
            // so command seven has no short form at all.
            if (code != 7)
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
        SmCompressionHeader terminator = SmCompressionHeader.Decode(SmCompressionFormat.Terminator);
        AssertTrue(terminator.IsTerminator, "compression terminator is distinct from command-seven long header");
        AssertEqual(0, terminator.PayloadByteCount, "terminator reads no payload");
        AssertThrows<InvalidOperationException>(() => _ = terminator.Command, "terminator has no command");

        // Payload size and copy behavior of every command, explicitly per member.
        foreach (var (command, payload, copy, relative, inverted) in new (SmCompressionCommand, int, bool, bool, bool)[]
        {
            (SmCompressionCommand.Literal, 33, false, false, false),
            (SmCompressionCommand.RepeatByte, 1, false, false, false),
            (SmCompressionCommand.AlternatePair, 2, false, false, false),
            (SmCompressionCommand.IncrementingSequence, 1, false, false, false),
            (SmCompressionCommand.AbsoluteCopy, 2, true, false, false),
            (SmCompressionCommand.AbsoluteCopyInverted, 2, true, false, true),
            (SmCompressionCommand.RelativeCopy, 1, true, true, false),
            (SmCompressionCommand.RelativeCopyInverted, 1, true, true, true),
        })
        {
            SmCompressionHeader header = SmCompressionHeader.Decode(
                unchecked((byte)(SmCompressionFormat.LongHeaderMarker | ((byte)command >> 5 << 2))), 32);
            AssertEqual(command, header.Command, $"{command} long header");
            AssertEqual(payload, header.PayloadByteCount, $"{command} payload bytes");
            if (copy)
            {
                AssertEqual(relative, header.IsRelativeCopy, $"{command} relative copy");
                AssertEqual(inverted, header.InvertsCopiedBytes, $"{command} inverts copied bytes");
            }
            else
            {
                AssertThrows<InvalidOperationException>(() => _ = header.IsRelativeCopy, $"{command} has no copy distance");
                AssertThrows<InvalidOperationException>(() => _ = header.InvertsCopiedBytes, $"{command} has no copy inversion");
            }
        }

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

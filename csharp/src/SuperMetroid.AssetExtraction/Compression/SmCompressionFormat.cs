namespace SuperMetroid.Core.Assets;

/// <summary>Lossless decoder for one Super Metroid compressed-stream command header.</summary>
public readonly record struct SmCompressionHeader
{
    private SmCompressionHeader(byte firstByte, byte? secondByte)
    {
        FirstByte = firstByte;
        SecondByte = secondByte;
        IsTerminator = firstByte == SmCompressionFormat.Terminator;
        if (IsTerminator)
        {
            Length = 0;
            HeaderByteCount = 1;
            return;
        }

        bool isLong = SmCompressionFormat.IsLongHeader(firstByte);
        if (isLong && secondByte is null)
            throw new InvalidDataException("Long compression header is missing its length byte.");
        // Masking to bits 5-7 yields one of the eight defined commands.
        Command = isLong
            ? (SmCompressionCommand)((firstByte <<
                SmCompressionFormat.LongCommandShift) & SmCompressionFormat.CommandMask)
            : (SmCompressionCommand)(firstByte & SmCompressionFormat.CommandMask);
        Length = isLong
            ? (((firstByte & SmCompressionFormat.LongLengthHighMask) << 8) |
                secondByte!.Value) + 1
            : (firstByte & SmCompressionFormat.ShortLengthMask) + 1;
        HeaderByteCount = isLong ? 2 : 1;
    }

    /// <summary>The original command-and-length byte, or <c>$FF</c> for the stream terminator.</summary>
    public byte FirstByte { get; }
    /// <summary>The supplied low length byte for a long header; short headers do not require it.</summary>
    public byte? SecondByte { get; }
    /// <summary>The command bits normalized into bits 5-7.</summary>
    /// <exception cref="InvalidOperationException">The header is the stream terminator, which has no command.</exception>
    public SmCompressionCommand Command
    {
        get => IsTerminator
            ? throw new InvalidOperationException("The compressed-stream terminator has no command.")
            : field;
    }
    /// <summary>Number of decompressed bytes emitted: 1-32 for short headers, 1-1024 for long headers, or zero for a terminator.</summary>
    public int Length { get; }
    /// <summary>Number of encoded header bytes consumed: two for a long header, otherwise one.</summary>
    public int HeaderByteCount { get; }
    /// <summary>Encoded operand bytes after this header, independent of expanded run length.</summary>
    public int PayloadByteCount => IsTerminator ? 0 : Command switch
    {
        SmCompressionCommand.Literal => Length,
        SmCompressionCommand.AlternatePair or SmCompressionCommand.AbsoluteCopy or
            SmCompressionCommand.AbsoluteCopyInverted => 2,
        SmCompressionCommand.RepeatByte or SmCompressionCommand.IncrementingSequence or
            SmCompressionCommand.RelativeCopy or SmCompressionCommand.RelativeCopyInverted => 1,
        _ => throw UndefinedCommand(Command),
    };
    /// <summary>Whether this header ends the compressed stream without emitting output or reading a payload.</summary>
    public bool IsTerminator { get; }
    /// <summary>Whether a copy command uses a one-byte backward distance instead of a two-byte absolute output offset.</summary>
    public bool IsRelativeCopy => Command switch
    {
        SmCompressionCommand.AbsoluteCopy or SmCompressionCommand.AbsoluteCopyInverted => false,
        SmCompressionCommand.RelativeCopy or SmCompressionCommand.RelativeCopyInverted => true,
        SmCompressionCommand.Literal or SmCompressionCommand.RepeatByte or
            SmCompressionCommand.AlternatePair or SmCompressionCommand.IncrementingSequence =>
            throw new InvalidOperationException($"{Command} is not a copy command."),
        _ => throw UndefinedCommand(Command),
    };
    /// <summary>Whether a copy command XORs every copied byte with <c>$FF</c> (commands 5 and 7).</summary>
    public bool InvertsCopiedBytes => Command switch
    {
        SmCompressionCommand.AbsoluteCopy or SmCompressionCommand.RelativeCopy => false,
        SmCompressionCommand.AbsoluteCopyInverted or SmCompressionCommand.RelativeCopyInverted => true,
        SmCompressionCommand.Literal or SmCompressionCommand.RepeatByte or
            SmCompressionCommand.AlternatePair or SmCompressionCommand.IncrementingSequence =>
            throw new InvalidOperationException($"{Command} is not a copy command."),
        _ => throw UndefinedCommand(Command),
    };

    private static ArgumentOutOfRangeException UndefinedCommand(SmCompressionCommand command) =>
        new(nameof(command), command, $"Undefined compression command ${(byte)command:X2}.");

    /// <summary>Decodes the command and expanded run length without consuming or validating its payload.</summary>
    /// <param name="firstByte">The first encoded header byte, including the optional <c>$FF</c> terminator.</param>
    /// <param name="secondByte">The required low length byte for a long header; ignored for short headers and terminators.</param>
    /// <returns>A header retaining the supplied bytes and exposing their decoded fields.</returns>
    /// <exception cref="InvalidDataException">A long header is supplied without its second byte.</exception>
    public static SmCompressionHeader Decode(byte firstByte, byte? secondByte = null) =>
        new(firstByte, secondByte);
}

/// <summary>Bit layout and bounds shared by every command in the cartridge format.</summary>
public static class SmCompressionFormat
{
    /// <summary><c>$FF</c>: ends a compressed stream; this value takes precedence over the long-header marker.</summary>
    public const byte Terminator = 0xff;
    /// <summary><c>$E0</c>: selects the upper three bits used to recognize a long command header.</summary>
    public const byte LongHeaderMarkerMask = 0xe0;
    /// <summary><c>$E0</c>: all three marker bits must be set to introduce a second length byte.</summary>
    public const byte LongHeaderMarker = 0xe0;
    /// <summary><c>$E0</c>: selects command bits in a short header or in the normalized long-header command.</summary>
    public const byte CommandMask = 0xe0;
    /// <summary><c>$1F</c>: selects the short header's five-bit output length minus one.</summary>
    public const byte ShortLengthMask = 0x1f;
    /// <summary><c>$03</c>: selects the high two bits of a long header's ten-bit output length minus one.</summary>
    public const byte LongLengthHighMask = 0x03;
    /// <summary>Left shift of three moves a long header's command field from bits 2-4 into normalized bits 5-7.</summary>
    public const int LongCommandShift = 3;
    /// <summary>Default decompression safety cap of 4 MiB, limiting expansion of malformed asset streams.</summary>
    public const int DefaultMaximumOutputBytes = 4 * 1024 * 1024;

    /// <summary>Checks whether a byte begins a two-byte header, excluding the <c>$FF</c> stream terminator.</summary>
    /// <param name="value">The first byte of a prospective command header.</param>
    /// <returns>True when bits 5-7 are set and the byte is not the terminator.</returns>
    public static bool IsLongHeader(byte value) =>
        value != Terminator &&
        (value & LongHeaderMarkerMask) == LongHeaderMarker;
}

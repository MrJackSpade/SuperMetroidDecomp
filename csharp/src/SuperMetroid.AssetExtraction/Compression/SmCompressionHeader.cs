using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Compression;

/// <summary>Lossless decoder for one Super Metroid compressed-stream command header.</summary>
[EncodingOwner("Super Metroid compression command header")]
public readonly record struct SmCompressionHeader
{
    /// <summary>The commands by their three-bit command number.</summary>
    private static readonly SmCompressionCommand[] commandsByNumber =
    [
        SmCompressionCommand.Literal, SmCompressionCommand.RepeatByte, SmCompressionCommand.AlternatePair,
        SmCompressionCommand.IncrementingSequence, SmCompressionCommand.AbsoluteCopy, SmCompressionCommand.AbsoluteCopyInverted,
        SmCompressionCommand.RelativeCopy, SmCompressionCommand.RelativeCopyInverted,
    ];

    /// <summary>Decodes the short, long, or terminator header.</summary>
    private SmCompressionHeader(int firstByte, int? secondByte)
    {
        firstByte = Ensure.BetweenInclusive(0, byte.MaxValue, firstByte);
        IsTerminator = firstByte == SmCompressionCodes.Terminator;
        if (IsTerminator)
        {
            Length = 0;
            HeaderByteCount = 1;
            return;
        }

        bool isLong = SmCompressionCodes.IsLongHeader(firstByte);
        // Masking to bits 5-7 yields one of the eight defined commands.
        int normalizedCommand = isLong
            ? (firstByte << SmCompressionCodes.LongCommandShift) & SmCompressionCodes.CommandMask
            : firstByte & SmCompressionCodes.CommandMask;
        Command = CommandOfNumber(normalizedCommand >> SmCompressionCodes.CommandFieldShift);
        Length = isLong
            ? LittleEndianBytes.Word(
                checked((byte)(secondByte ?? throw new InvalidDataException("Long compression header is missing its length byte."))),
                (byte)(firstByte & SmCompressionCodes.LongLengthHighMask)) + 1
            : (firstByte & SmCompressionCodes.ShortLengthMask) + 1;
        HeaderByteCount = isLong ? 2 : 1;
    }

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

    /// <summary>Reports an encoded command value outside the eight supported compression operations.</summary>
    private static ArgumentOutOfRangeException UndefinedCommand(SmCompressionCommand command) =>
        new(nameof(command), command, $"Undefined compression command {command}.");

    /// <summary>The command a three-bit command number, 0-7, selects.</summary>
    /// <param name="number">The command field's value.</param>
    /// <returns>The command the number selects.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="number"/> is outside 0-7.</exception>
    public static SmCompressionCommand CommandOfNumber(int number) =>
        commandsByNumber[Ensure.BetweenInclusive(0, commandsByNumber.Length - 1, number)];

    /// <summary>Decodes the command and expanded run length without consuming or validating its payload.</summary>
    /// <param name="firstByte">The first encoded header byte, including the optional <c>$FF</c> terminator.</param>
    /// <param name="secondByte">The required low length byte for a long header; ignored for short headers and terminators.</param>
    /// <returns>A header retaining the supplied bytes and exposing their decoded fields.</returns>
    /// <exception cref="InvalidDataException">A long header is supplied without its second byte.</exception>
    public static SmCompressionHeader Decode(int firstByte, int? secondByte = null) =>
        new(firstByte, secondByte);
}

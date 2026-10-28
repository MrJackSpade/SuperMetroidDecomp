using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Compression;

/// <summary>Lossless decoder for one Super Metroid compressed-stream command header.</summary>
[EncodingOwner("Super Metroid compression command header")]
public readonly record struct SmCompressionHeader
{
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
        Command = isLong
            ? (SmCompressionCommand)((firstByte <<
                SmCompressionCodes.LongCommandShift) & SmCompressionCodes.CommandMask)
            : (SmCompressionCommand)(firstByte & SmCompressionCodes.CommandMask);
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
        new(nameof(command), command, $"Undefined compression command ${(byte)command:X2}.");

    /// <summary>Decodes the command and expanded run length without consuming or validating its payload.</summary>
    /// <param name="firstByte">The first encoded header byte, including the optional <c>$FF</c> terminator.</param>
    /// <param name="secondByte">The required low length byte for a long header; ignored for short headers and terminators.</param>
    /// <returns>A header retaining the supplied bytes and exposing their decoded fields.</returns>
    /// <exception cref="InvalidDataException">A long header is supplied without its second byte.</exception>
    public static SmCompressionHeader Decode(int firstByte, int? secondByte = null) =>
        new(firstByte, secondByte);
}

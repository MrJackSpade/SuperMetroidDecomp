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
            Command = default;
            Length = 0;
            HeaderByteCount = 1;
            return;
        }

        bool isLong = SmCompressionFormat.IsLongHeader(firstByte);
        if (isLong && secondByte is null)
            throw new InvalidDataException("Long compression header is missing its length byte.");
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

    public byte FirstByte { get; }
    public byte? SecondByte { get; }
    public SmCompressionCommand Command { get; }
    public int Length { get; }
    public int HeaderByteCount { get; }
    public bool IsTerminator { get; }
    public bool IsCopy => (byte)Command >= (byte)SmCompressionCommand.AbsoluteCopy;
    public bool IsRelativeCopy => (byte)Command >= (byte)SmCompressionCommand.RelativeCopy;
    public bool InvertsCopiedBytes =>
        ((byte)Command & SmCompressionFormat.InvertedCopyBit) != 0;

    public static SmCompressionHeader Decode(byte firstByte, byte? secondByte = null) =>
        new(firstByte, secondByte);
}

/// <summary>Bit layout and bounds shared by every command in the cartridge format.</summary>
public static class SmCompressionFormat
{
    public const byte Terminator = 0xff;
    public const byte LongHeaderMarkerMask = 0xe0;
    public const byte LongHeaderMarker = 0xe0;
    public const byte CommandMask = 0xe0;
    public const byte ShortLengthMask = 0x1f;
    public const byte LongLengthHighMask = 0x03;
    public const int LongCommandShift = 3;
    public const byte InvertedCopyBit = 0x20;
    public const int MaximumShortLength = 32;
    public const int MaximumLongLength = 1024;
    // Command seven has no short form: $E0-$FE introduce expanded headers and $FF is the
    // terminator. It also loses the final expanded range because $FF cannot be a header.
    public const int MaximumInvertedRelativeLongLength = 768;
    public const int DefaultMaximumOutputBytes = 4 * 1024 * 1024;

    public static bool IsLongHeader(byte value) =>
        value != Terminator &&
        (value & LongHeaderMarkerMask) == LongHeaderMarker;
}

namespace SuperMetroid.Core.Frontend;

/// <summary>One immutable eight-byte transfer record from <c>$8B:E45A</c>.</summary>
internal readonly record struct EndingPostShotUploadDefinition(
    ushort Length, int SourceAddress, ushort DestinationWord);

/// <summary>
/// Compiled six-transfer schedule for Func142's post-credits Super Metroid logo.
/// Artwork bytes remain separate presentation assets; this table fixes only
/// their native ordering, transfer lengths, and VRAM destinations.
/// </summary>
internal static class EndingPostShotUploadDefinitions
{
    /// <summary>First eight-byte transfer record at <c>$8B:E45A</c>.</summary>
    public const int TableAddress = 0x8be45a;

    /// <summary>Native record size: length, long source with padding, destination word.</summary>
    public const int RecordBytes = 8;

    /// <summary>Func142 uploads the subtitle, four logo-tile chunks, then the logo map.</summary>
    public const int Count = 6;

    /// <summary>Subtitle font transfer length at <c>$8B:E45A</c>.</summary>
    private const ushort SubtitleLength = 0x0400;

    /// <summary>Logo-tile chunk length at <c>$8B:E462</c> through <c>$8B:E47A</c>.</summary>
    private const ushort LogoTileChunkLength = 0x0800;

    /// <summary>Logo-map transfer length at <c>$8B:E482</c>.</summary>
    private const ushort LogoMapLength = 0x0800;

    /// <summary>Subtitle destination VRAM word from the first record.</summary>
    private const ushort SubtitleDestinationWord = 0x4800;

    /// <summary>First logo-tile destination VRAM word from the second record.</summary>
    private const ushort LogoTileDestinationWord = 0x6000;

    /// <summary>Logo-map destination VRAM word from the sixth record.</summary>
    private const ushort LogoMapDestinationWord = 0x5400;

    private static readonly EndingPostShotUploadDefinition[] Records =
    [
        new(SubtitleLength, EndingPostShotDefinitions.SubtitleSource,
            SubtitleDestinationWord),
        new(LogoTileChunkLength, EndingPostShotDefinitions.LogoTileSource,
            LogoTileDestinationWord),
        new(LogoTileChunkLength, EndingPostShotDefinitions.LogoTileSource + LogoTileChunkLength,
            LogoTileDestinationWord + LogoTileChunkLength / sizeof(ushort)),
        new(LogoTileChunkLength, EndingPostShotDefinitions.LogoTileSource + 2 * LogoTileChunkLength,
            LogoTileDestinationWord + LogoTileChunkLength),
        new(LogoTileChunkLength, EndingPostShotDefinitions.LogoTileSource + 3 * LogoTileChunkLength,
            LogoTileDestinationWord + 3 * LogoTileChunkLength / sizeof(ushort)),
        new(LogoMapLength, EndingPostShotDefinitions.LogoMapSource,
            LogoMapDestinationWord),
    ];

    /// <summary>Returns one of the six native transfer records; unknown indexes fail.</summary>
    public static EndingPostShotUploadDefinition Get(int index) =>
        (uint)index < Records.Length ? Records[index] :
            throw new ArgumentOutOfRangeException(nameof(index));
}

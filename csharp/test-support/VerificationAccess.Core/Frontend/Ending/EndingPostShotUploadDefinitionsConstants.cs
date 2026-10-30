using SuperMetroid.AssetExtraction.Frontend.Ending;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Cartridge values of <see cref="EndingPostShotUploadDefinitions"/> that only verification reads.</summary>
internal static class EndingPostShotUploadDefinitionsConstants
{
    /// <summary>Native record size: length, long source with padding, destination word.</summary>
    public const int RecordBytes = 8;
    /// <summary>First eight-byte transfer record at <c>$8B:E45A</c>.</summary>
    public const int TableAddress = 0x8be45a;

    /// <summary>Provides the ending post-shot upload table and record width for decoding its transfers.</summary>
    extension(EndingPostShotUploadDefinitions)
    {
        /// <inheritdoc cref="EndingPostShotUploadDefinitionsConstants.RecordBytes"/>
        internal static int RecordBytes => EndingPostShotUploadDefinitionsConstants.RecordBytes;
        /// <inheritdoc cref="EndingPostShotUploadDefinitionsConstants.TableAddress"/>
        internal static int TableAddress => EndingPostShotUploadDefinitionsConstants.TableAddress;
    }
}

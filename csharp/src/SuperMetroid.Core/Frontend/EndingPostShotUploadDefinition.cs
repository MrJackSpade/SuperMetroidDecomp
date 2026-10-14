namespace SuperMetroid.Core.Frontend;

/// <summary>One immutable eight-byte transfer record from <c>$8B:E45A</c>.</summary>
internal readonly record struct EndingPostShotUploadDefinition(
    ushort Length, int SourceAddress, ushort DestinationWord, EndingPostShotUploadSource Source);

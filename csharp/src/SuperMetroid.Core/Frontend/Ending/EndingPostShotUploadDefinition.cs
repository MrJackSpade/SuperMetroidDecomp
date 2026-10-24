namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One immutable eight-byte transfer record from <c>$8B:E45A</c>: what it shows and how many bytes of it.</summary>
/// <param name="Length">The bytes the part shows.</param>
/// <param name="SourceAddress">The part's cartridge source, which names its installed artwork.</param>
/// <param name="Source">Which artwork the part shows.</param>
internal readonly record struct EndingPostShotUploadDefinition(
    ushort Length, int SourceAddress, EndingPostShotUploadSource Source);

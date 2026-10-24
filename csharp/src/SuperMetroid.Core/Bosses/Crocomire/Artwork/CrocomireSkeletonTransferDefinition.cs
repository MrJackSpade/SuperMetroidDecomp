namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>One native Crocomire skeleton-character chunk.</summary>
/// <param name="SourceAddress">The chunk's cartridge source, which names its installed characters.</param>
internal readonly record struct CrocomireSkeletonTransferDefinition(int SourceAddress);

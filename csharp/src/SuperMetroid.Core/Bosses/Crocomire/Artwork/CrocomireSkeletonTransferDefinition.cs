namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>One native Crocomire skeleton-character upload, with its fixed place among the OBJ characters.</summary>
/// <param name="SourceAddress">The chunk's cartridge source, which names its installed characters.</param>
/// <param name="FirstCharacter">The OBJ character the chunk begins at.</param>
internal readonly record struct CrocomireSkeletonTransferDefinition(
    int SourceAddress, int FirstCharacter);

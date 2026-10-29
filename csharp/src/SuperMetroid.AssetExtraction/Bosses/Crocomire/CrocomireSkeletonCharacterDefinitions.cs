using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Crocomire;

/// <summary>
/// The installed skeleton characters Crocomire's death shows, chunk by chunk (#1463): the import pipeline's knowledge of
/// the bytes each skeleton chunk look holds, which gameplay names only by its place in the sequence.
/// </summary>
internal static class CrocomireSkeletonCharacterDefinitions
{
    /// <summary>The bytes of one skeleton character chunk.</summary>
    internal const int ChunkByteCount = 512;

    /// <summary>The bytes of all the skeleton's character chunks.</summary>
    internal const int TotalByteCount = ChunkByteCount * CrocomireSkeletonTransferDefinitions.FrameCount;

    /// <summary>Editable source image containing the extracted skeleton character tiles.</summary>
    internal const string FileName = "crocomire-skeleton-tiles.png";
}

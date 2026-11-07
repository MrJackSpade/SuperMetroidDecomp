namespace SuperMetroid.AssetExtraction.Bosses.Crocomire;

/// <summary>
/// The installed skeleton characters Crocomire's death shows, chunk by chunk (#1463): the import pipeline's knowledge of
/// the bytes each skeleton chunk holds, which the skeleton look's finished pictures are composed from (#1538).
/// </summary>
internal static class CrocomireSkeletonCharacterDefinitions
{
    /// <summary>The skeleton character chunks Crocomire's death loads, in the order it loads them.</summary>
    internal const int ChunkCount = 6;

    /// <summary>The bytes of one skeleton character chunk.</summary>
    internal const int ChunkByteCount = 512;

    /// <summary>The bytes of all the skeleton's character chunks.</summary>
    internal const int TotalByteCount = ChunkByteCount * ChunkCount;

    /// <summary>Editable source image containing the extracted skeleton character tiles.</summary>
    internal const string FileName = "crocomire-skeleton-tiles.png";
}

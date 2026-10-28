using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The skeleton's six character uploads, in the order the death sequence shows them; a seventh entry ends the
/// sequence. Where each chunk comes from in the cartridge is extraction's knowledge.
/// </summary>
internal static class CrocomireSkeletonTransferDefinitions
{
    /// <summary>The bytes of one skeleton character chunk.</summary>
    internal const int ChunkByteCount = 512;
    /// <summary>The bytes of all six skeleton character chunks.</summary>
    internal const int TotalByteCount = ChunkByteCount * FrameCount;
    /// <summary>Editable source image containing the extracted skeleton character tiles.</summary>
    internal const string FileName = "crocomire-skeleton-tiles.png";

    /// <summary>The six chunks before the end of the sequence: four rows of the first character region and two of the second.</summary>
    internal const int FrameCount = 6;

    /// <summary>The six skeleton chunks, in upload order (#1392, #1406).</summary>
    internal static IReadOnlyList<SpriteArtworkSource> Sources { get; } = [.. Enumerable.Range(0, FrameCount).Select(index => new CrocomireSkeletonChunkCharacters(index))];
}

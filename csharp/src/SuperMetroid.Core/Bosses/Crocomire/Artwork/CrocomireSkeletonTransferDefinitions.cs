using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>
/// The skeleton chunk looks Crocomire's death shows, in the order the death sequence shows them; a seventh entry ends the
/// sequence. What each chunk holds and where it lands is the import pipeline's knowledge.
/// </summary>
internal static class CrocomireSkeletonTransferDefinitions
{
    /// <summary>The chunks the sequence shows before its end.</summary>
    internal const int FrameCount = 6;

    /// <summary>The six skeleton chunks, in the order shown (#1392, #1406).</summary>
    internal static IReadOnlyList<SpriteArtworkSource> Sources { get; } = [.. Enumerable.Range(0, FrameCount).Select(index => new CrocomireSkeletonChunkCharacters(index))];
}

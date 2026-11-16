using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Development-tool members of <see cref="EnemyExtendedFrameDefinitions"/>; never linked by player hosts. Each
/// family's place is read from the document order at its first frame.
/// </summary>
internal static class EnemyExtendedFrameToolingDefinitions
{
    /// <summary>Frames the document lists before Crocomire's corpse and skeleton composite frames.</summary>
    internal static int PreCrocomireSkeletonFrameCount => FramesBefore(EnemyFrameId.CrocomireBodySkeletonToppling);
    /// <summary>Frames the document lists before Kraid's thirty-five extended foot frames.</summary>
    internal static int PreKraidFootFrameCount => FramesBefore(EnemyFrameId.KraidFootPlantedTwin);

    /// <summary>The number of frames the extended enemy frame document lists before <paramref name="first"/>.</summary>
    /// <exception cref="InvalidOperationException">The document does not list <paramref name="first"/>.</exception>
    internal static int FramesBefore(EnemyFrameId first)
    {
        int place = EnemyExtendedFrameDocumentOrder.Frames.IndexOf(first);
        return place >= 0 ? place : throw new InvalidOperationException($"The extended enemy frame document does not list {first}.");
    }
}

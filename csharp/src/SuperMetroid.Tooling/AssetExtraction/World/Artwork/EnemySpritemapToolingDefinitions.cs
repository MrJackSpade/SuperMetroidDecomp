using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// Development-tool members of <see cref="EnemySpritemapDefinitions"/>; never linked by player hosts. Each family's
/// place is read from the document order at its first frame, so deleting or merging a frame moves every later place
/// with it instead of leaving a hand-kept count behind.
/// </summary>
internal static class EnemySpritemapToolingDefinitions
{
    /// <summary>Frames the document lists before the Puromi/Nuclear Waffle head frames.</summary>
    internal static int PreNuclearWaffleFrameCount => FramesBefore(EnemyFrameId.NuclearWaffleBodyTurned0);
    /// <summary>Frames the document lists before Kraid's two belly-lint frames.</summary>
    internal static int PreKraidLintFrameCount => FramesBefore(EnemyFrameId.KraidLintBudding);
    /// <summary>Frames the document lists before the audited environmental and Tourian artwork, Hibashi first.</summary>
    internal static int PreAuditOrdinaryFrameCount => FramesBefore(EnemyFrameId.HibashiEmber);
    /// <summary>Frames the document lists before the friendly-animal compositions, the Etecoon first.</summary>
    internal static int PreFriendlyAnimalFrameCount => FramesBefore(EnemyFrameId.EtecoonLookingRight);
    /// <summary>Frames the document lists before the Zero crawler artwork.</summary>
    internal static int PreZeroFrameCount => FramesBefore(EnemyFrameId.ZeroOnLeftWallContracted);
    /// <summary>Frames the document lists before the tatori family.</summary>
    internal static int PreMamaTurtleFrameCount => FramesBefore(EnemyFrameId.TurtleBabyCrawlingLeftLegsTucked);

    /// <summary>The number of frames the enemy spritemap document lists before <paramref name="first"/>.</summary>
    /// <exception cref="InvalidOperationException">The document does not list <paramref name="first"/>.</exception>
    internal static int FramesBefore(EnemyFrameId first)
    {
        int place = EnemySpritemapDocumentOrder.Frames.IndexOf(first);
        return place >= 0 ? place : throw new InvalidOperationException($"The enemy spritemap document does not list {first}.");
    }
}

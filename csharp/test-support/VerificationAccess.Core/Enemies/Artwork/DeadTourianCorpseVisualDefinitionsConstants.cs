namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values of <see cref="DeadTourianCorpseVisualDefinitions"/> that only verification reads.</summary>
internal static class DeadTourianCorpseVisualDefinitionsConstants
{
    /// <summary>
    /// Five corpse images precede the Sidehopper images in the extracted atlas: one per way a dead Zoomer, Ripper or Skree
    /// lies (#1810).
    /// </summary>
    public const int CorpseFrameCount = 5;
    /// <summary>Total images in the shared corpse and Sidehopper atlas.</summary>
    public const int FrameCount = CorpseFrameCount + SidehopperOwnFrameCount;
    /// <summary>
    /// Five records the dead Sidehopper's programs show: its two sprawled pictures, and the large Sidehopper's three legs,
    /// which its bank's records draw from the large Sidehopper's tiles (#2327).
    /// </summary>
    public const int SidehopperFrameCount = 5;
    /// <summary>The dead Sidehopper's two sprawled pictures follow the corpse images in the extracted atlas.</summary>
    public const int SidehopperOwnFrameCount = 2;

    /// <summary>Provides the native corpse and Sidehopper frame counts used to check dead Tourian visual imports.</summary>
    extension(DeadTourianCorpseVisualDefinitions)
    {
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.CorpseFrameCount"/>
        internal static int CorpseFrameCount => DeadTourianCorpseVisualDefinitionsConstants.CorpseFrameCount;
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => DeadTourianCorpseVisualDefinitionsConstants.FrameCount;
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.SidehopperFrameCount"/>
        internal static int SidehopperFrameCount => DeadTourianCorpseVisualDefinitionsConstants.SidehopperFrameCount;    }
}

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values of <see cref="DeadTourianCorpseVisualDefinitions"/> that only verification reads.</summary>
internal static class DeadTourianCorpseVisualDefinitionsConstants
{
    /// <summary>Eight corpse images precede the Sidehopper images in the extracted atlas.</summary>
    public const int CorpseFrameCount = 8;
    /// <summary>Total images in the shared corpse and Sidehopper atlas.</summary>
    public const int FrameCount = CorpseFrameCount + SidehopperFrameCount;
    /// <summary>Five Sidehopper images follow the corpse images in the extracted atlas.</summary>
    public const int SidehopperFrameCount = 5;

    /// <summary>Provides the native corpse and Sidehopper frame counts used to check dead Tourian visual imports.</summary>
    extension(DeadTourianCorpseVisualDefinitions)
    {
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.CorpseFrameCount"/>
        internal static int CorpseFrameCount => DeadTourianCorpseVisualDefinitionsConstants.CorpseFrameCount;
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => DeadTourianCorpseVisualDefinitionsConstants.FrameCount;
        /// <inheritdoc cref="DeadTourianCorpseVisualDefinitionsConstants.SidehopperFrameCount"/>
        internal static int SidehopperFrameCount => DeadTourianCorpseVisualDefinitionsConstants.SidehopperFrameCount;
    }
}

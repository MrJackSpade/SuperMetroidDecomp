namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values of <see cref="EvirVisualDefinitions"/> that only verification reads.</summary>
internal static class EvirVisualDefinitionsConstants
{
    /// <summary>Number of Evir animation images expected by the extracted visual sheet.</summary>
    public const int FrameCount = 24;

    /// <summary>Provides the expected number of Evir visual frames for imported animation checks.</summary>
    extension(EvirVisualDefinitions)
    {
        /// <inheritdoc cref="EvirVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => EvirVisualDefinitionsConstants.FrameCount;
    }
}

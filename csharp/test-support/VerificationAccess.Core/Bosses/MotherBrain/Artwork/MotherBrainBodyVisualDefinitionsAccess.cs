namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Verification access to <see cref="MotherBrainBodyVisualDefinitions"/> members production does not use.</summary>
internal static class MotherBrainBodyVisualDefinitionsAccess
{
    /// <summary>Enumerates all compiled Mother Brain body-frame definitions.</summary>
    extension(MotherBrainBodyVisualDefinitions)
    {
        /// <summary>Materializes the indexed frame definitions in animation order.</summary>
        internal static NativeEnemySpritemapDefinition[] Frames =>
            [.. Enumerable.Range(0, MotherBrainBodyFrameDefinitions.FrameCount).Select(MotherBrainBodyVisualDefinitionsConstants.Frame)];
    }
}

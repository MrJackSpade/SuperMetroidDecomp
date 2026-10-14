using SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="MotherBrainBodyVisualDefinitions"/> members production does not use.</summary>
internal static class MotherBrainBodyVisualDefinitionsAccess
{
    /// <summary>Enumerates all compiled Mother Brain body-frame definitions.</summary>
    extension(MotherBrainBodyVisualDefinitions)
    {
        /// <summary>Materializes the indexed frame definitions in animation order.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, MotherBrainBodyVisualDefinitions.FrameCount).Select(MotherBrainBodyVisualDefinitions.Frame)];
    }
}

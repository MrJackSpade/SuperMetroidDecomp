namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Cartridge values of <see cref="WorkRobotVisualDefinitions"/> that only verification reads.</summary>
internal static class WorkRobotVisualDefinitionsConstants
{
    /// <summary>Number of Work Robot animation images in its extracted visual sheet.</summary>
    public const int FrameCount = 27;

    /// <summary>Provides the expected Work Robot visual frame count for sprite extraction checks.</summary>
    extension(WorkRobotVisualDefinitions)
    {
        /// <inheritdoc cref="WorkRobotVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => WorkRobotVisualDefinitionsConstants.FrameCount;
    }
}

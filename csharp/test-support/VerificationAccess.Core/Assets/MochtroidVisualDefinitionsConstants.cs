using SuperMetroid.Core.Assets;

/// <summary>Cartridge values of <see cref="MochtroidVisualDefinitions"/> that only verification reads.</summary>
internal static class MochtroidVisualDefinitionsConstants
{
    /// <summary>Number of Mochtroid images in the extracted visual sheet.</summary>
    public const int FrameCount = 6;

    /// <summary>Provides the expected Mochtroid visual frame count for sprite import checks.</summary>
    extension(MochtroidVisualDefinitions)
    {
        /// <inheritdoc cref="MochtroidVisualDefinitionsConstants.FrameCount"/>
        internal static int FrameCount => MochtroidVisualDefinitionsConstants.FrameCount;
    }
}

namespace SuperMetroid.Core.Enemies.Powamp.Artwork;

/// <summary>Frame lists and counts of <see cref="PowampSpikeFrameDefinitions"/> that only verification read.</summary>
internal static class PowampSpikeFrameDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="PowampSpikeFrameDefinitions"/>.</summary>
    extension(PowampSpikeFrameDefinitions)
    {
        /// <summary>The poses, in installed artwork order.</summary>
        internal static int PoseCount => 3;
    }
}

using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Dragon.Artwork;

/// <summary>Frame lists and counts of <see cref="DragonFireballFrameDefinitions"/> that only verification read.</summary>
internal static class DragonFireballFrameDefinitionsFramesAccess
{
    /// <summary>The members, read as members of <see cref="DragonFireballFrameDefinitions"/>.</summary>
    extension(DragonFireballFrameDefinitions)
    {
        /// <summary>The poses, in installed artwork order.</summary>
        internal static int PoseCount => 8;
    }
}

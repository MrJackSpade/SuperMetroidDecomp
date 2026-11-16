using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Cartridge values and helpers of <see cref="CrocomireBodyFramesTooling"/> that only verification reads.</summary>
internal static class CrocomireBodyFramesToolingConstants
{
    /// <summary>Whether <paramref name="frame"/> is one of the skeleton's frames.</summary>
    internal static bool IsSkeleton(EnemyFrameId frame) => Array.IndexOf(CrocomireBodyFramesTooling.SkeletonOrder, frame) >= 0;
}

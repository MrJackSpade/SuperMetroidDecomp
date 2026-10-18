using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Development-tool members of <see cref="CrocomireBodyFrameSpritemaps"/>; never linked by player hosts.</summary>
internal static class CrocomireBodyFrameSpritemapsTooling
{
    /// <summary>The installed extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CrocomireBodyFrame frame) => frame.Set switch
    {
        CrocomireBodyFrameSet.Body => CrocomireBodyVisualDefinitions.FramePointer(frame.Index),
        CrocomireBodyFrameSet.Skeleton => CrocomireSkeletonVisualDefinitions.FramePointer(frame.Index),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyFrameSet)} {frame.Set}."),
    };
}

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>The presentation boundary of Crocomire's body frames: the installed extended spritemap each one shows.</summary>
internal static class CrocomireBodyFrameSpritemaps
{
    /// <summary>The installed extended spritemap <paramref name="frame"/> shows.</summary>
    internal static ushort Spritemap(CrocomireBodyFrame frame) => frame.Set switch
    {
        CrocomireBodyFrameSet.Body => CrocomireBodyVisualDefinitions.FramePointer(frame.Index),
        CrocomireBodyFrameSet.Skeleton => CrocomireSkeletonVisualDefinitions.FramePointer(frame.Index),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyFrameSet)} {frame.Set}."),
    };
}

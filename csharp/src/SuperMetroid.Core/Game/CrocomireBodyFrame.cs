using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One frame of Crocomire's body programs: an index into its body or skeleton visual catalog.</summary>
/// <param name="Set">The catalog the frame belongs to.</param>
/// <param name="Index">The frame's position in that catalog.</param>
public readonly record struct CrocomireBodyFrame(CrocomireBodyFrameSet Set, int Index)
{
    /// <summary>The installed extended spritemap the frame shows.</summary>
    internal ushort Spritemap => Set switch
    {
        CrocomireBodyFrameSet.Body => CrocomireBodyVisualDefinitions.FramePointer(Index),
        CrocomireBodyFrameSet.Skeleton => CrocomireSkeletonVisualDefinitions.FramePointer(Index),
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireBodyFrameSet)} {Set}."),
    };
}

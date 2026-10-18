using SuperMetroid.Core.Bosses.Crocomire.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One frame of Crocomire's body programs: an index into its body or skeleton visual catalog.</summary>
/// <param name="Set">The catalog the frame belongs to.</param>
/// <param name="Index">The frame's position in that catalog.</param>
public readonly record struct CrocomireBodyFrame(CrocomireBodyFrameSet Set, int Index)
{
}

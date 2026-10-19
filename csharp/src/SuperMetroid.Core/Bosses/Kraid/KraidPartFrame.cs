using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One frame of Kraid's part programs.</summary>
/// <param name="Kind">The run the frame belongs to.</param>
/// <param name="Index">The frame's position in that run.</param>
public readonly record struct KraidPartFrame(KraidPartFrameKind Kind, int Index)
{
}

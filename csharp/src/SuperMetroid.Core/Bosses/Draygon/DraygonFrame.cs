using SuperMetroid.Core.Bosses.Draygon.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Draygon frame: an index into <see cref="DraygonFrameDefinitions"/>.</summary>
/// <param name="Index">The frame's position in the catalog.</param>
public readonly record struct DraygonFrame(int Index)
{
}

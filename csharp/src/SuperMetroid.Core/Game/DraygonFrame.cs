using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One Draygon frame: an index into <see cref="DraygonFrameDefinitions"/>.</summary>
/// <param name="Index">The frame's position in the catalog.</param>
public readonly record struct DraygonFrame(int Index)
{
    /// <summary>The installed extended spritemap the frame shows.</summary>
    internal ushort Spritemap => DraygonFrameDefinitions.Spritemap(this);
}

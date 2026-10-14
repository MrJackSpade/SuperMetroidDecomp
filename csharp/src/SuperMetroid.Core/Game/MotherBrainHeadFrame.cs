using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>One brain frame, an index into <see cref="MotherBrainVisualDefinitions"/>.</summary>
/// <param name="Index">The frame's position in the Mother Brain composition catalog.</param>
public readonly record struct MotherBrainHeadFrame(int Index)
{
    /// <summary>The installed spritemap the frame shows.</summary>
    internal ushort Spritemap => MotherBrainVisualDefinitions.Frame(Index).Pointer;
}

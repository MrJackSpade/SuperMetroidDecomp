using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>A falling tube's pose, an index into the tube compositions of <see cref="MotherBrainVisualDefinitions"/>.</summary>
/// <param name="Index">The piece whose pose this is.</param>
public readonly record struct MotherBrainTubeFrame(int Index)
{
    /// <summary>The installed spritemap the pose shows.</summary>
    internal ushort Spritemap => MotherBrainVisualDefinitions.TubeFrame(Index).Pointer;
}

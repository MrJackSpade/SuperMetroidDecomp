using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A falling tube's pose, an index into the tube compositions of <see cref="MotherBrainVisualDefinitions"/>.</summary>
/// <param name="Index">The piece whose pose this is.</param>
public readonly record struct MotherBrainTubeFrame(int Index)
{
}

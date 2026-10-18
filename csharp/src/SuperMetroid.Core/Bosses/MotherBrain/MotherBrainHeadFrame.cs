using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One brain frame, an index into <see cref="MotherBrainVisualDefinitions"/>.</summary>
/// <param name="Index">The frame's position in the Mother Brain composition catalog.</param>
public readonly record struct MotherBrainHeadFrame(int Index)
{
}

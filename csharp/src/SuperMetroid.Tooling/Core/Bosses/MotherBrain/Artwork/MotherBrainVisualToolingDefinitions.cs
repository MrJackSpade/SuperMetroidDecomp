using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Development-tool members of <see cref="MotherBrainVisualDefinitions"/>; never linked by player hosts.</summary>
internal static class MotherBrainVisualToolingDefinitions
{
    /// <summary>$A9:ADA1-AE5D: the final five catalog frames are the falling-tube compositions.</summary>
    internal static EnemySpritemapDefinition TubeFrame(int index)
    {
        if ((uint)index >= 5) throw new IndexOutOfRangeException();
        return MotherBrainVisualDefinitions.Frame(MotherBrainVisualDefinitions.FrameCount - 5 + index);
    }
}

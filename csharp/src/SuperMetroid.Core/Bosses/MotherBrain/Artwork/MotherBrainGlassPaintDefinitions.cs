using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The independently chosen cyan glass-face paint; shade interpolation and shared room/recovery dependencies remain separate.</summary>
internal static class MotherBrainGlassPaintDefinitions
{
    /// <summary>
    /// Palette_MotherBrain_GlassShards index four: bright triangular face
    /// in the glass shard frames. The shard initializer selects its graphics, selecting
    /// head-sheet tiles 354/357 and sprite palette three; this is a fixed painted tint.
    /// </summary>
    internal static readonly RgbColor FaceHighlight = new(11, 28, 31);
}

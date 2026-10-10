using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>The independently chosen cyan glass-face paint; shade interpolation and shared room/recovery dependencies remain separate.</summary>
internal static class MotherBrainGlassPaintDefinitions
{
    /// <summary>
    /// $A9:951A, Palette_MotherBrain_GlassShards index four: bright triangular face
    /// in maps $8D:9750/976C. Init $86:CDF0 supplies graphics $0640, selecting
    /// head-sheet tiles $162/$165 and OBJ palette three; this is a fixed painted tint.
    /// </summary>
    internal static readonly Bgr555 FaceHighlight = Bgr555.FromWord(0x7f8b);
}

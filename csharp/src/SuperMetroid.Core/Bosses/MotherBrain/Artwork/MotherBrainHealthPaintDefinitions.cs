using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Six independently chosen Mother Brain body paint anchors shared by calculated palette effects.</summary>
internal static class MotherBrainHealthPaintDefinitions
{
    /// <summary>$AD:E6AC: chosen bright cortex patches on the native head artwork.</summary>
    internal static readonly RgbColor CortexHighlight = RgbColor.FromWord(0x269f);
    /// <summary>$AD:E6AE: chosen orange cortex middle tones.</summary>
    internal static readonly RgbColor CortexMidtone = RgbColor.FromWord(0x0159);
    /// <summary>$AD:E6B0: chosen deep red cortex folds.</summary>
    internal static readonly RgbColor CortexShadow = RgbColor.FromWord(0x004c);
    /// <summary>$AD:E6B2: chosen head/limb silhouette and internal outlines.</summary>
    internal static readonly RgbColor Outline = RgbColor.FromWord(0x0004);
    /// <summary>$AD:E6B4: chosen skull, teeth, spikes and front-limb plate highlight.</summary>
    internal static readonly RgbColor PlateHighlight = RgbColor.FromWord(0x5739);
    /// <summary>$AD:E6BC: chosen lower-face, mouth and neck-joint tissue highlight.</summary>
    internal static readonly RgbColor TissueHighlight = RgbColor.FromWord(0x367f);
}

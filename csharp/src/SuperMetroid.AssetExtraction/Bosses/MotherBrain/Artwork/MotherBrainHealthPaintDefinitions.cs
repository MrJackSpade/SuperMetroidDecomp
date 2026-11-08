using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>Six independently chosen Mother Brain body paint anchors shared by calculated palette effects.</summary>
internal static class MotherBrainHealthPaintDefinitions
{
    /// <summary>Chosen bright cortex patches on the head artwork.</summary>
    internal static readonly RgbColor CortexHighlight = new(255, 165, 74);
    /// <summary>Chosen orange cortex middle tones.</summary>
    internal static readonly RgbColor CortexMidtone = new(206, 82, 0);
    /// <summary>Chosen deep red cortex folds.</summary>
    internal static readonly RgbColor CortexShadow = new(99, 16, 0);
    /// <summary>Chosen head/limb silhouette and internal outlines.</summary>
    internal static readonly RgbColor Outline = new(33, 0, 0);
    /// <summary>Chosen skull, teeth, spikes and front-limb plate highlight.</summary>
    internal static readonly RgbColor PlateHighlight = new(206, 206, 173);
    /// <summary>Chosen lower-face, mouth and neck-joint tissue highlight.</summary>
    internal static readonly RgbColor TissueHighlight = new(255, 156, 107);
}

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>Six independently chosen Mother Brain body paint anchors shared by calculated palette effects.</summary>
internal static class MotherBrainHealthPaintDefinitions
{
    /// <summary>Chosen bright cortex patches on the head artwork.</summary>
    internal static readonly RgbColor CortexHighlight = new(31, 20, 9);
    /// <summary>Chosen orange cortex middle tones.</summary>
    internal static readonly RgbColor CortexMidtone = new(25, 10, 0);
    /// <summary>Chosen deep red cortex folds.</summary>
    internal static readonly RgbColor CortexShadow = new(12, 2, 0);
    /// <summary>Chosen head/limb silhouette and internal outlines.</summary>
    internal static readonly RgbColor Outline = new(4, 0, 0);
    /// <summary>Chosen skull, teeth, spikes and front-limb plate highlight.</summary>
    internal static readonly RgbColor PlateHighlight = new(25, 25, 21);
    /// <summary>Chosen lower-face, mouth and neck-joint tissue highlight.</summary>
    internal static readonly RgbColor TissueHighlight = new(31, 19, 13);
}

using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Six independently chosen Mother Brain body paint anchors shared by calculated palette effects.</summary>
internal static class MotherBrainHealthPaintDefinitions
{
    /// <summary>$AD:E6AC: chosen bright cortex patches on the native head artwork.</summary>
    internal static readonly Bgr555 CortexHighlight = Bgr555.FromWord(0x269f);
    /// <summary>$AD:E6AE: chosen orange cortex middle tones.</summary>
    internal static readonly Bgr555 CortexMidtone = Bgr555.FromWord(0x0159);
    /// <summary>$AD:E6B0: chosen deep red cortex folds.</summary>
    internal static readonly Bgr555 CortexShadow = Bgr555.FromWord(0x004c);
    /// <summary>$AD:E6B2: chosen head/limb silhouette and internal outlines.</summary>
    internal static readonly Bgr555 Outline = Bgr555.FromWord(0x0004);
    /// <summary>$AD:E6B4: chosen skull, teeth, spikes and front-limb plate highlight.</summary>
    internal static readonly Bgr555 PlateHighlight = Bgr555.FromWord(0x5739);
    /// <summary>$AD:E6BC: chosen lower-face, mouth and neck-joint tissue highlight.</summary>
    internal static readonly Bgr555 TissueHighlight = Bgr555.FromWord(0x367f);
}

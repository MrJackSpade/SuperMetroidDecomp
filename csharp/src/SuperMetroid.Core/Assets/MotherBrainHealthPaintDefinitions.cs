namespace SuperMetroid.Core.Assets;

/// <summary>Six independently chosen Mother Brain body paint anchors shared by calculated palette effects.</summary>
internal static class MotherBrainHealthPaintDefinitions
{
    /// <summary>$AD:E6AC: chosen bright cortex patches on the native head artwork.</summary>
    internal const ushort CortexHighlight = 0x269f;
    /// <summary>$AD:E6AE: chosen orange cortex middle tones.</summary>
    internal const ushort CortexMidtone = 0x0159;
    /// <summary>$AD:E6B0: chosen deep red cortex folds.</summary>
    internal const ushort CortexShadow = 0x004c;
    /// <summary>$AD:E6B2: chosen head/limb silhouette and internal outlines.</summary>
    internal const ushort Outline = 0x0004;
    /// <summary>$AD:E6B4: chosen skull, teeth, spikes and front-limb plate highlight.</summary>
    internal const ushort PlateHighlight = 0x5739;
    /// <summary>$AD:E6BC: chosen lower-face, mouth and neck-joint tissue highlight.</summary>
    internal const ushort TissueHighlight = 0x367f;
}

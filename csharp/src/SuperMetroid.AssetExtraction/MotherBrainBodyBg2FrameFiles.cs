using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports BG2 streams only; OAM limbs use the common extended-frame importer.</summary>
internal static class MotherBrainBodyBg2FrameFiles
{
    /// <summary>Serializes the BG2 components from Mother Brain's mixed body-frame streams while leaving OAM limbs to the extended-frame catalog.</summary>
    /// <param name="bus">Import address space containing the native Mother Brain body frame streams.</param>
    /// <returns>The UTF-8 JSON document consumed by the installed Mother Brain body presentation catalog.</returns>
    internal static byte[] Extract(ISnesAddressSpace bus) => EnemyBg2FrameFiles.Extract(
        bus, MotherBrainBodyVisualDefinitions.Bank, MotherBrainBodyVisualDefinitions.Bg2Frames,
        MotherBrainBodyVisualDefinitions.Bg2Version,
        MotherBrainBodyVisualDefinitions.MaximumNativeComponents, "Mother Brain body",
        allowMixedOam: true);
}

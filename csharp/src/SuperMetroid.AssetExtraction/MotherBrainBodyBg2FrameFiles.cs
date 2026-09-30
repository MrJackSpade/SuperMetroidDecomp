using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports BG2 streams only; OAM limbs use the common extended-frame importer.</summary>
internal static class MotherBrainBodyBg2FrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus) => EnemyBg2FrameFiles.Extract(
        bus, MotherBrainBodyVisualDefinitions.Bank, MotherBrainBodyVisualDefinitions.Bg2Frames,
        MotherBrainBodyVisualDefinitions.Bg2Version,
        MotherBrainBodyVisualDefinitions.MaximumNativeComponents, "Mother Brain body",
        allowMixedOam: true);
}

using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Imports BG2 streams only; OAM limbs use the common extended-frame importer.</summary>
internal static class MotherBrainBodyBg2FrameFiles
{
    /// <summary>Imports the body BG2 stream while accepting OAM components interleaved in its native frame records.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus) => EnemyBg2FrameFiles.Extract(
        bus, MotherBrainBodyVisualDefinitions.Bank, MotherBrainBodyVisualDefinitions.Bg2Frames,
        MotherBrainBodyVisualDefinitions.Bg2Version,
        MotherBrainBodyVisualDefinitions.MaximumNativeComponents, "Mother Brain body",
        allowMixedOam: true);
}

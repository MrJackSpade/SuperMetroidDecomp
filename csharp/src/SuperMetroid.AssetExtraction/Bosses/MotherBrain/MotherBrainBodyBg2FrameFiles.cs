using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Imports BG2 streams only; OAM limbs use the common extended-frame importer.</summary>
internal static class MotherBrainBodyBg2FrameFiles
{
    /// <summary>Imports the body BG2 stream while accepting OAM components interleaved in its native frame records.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus) => EnemyBg2FrameFiles.Extract(
        bus, MotherBrainBodyBg2FrameDefinitions.Frames,
        MotherBrainBodyBg2FrameDefinitions.Version,
        MotherBrainBodyBg2FrameDefinitions.MaximumComponents, "Mother Brain body",
        allowMixedOam: true);
}

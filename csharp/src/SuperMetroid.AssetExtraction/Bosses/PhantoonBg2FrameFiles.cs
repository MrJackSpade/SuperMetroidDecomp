using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Phantoon's BG2 visual streams without its physical hitbox lists.</summary>
internal static class PhantoonBg2FrameFiles
{
    /// <summary>Reads the selected Phantoon BG2 streams from their native bank and validates the expected component and version limits.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, PhantoonBg2FrameDefinitions.Bank,
            PhantoonBg2FrameDefinitions.Frames,
            PhantoonBg2FrameDefinitions.Version,
            PhantoonBg2FrameDefinitions.MaximumComponents, "Phantoon");
}

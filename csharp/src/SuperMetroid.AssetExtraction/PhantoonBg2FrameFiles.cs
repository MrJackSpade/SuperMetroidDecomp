using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Phantoon's BG2 visual streams without its physical hitbox lists.</summary>
internal static class PhantoonBg2FrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, PhantoonBg2FrameDefinitions.Bank,
            PhantoonBg2FrameDefinitions.Frames,
            PhantoonBg2FrameDefinitions.Version,
            PhantoonBg2FrameDefinitions.MaximumComponents, "Phantoon");
}

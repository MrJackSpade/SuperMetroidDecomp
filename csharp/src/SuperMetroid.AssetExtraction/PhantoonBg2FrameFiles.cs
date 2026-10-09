using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Phantoon's BG2 visual streams without its physical hitbox lists.</summary>
internal static class PhantoonBg2FrameFiles
{
    /// <summary>Serializes Phantoon's bounded bank-$A7 BG2 frame streams into the shared versioned enemy-BG2 document.</summary>
    /// <param name="bus">Import address space containing the native Phantoon frame streams.</param>
    /// <returns>The UTF-8 JSON document consumed by the installed Phantoon presentation catalog.</returns>
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, PhantoonBg2FrameDefinitions.Bank,
            PhantoonBg2FrameDefinitions.Frames,
            PhantoonBg2FrameDefinitions.Version,
            PhantoonBg2FrameDefinitions.MaximumComponents, "Phantoon");
}

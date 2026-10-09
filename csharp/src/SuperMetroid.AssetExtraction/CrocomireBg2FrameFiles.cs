using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only the BG2 components of Crocomire's mixed fight frames.</summary>
internal static class CrocomireBg2FrameFiles
{
    /// <summary>Serializes the BG2 components from Crocomire's mixed bank-$A4 frame streams while admitting their accompanying OAM entries.</summary>
    /// <param name="bus">Import address space containing the native Crocomire mixed frame streams.</param>
    /// <returns>The UTF-8 JSON document consumed by the installed Crocomire presentation catalog.</returns>
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, CrocomireBg2FrameDefinitions.Bank,
            CrocomireBg2FrameDefinitions.Frames,
            CrocomireBg2FrameDefinitions.Version,
            EnemyBg2FrameLayout.MaximumComponents, "Crocomire",
            allowMixedOam: true);
}

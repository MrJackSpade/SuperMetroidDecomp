using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only the BG2 components of Crocomire's mixed fight frames.</summary>
internal static class CrocomireBg2FrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, CrocomireBg2FrameDefinitions.Bank,
            CrocomireBg2FrameDefinitions.Frames,
            CrocomireBg2FrameDefinitions.Version,
            EnemyBg2FrameLayout.MaximumComponents, "Crocomire",
            allowMixedOam: true);
}

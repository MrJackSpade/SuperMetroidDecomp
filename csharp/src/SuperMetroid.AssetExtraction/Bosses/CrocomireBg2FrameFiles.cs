using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts only the BG2 components of Crocomire's mixed fight frames.</summary>
internal static class CrocomireBg2FrameFiles
{
    /// <summary>Reads Crocomire's mixed bank-$A4 frame set as BG2 tilemap data while allowing its separately authored OAM components.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, CrocomireBg2FrameDefinitions.Bank,
            CrocomireBg2FrameDefinitions.Frames,
            CrocomireBg2FrameDefinitions.Version,
            EnemyBg2FrameLayout.MaximumComponents, "Crocomire",
            allowMixedOam: true);
}

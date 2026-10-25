using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>Extracts Draygon's BG2 appearance, excluding native collision lists.</summary>
internal static class DraygonBg2FrameFiles
{
    /// <summary>Decodes Draygon's defined BG2 tilemap streams into the installed frame JSON bytes.</summary>
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus,
            DraygonBg2FrameDefinitions.Frames,
            DraygonBg2FrameDefinitions.Version,
            DraygonBg2FrameDefinitions.MaximumComponents, "Draygon");
}

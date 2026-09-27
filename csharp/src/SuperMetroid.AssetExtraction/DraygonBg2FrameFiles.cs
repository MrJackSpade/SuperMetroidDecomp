using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Draygon's BG2 appearance, excluding native collision lists.</summary>
internal static class DraygonBg2FrameFiles
{
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, DraygonBg2FrameDefinitions.Bank,
            DraygonBg2FrameDefinitions.Frames,
            DraygonBg2FrameDefinitions.Version,
            DraygonBg2FrameDefinitions.MaximumComponents, "Draygon");
}

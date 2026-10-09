using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts Draygon's BG2 appearance, excluding native collision lists.</summary>
internal static class DraygonBg2FrameFiles
{
    /// <summary>Serializes Draygon's bounded bank-$A5 BG2 frame streams into the shared versioned enemy-BG2 document.</summary>
    /// <param name="bus">Import address space containing the native Draygon frame streams.</param>
    /// <returns>The UTF-8 JSON document consumed by the installed Draygon presentation catalog.</returns>
    internal static byte[] Extract(ISnesAddressSpace bus) =>
        EnemyBg2FrameFiles.Extract(bus, DraygonBg2FrameDefinitions.Bank,
            DraygonBg2FrameDefinitions.Frames,
            DraygonBg2FrameDefinitions.Version,
            DraygonBg2FrameDefinitions.MaximumComponents, "Draygon");
}

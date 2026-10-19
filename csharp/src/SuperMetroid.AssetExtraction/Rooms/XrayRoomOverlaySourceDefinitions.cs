using SuperMetroid.Core.Rooms.Xray;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The bank-$8F pointer the cartridge stores for each room-specific X-ray overlay.</summary>
internal static class XrayRoomOverlaySourceDefinitions
{
    /// <summary>Each overlay, by its bank-$8F pointer.</summary>
    internal static IReadOnlyDictionary<ushort, XrayRoomOverlayDefinition> ByPointer { get; } = new Dictionary<ushort, XrayRoomOverlayDefinition>
    {
        [0x986b] = XrayRoomOverlayDefinitions.BombTorizoAnimalEscapeBlocks,
    };
}

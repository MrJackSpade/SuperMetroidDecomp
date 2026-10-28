using SuperMetroid.Core.Rooms.Plms;
using static SuperMetroid.AssetExtraction.Rooms.RoomPlmDynamicCollectibleGraphicsDefinitions;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Cartridge values and helpers of <see cref="DynamicCollectibleGraphics"/> that only verification reads.</summary>
internal static class DynamicCollectibleGraphicsConstants
{
    /// <summary>Enumerates the complete set of dynamic permanent-item graphics in collectible-kind order.</summary>
    internal static IEnumerable<RoomPlmDynamicCollectibleGraphic> All => Kinds.Select(DynamicCollectibleGraphics.Get);
}

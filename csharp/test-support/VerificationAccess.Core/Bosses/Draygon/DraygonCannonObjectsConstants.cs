using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values and helpers of <see cref="DraygonCannonObjects"/> that only verification reads.</summary>
internal static class DraygonCannonObjectsConstants
{
    /// <summary>Whether <paramref name="kind"/> is one of the room's wall cannons: the live right, destroyed right or live left cannon.</summary>
    internal static bool IsDraygonCannonKind(RoomObjectKind kind) => DraygonCannonDefinitions.Setups.ContainsKey(kind);
}

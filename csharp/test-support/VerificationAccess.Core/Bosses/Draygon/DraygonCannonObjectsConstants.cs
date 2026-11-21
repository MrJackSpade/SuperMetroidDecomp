using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>Cartridge values and helpers of <see cref="DraygonCannonObjects"/> that only verification reads.</summary>
internal static class DraygonCannonObjectsConstants
{
    /// <summary>
    /// The wall each cannon kind faces out from and whether it starts destroyed, which its placements carry (#2400), read
    /// from Core's table of the kind each cannon setup places (#2557): the live right, destroyed right and live left cannon.
    /// No other kind is a cannon.
    /// </summary>
    internal static IReadOnlyDictionary<RoomObjectKind, (DraygonCannonOrientation Orientation, DraygonCannonStart Start)> CannonKinds { get; } =
        RoomObjectKindDefinitions.DraygonCannons.ToDictionary(pair => pair.Value, pair => (pair.Key.Orientation, pair.Key.Start));

    /// <summary>Whether <paramref name="kind"/> is one of the room's wall cannons: the live right, destroyed right or live left cannon.</summary>
    internal static bool IsDraygonCannonKind(RoomObjectKind kind) => CannonKinds.ContainsKey(kind);
}

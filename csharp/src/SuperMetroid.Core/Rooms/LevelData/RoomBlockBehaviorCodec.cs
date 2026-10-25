using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The codec between <see cref="RoomBlockBehavior"/> and the BTS byte a room's behavior data stores. It is the one
/// place a typed behavior becomes that byte, and the one place a BTS byte becomes a blue-door orientation, a station
/// access or a downward-gate trigger; the behavior holds 0 through 255, so the conversion is lossless.
/// </summary>
[EncodingOwner("room block behaviour byte")]
internal static class RoomBlockBehaviorCodec
{
    /// <summary>The four blue-door cap behaviors in BTS order, starting at the left-facing cap.</summary>
    private static readonly ColoredDoorOrientation[] BlueDoorOrientations =
        [ColoredDoorOrientation.Left, ColoredDoorOrientation.Right, ColoredDoorOrientation.Up, ColoredDoorOrientation.Down];

    /// <summary>The BTS byte that stores <paramref name="behavior"/>.</summary>
    internal static byte ToBtsByte(this RoomBlockBehavior behavior) => checked((byte)behavior.Value);

    /// <summary>The blue-door cap behavior facing <paramref name="orientation"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="orientation"/> is not a door orientation.</exception>
    internal static RoomBlockBehavior BlueDoor(ColoredDoorOrientation orientation)
    {
        int index = Array.IndexOf(BlueDoorOrientations, orientation);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door orientation.");
        return new RoomBlockBehavior(RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + index);
    }

    /// <summary>Decodes the four contiguous blue-door cap values.</summary>
    internal static bool TryBlueDoorOrientation(RoomBlockBehavior behavior, out ColoredDoorOrientation orientation)
    {
        int index = behavior.Value - RoomBlockBehaviorValues.BlueDoorFacingLeft.Value;
        if ((uint)index < (uint)BlueDoorOrientations.Length)
        {
            orientation = BlueDoorOrientations[index];
            return true;
        }

        orientation = default;
        return false;
    }

    /// <summary>The station-access behavior <paramref name="access"/> as the BTS byte the station's block stores.</summary>
    internal static RoomBlockBehavior StationAccess(StationAccessBehavior access) => new((byte)access);

    /// <summary>Decodes a map, recharge, or save-station access byte.</summary>
    internal static bool TryStationAccess(RoomBlockBehavior behavior, out StationAccessBehavior access)
    {
        access = (StationAccessBehavior)behavior.Value;
        if (Enum.IsDefined(access))
            return true;
        access = default;
        return false;
    }

    /// <summary>Decodes the eight shootable-block values reserved for downward gate triggers.</summary>
    internal static bool TryDownwardGateTrigger(RoomBlockBehavior behavior, out DownwardGateTriggerBehavior trigger)
    {
        trigger = (DownwardGateTriggerBehavior)behavior.Value;
        if (Enum.IsDefined(trigger))
            return true;
        trigger = default;
        return false;
    }
}

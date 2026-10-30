using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// The one place a block's behaviour parameter becomes a blue-door orientation, a station access or a downward-gate
/// trigger, and the one place those become a parameter.
/// </summary>
[EncodingOwner("room block behaviour parameter")]
internal static class RoomBlockBehaviorCodec
{
    /// <summary>The four blue-door cap behaviors in parameter order, starting at the left-facing cap.</summary>
    private static readonly ColoredDoorOrientation[] BlueDoorOrientations =
        [ColoredDoorOrientation.Left, ColoredDoorOrientation.Right, ColoredDoorOrientation.Up, ColoredDoorOrientation.Down];

    /// <summary>The blue-door cap behavior facing <paramref name="orientation"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="orientation"/> is not a door orientation.</exception>
    internal static RoomBlockBehavior BlueDoor(ColoredDoorOrientation orientation)
    {
        int index = Array.IndexOf(BlueDoorOrientations, orientation);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(orientation), orientation, "Undefined door orientation.");
        return new RoomBlockBehavior(RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + index);
    }

    /// <summary>The behaviour a gate trigger takes from its block number: the number's low eight bits.</summary>
    internal static RoomBlockBehavior GateTrigger(RoomBlockAppearance trigger) => new(trigger.Block & byte.MaxValue);

    /// <summary>An extension's signed offset, -128 through 127, stored as its parameter in two's complement.</summary>
    internal static RoomBlockBehavior ExtensionOffset(int offset) =>
        new(Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, offset) & byte.MaxValue);

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

    /// <summary>The station-access behavior <paramref name="access"/> as the parameter the station's block holds.</summary>
    internal static RoomBlockBehavior StationAccess(StationAccessBehavior access) => new((int)access);

    /// <summary>Decodes a map, recharge, or save-station access behaviour.</summary>
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

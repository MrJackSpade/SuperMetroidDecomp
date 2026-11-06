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

    /// <summary>The seven station accesses in parameter order, starting at the map station's right-side access.</summary>
    private static readonly StationAccessBehavior[] StationAccesses =
    [
        StationAccessBehavior.MapRight, StationAccessBehavior.MapLeft,
        StationAccessBehavior.EnergyRight, StationAccessBehavior.EnergyLeft,
        StationAccessBehavior.MissileRight, StationAccessBehavior.MissileLeft,
        StationAccessBehavior.SaveFloor,
    ];

    /// <summary>The eight downward-gate triggers in parameter order: blue, red, green and yellow, each left then right.</summary>
    private static readonly DownwardGateTriggerBehavior[] DownwardGateTriggers =
    [
        DownwardGateTriggerBehavior.BlueLeft, DownwardGateTriggerBehavior.BlueRight,
        DownwardGateTriggerBehavior.RedLeft, DownwardGateTriggerBehavior.RedRight,
        DownwardGateTriggerBehavior.GreenLeft, DownwardGateTriggerBehavior.GreenRight,
        DownwardGateTriggerBehavior.YellowLeft, DownwardGateTriggerBehavior.YellowRight,
    ];

    /// <summary>The blue-door cap behavior facing <paramref name="orientation"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="orientation"/> is not a door orientation.</exception>
    internal static RoomBlockBehavior BlueDoor(ColoredDoorOrientation orientation) =>
        Encode(RoomBlockBehaviorValues.BlueDoorFacingLeft, BlueDoorOrientations, orientation);

    /// <summary>The behaviour a gate trigger takes from its block number: the number's low eight bits.</summary>
    internal static RoomBlockBehavior GateTrigger(RoomBlockAppearance trigger) => new(trigger.Block & byte.MaxValue);

    /// <summary>An extension's signed offset, -128 through 127, stored as its parameter in two's complement.</summary>
    internal static RoomBlockBehavior ExtensionOffset(int offset) =>
        new(Ensure.BetweenInclusive(sbyte.MinValue, sbyte.MaxValue, offset) & byte.MaxValue);

    /// <summary>The signed offset, -128 through 127, an extension's parameter holds in two's complement.</summary>
    internal static int ReadExtensionOffset(RoomBlockBehavior behavior) => unchecked((sbyte)behavior.Value);

    /// <summary>Decodes the four contiguous blue-door cap values.</summary>
    internal static bool TryBlueDoorOrientation(RoomBlockBehavior behavior, out ColoredDoorOrientation orientation) =>
        TryDecode(behavior, RoomBlockBehaviorValues.BlueDoorFacingLeft, BlueDoorOrientations, out orientation);

    /// <summary>The station-access behavior <paramref name="access"/> as the parameter the station's block holds.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="access"/> is not a station access.</exception>
    internal static RoomBlockBehavior StationAccess(StationAccessBehavior access) =>
        Encode(RoomBlockBehaviorValues.FirstStationAccess, StationAccesses, access);

    /// <summary>Decodes a map, recharge, or save-station access behaviour.</summary>
    internal static bool TryStationAccess(RoomBlockBehavior behavior, out StationAccessBehavior access) =>
        TryDecode(behavior, RoomBlockBehaviorValues.FirstStationAccess, StationAccesses, out access);

    /// <summary>Decodes the eight shootable-block values reserved for downward gate triggers.</summary>
    internal static bool TryDownwardGateTrigger(RoomBlockBehavior behavior, out DownwardGateTriggerBehavior trigger) =>
        TryDecode(behavior, RoomBlockBehaviorValues.FirstDownwardGateTrigger, DownwardGateTriggers, out trigger);

    /// <summary>The parameter of <paramref name="meaning"/> within a contiguous run of meanings starting at <paramref name="first"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="meaning"/> is not in <paramref name="run"/>.</exception>
    private static RoomBlockBehavior Encode<T>(RoomBlockBehavior first, T[] run, T meaning)
        where T : struct, Enum
    {
        int index = Array.IndexOf(run, meaning);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(meaning), meaning, $"Undefined {typeof(T).Name}.");
        return new RoomBlockBehavior(first.Value + index);
    }

    /// <summary>Decodes a parameter within a contiguous run of meanings starting at <paramref name="first"/>.</summary>
    private static bool TryDecode<T>(RoomBlockBehavior behavior, RoomBlockBehavior first, T[] run, out T meaning)
        where T : struct, Enum
    {
        int index = behavior.Value - first.Value;
        if ((uint)index < (uint)run.Length)
        {
            meaning = run[index];
            return true;
        }

        meaning = default;
        return false;
    }
}

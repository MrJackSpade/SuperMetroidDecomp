using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Verification's native view of the block behaviours the PLM identities stand for (#1481): the behaviour parameter
/// naming each member. Core's values are arbitrary; its block-behaviour codec decodes the parameters in production,
/// and checks compare against this independent table.
/// </summary>
internal static class RoomObjectBlockBehaviorNativeWords
{
    /// <summary>The special-solid block behaviour naming each <see cref="StationAccess"/>.</summary>
    internal static readonly NativeWordIdentities<StationAccess> StationAccesses = new(new Dictionary<StationAccess, ushort>
    {
        [new(StationKind.Map, HorizontalDirection.Right)] = 0x47,
        [new(StationKind.Map, HorizontalDirection.Left)] = 0x48,
        [new(StationKind.Energy, HorizontalDirection.Right)] = 0x49,
        [new(StationKind.Energy, HorizontalDirection.Left)] = 0x4a,
        [new(StationKind.Missile, HorizontalDirection.Right)] = 0x4b,
        [new(StationKind.Missile, HorizontalDirection.Left)] = 0x4c,
    });

    /// <summary>The special-solid block behaviour of the save station's floor, a reaction of its own.</summary>
    internal const ushort SaveStationFloor = 0x4d;

    /// <summary>The shootable block behaviour naming each <see cref="DownwardGateTrigger"/>.</summary>
    internal static readonly NativeWordIdentities<DownwardGateTrigger> DownwardGateTriggers = new(new Dictionary<DownwardGateTrigger, ushort>
    {
        [new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Left)] = 0x46,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Blue, HorizontalDirection.Right)] = 0x47,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Red, HorizontalDirection.Left)] = 0x48,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Red, HorizontalDirection.Right)] = 0x49,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Left)] = 0x4a,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Green, HorizontalDirection.Right)] = 0x4b,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Yellow, HorizontalDirection.Left)] = 0x4c,
        [new DownwardGateTrigger(DownwardGateTriggerColor.Yellow, HorizontalDirection.Right)] = 0x4d,
    });
}

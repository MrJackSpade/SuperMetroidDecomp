using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Verification's native view of the block behaviours the PLM identities stand for (#1481): the behaviour parameter
/// naming each member. Core's values are arbitrary; its block-behaviour codec decodes the parameters in production,
/// and checks compare against this independent table.
/// </summary>
internal static class PlmBlockBehaviorNativeWords
{
    /// <summary>The special-solid block behaviour naming each <see cref="StationAccessBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<StationAccessBehavior> StationAccesses = new(new Dictionary<StationAccessBehavior, ushort>
    {
        [StationAccessBehavior.MapRight] = 0x47,
        [StationAccessBehavior.MapLeft] = 0x48,
        [StationAccessBehavior.EnergyRight] = 0x49,
        [StationAccessBehavior.EnergyLeft] = 0x4a,
        [StationAccessBehavior.MissileRight] = 0x4b,
        [StationAccessBehavior.MissileLeft] = 0x4c,
        [StationAccessBehavior.SaveFloor] = 0x4d,
    });

    /// <summary>The shootable block behaviour naming each <see cref="DownwardGateTriggerBehavior"/>.</summary>
    internal static readonly NativeWordIdentities<DownwardGateTriggerBehavior> DownwardGateTriggers = new(new Dictionary<DownwardGateTriggerBehavior, ushort>
    {
        [DownwardGateTriggerBehavior.BlueLeft] = 0x46,
        [DownwardGateTriggerBehavior.BlueRight] = 0x47,
        [DownwardGateTriggerBehavior.RedLeft] = 0x48,
        [DownwardGateTriggerBehavior.RedRight] = 0x49,
        [DownwardGateTriggerBehavior.GreenLeft] = 0x4a,
        [DownwardGateTriggerBehavior.GreenRight] = 0x4b,
        [DownwardGateTriggerBehavior.YellowLeft] = 0x4c,
        [DownwardGateTriggerBehavior.YellowRight] = 0x4d,
    });
}

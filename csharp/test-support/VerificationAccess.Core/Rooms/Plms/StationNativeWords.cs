using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// Verification's native view of the station animations (#1414): the bank-$84 instruction-list pointer of each
/// <see cref="StationAnimationList"/>. Core's values are arbitrary; the retail oracles translate through this table.
/// </summary>
internal static class StationNativeWords
{
    /// <summary>The bank-$84 list pointer of each station animation.</summary>
    internal static readonly NativeWordIdentities<StationAnimationList> AnimationLists = new(new Dictionary<StationAnimationList, ushort>
    {
        [StationAnimationList.MapIdle] = 0xad66,
        [StationAnimationList.MapAcquired] = 0xad76,
        [StationAnimationList.Energy] = 0xadc6,
        [StationAnimationList.Missile] = 0xae50,
        [StationAnimationList.SaveIdle] = RoomPlmInstructionListsConstants.SaveStationIdleDraw,
        [StationAnimationList.SaveFirstFrame] = RoomPlmInstructionListsConstants.SaveStationAnimationFirstFrame,
        [StationAnimationList.SaveSecondFrame] = RoomPlmInstructionListsConstants.SaveStationAnimationSecondFrame,
    });
}

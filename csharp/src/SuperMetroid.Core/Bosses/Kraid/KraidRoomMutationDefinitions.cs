using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The room PLM and program of each of Kraid's room mutations.</summary>
internal static class KraidRoomMutationDefinitions
{
    /// <summary>The room PLM each mutation occupies its slot as.</summary>
    internal static readonly IReadOnlyDictionary<KraidRoomMutation, RoomObjectHeaderId> RoomObjects =
        new Dictionary<KraidRoomMutation, RoomObjectHeaderId>
        {
            [KraidRoomMutation.CrumbleCeilingIntoBackground1] = RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground1,
            [KraidRoomMutation.CrumblePlatformVariant1] = RoomObjectHeaderId.CrumbleKraidPlatformVariant1,
            [KraidRoomMutation.CrumbleCeilingIntoBackground2] = RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground2,
            [KraidRoomMutation.CrumblePlatformVariant2] = RoomObjectHeaderId.CrumbleKraidPlatformVariant2,
            [KraidRoomMutation.CrumbleCeilingIntoBackground3] = RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground3,
            [KraidRoomMutation.ClearCeiling] = RoomObjectHeaderId.ClearKraidCeiling,
            [KraidRoomMutation.ClearSpikes] = RoomObjectHeaderId.ClearKraidSpikes,
            [KraidRoomMutation.CrumbleSpikes] = RoomObjectHeaderId.CrumbleKraidSpikes,
        };

    /// <summary>The room program each mutation runs: a platform variant only sets its block to its background.</summary>
    internal static readonly IReadOnlyDictionary<KraidRoomMutation, KraidRoomProgram> Programs =
        new Dictionary<KraidRoomMutation, KraidRoomProgram>
        {
            [KraidRoomMutation.CrumbleCeilingIntoBackground1] = KraidRoomProgram.CrumbleCeilingIntoBackground1,
            [KraidRoomMutation.CrumblePlatformVariant1] = KraidRoomProgram.SetCeilingToBackground2,
            [KraidRoomMutation.CrumbleCeilingIntoBackground2] = KraidRoomProgram.CrumbleCeilingIntoBackground2,
            [KraidRoomMutation.CrumblePlatformVariant2] = KraidRoomProgram.SetCeilingToBackground3,
            [KraidRoomMutation.CrumbleCeilingIntoBackground3] = KraidRoomProgram.CrumbleCeilingIntoBackground3,
            [KraidRoomMutation.ClearCeiling] = KraidRoomProgram.ClearCeiling,
            [KraidRoomMutation.ClearSpikes] = KraidRoomProgram.ClearSpikes,
            [KraidRoomMutation.CrumbleSpikes] = KraidRoomProgram.CrumbleSpikes,
        };
}

using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Chooses the room objects Kraid's sinking callbacks spawn.</summary>
public static class KraidObjects
{
    /// <summary>Returns the platform mutation paired with a sinking-table callback.</summary>
    public static RoomObjectSpawnRequest<KraidRoomMutation>? ForSinkCallback(KraidSinkCallback callback) =>
        KraidObjectDefinitions.SinkMutations.TryGetValue(callback, out RoomObjectSpawnRequest<KraidRoomMutation>? request)
            ? request
            : throw new InvalidOperationException($"Undefined KraidSinkCallback {callback}.");
}

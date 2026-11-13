using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Chooses the room objects Kraid's sinking steps spawn.</summary>
public static class KraidObjects
{
    /// <summary>Returns the platform mutation paired with a sinking-schedule step.</summary>
    public static RoomObjectSpawnRequest<KraidRoomMutation>? ForSinkStep(KraidSinkStep step) =>
        KraidObjectDefinitions.SinkMutations.TryGetValue(step, out RoomObjectSpawnRequest<KraidRoomMutation>? request)
            ? request
            : throw new InvalidOperationException($"Undefined Kraid sinking step {step}.");
}

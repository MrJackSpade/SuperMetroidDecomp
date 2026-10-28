using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Chooses the PLMs Kraid's sinking callbacks spawn.</summary>
public static class KraidPlms
{
    /// <summary>Returns the platform mutation paired with a sinking-table callback.</summary>
    public static PlmSpawnRequest? ForSinkCallback(KraidSinkCallback callback) =>
        KraidPlmDefinitions.SinkMutations.TryGetValue(callback, out PlmSpawnRequest? request)
            ? request
            : throw new InvalidOperationException($"Undefined KraidSinkCallback {callback}.");
}

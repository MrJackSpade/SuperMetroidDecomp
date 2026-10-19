using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Converts Spore Spawn's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="SporeSpawnHitboxTouch"/> and <see cref="SporeSpawnHitboxShot"/>; this is the one place they become
/// words, and it goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class SporeSpawnHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(SporeSpawnHitboxTouch touch) => touch switch
    {
        SporeSpawnHitboxTouch.Contact => EnemyInteractionCallback.SporeSpawnTouch,
        SporeSpawnHitboxTouch.Inert => EnemyInteractionCallback.NoOp,
        _ => throw new InvalidOperationException($"Undefined {nameof(SporeSpawnHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(SporeSpawnHitboxShot shot) => shot switch
    {
        SporeSpawnHitboxShot.Vulnerable => EnemyInteractionCallback.SporeSpawnShot,
        SporeSpawnHitboxShot.Dud => EnemyInteractionCallback.DudShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(SporeSpawnHitboxShot)} {shot}."),
    };
}

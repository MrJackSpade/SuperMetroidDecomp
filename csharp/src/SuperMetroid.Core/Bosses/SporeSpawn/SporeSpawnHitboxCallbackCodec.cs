using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>
/// Converts Spore Spawn's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="SporeSpawnHitboxTouch"/> and <see cref="SporeSpawnHitboxShot"/>; this is the one place they become
/// words, and it goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class SporeSpawnHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(SporeSpawnHitboxTouch touch) => touch switch
    {
        SporeSpawnHitboxTouch.Contact => EnemyAiCodePointers.BankA5.SporeSpawnTouch,
        SporeSpawnHitboxTouch.Inert => EnemyAiCodePointers.BankA0.NoOp,
        _ => throw new InvalidOperationException($"Undefined {nameof(SporeSpawnHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(SporeSpawnHitboxShot shot) => shot switch
    {
        SporeSpawnHitboxShot.Vulnerable => EnemyAiCodePointers.BankA5.SporeSpawnShot,
        SporeSpawnHitboxShot.Dud => EnemyAiCodePointers.BankA0.DudShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(SporeSpawnHitboxShot)} {shot}."),
    };
}

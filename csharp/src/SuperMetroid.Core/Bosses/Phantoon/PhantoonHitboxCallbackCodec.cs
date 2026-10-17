using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Converts Phantoon's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="PhantoonHitboxTouch"/> and <see cref="PhantoonHitboxShot"/>; this is the one place they become words, and
/// it goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class PhantoonHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(PhantoonHitboxTouch touch) => touch switch
    {
        PhantoonHitboxTouch.Contact => PhantoonCollisionDefinitions.TouchAi,
        PhantoonHitboxTouch.Inert => EnemyAiCodePointers.BankA0.NoOp,
        _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(PhantoonHitboxShot shot) => shot switch
    {
        PhantoonHitboxShot.Vulnerable => PhantoonCollisionDefinitions.ShotAi,
        PhantoonHitboxShot.Inert => EnemyAiCodePointers.BankA0.NoOp,
        _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonHitboxShot)} {shot}."),
    };
}

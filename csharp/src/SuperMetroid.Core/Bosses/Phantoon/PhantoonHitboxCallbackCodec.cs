using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Converts Phantoon's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="PhantoonHitboxTouch"/> and <see cref="PhantoonHitboxShot"/>; this is the one place they become words, and
/// it goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class PhantoonHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(PhantoonHitboxTouch touch) => touch switch
    {
        PhantoonHitboxTouch.Contact => EnemyInteractionCallback.PhantoonTouch,
        PhantoonHitboxTouch.Inert => EnemyInteractionCallback.NoOp,
        _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(PhantoonHitboxShot shot) => shot switch
    {
        PhantoonHitboxShot.Vulnerable => EnemyInteractionCallback.PhantoonShot,
        PhantoonHitboxShot.Inert => EnemyInteractionCallback.NoOp,
        _ => throw new InvalidOperationException($"Undefined {nameof(PhantoonHitboxShot)} {shot}."),
    };
}

using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Converts Kraid's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="KraidHitboxTouch"/> and <see cref="KraidHitboxShot"/>; this is the one place they become words, and it
/// goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class KraidHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(KraidHitboxTouch touch) => touch switch
    {
        KraidHitboxTouch.Arm => EnemyInteractionCallback.KraidArmTouch,
        KraidHitboxTouch.Background => EnemyInteractionCallback.KraidBackgroundTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(KraidHitboxShot shot) => shot switch
    {
        KraidHitboxShot.Arm => EnemyInteractionCallback.KraidArmShot,
        KraidHitboxShot.Inert => EnemyInteractionCallback.KraidNoOpShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidHitboxShot)} {shot}."),
    };
}

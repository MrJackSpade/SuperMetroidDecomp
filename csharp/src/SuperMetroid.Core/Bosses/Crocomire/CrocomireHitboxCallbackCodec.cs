using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Converts Crocomire's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="CrocomireHitboxTouch"/> and <see cref="CrocomireHitboxShot"/>; this is the one place they become words,
/// and it goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class CrocomireHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(CrocomireHitboxTouch touch) => touch switch
    {
        CrocomireHitboxTouch.Common => EnemyInteractionCallback.NormalEnemyTouch,
        CrocomireHitboxTouch.Claw => EnemyInteractionCallback.CrocomireClawTouch,
        CrocomireHitboxTouch.Inert => EnemyInteractionCallback.CrocomireHeaderTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(CrocomireHitboxShot shot) => shot switch
    {
        CrocomireHitboxShot.Common => EnemyInteractionCallback.NormalEnemyShot,
        CrocomireHitboxShot.CountHit => EnemyInteractionCallback.CrocomireNoOpHitboxShot,
        CrocomireHitboxShot.Dust => EnemyInteractionCallback.CrocomireDustHitboxShot,
        CrocomireHitboxShot.Mouth => EnemyInteractionCallback.CrocomireMouthShot,
        CrocomireHitboxShot.AlternateDust => EnemyInteractionCallback.CrocomireAlternateDustHitboxShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireHitboxShot)} {shot}."),
    };
}

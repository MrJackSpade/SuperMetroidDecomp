using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Converts Ridley's hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. Every Ridley body rectangle has Ridley's
/// extended-body touch reaction and Ridley's shot reaction; this is the one place they become words, and it goes when
/// that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class RidleyHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns when Samus touches any Ridley body rectangle.</summary>
    internal static EnemyInteractionCallback TouchCallback => EnemyInteractionCallback.RidleyExtendedTouch;

    /// <summary>The callbacks the shared walker returns when a Samus projectile hits any Ridley body rectangle.</summary>
    internal static EnemyInteractionCallback ShotCallback => EnemyInteractionCallback.RidleyShot;

    /// <summary>The callbacks for a shot probe when <paramref name="shot"/> is set, otherwise for a touch probe.</summary>
    internal static EnemyInteractionCallback ToCallback(bool shot) => shot ? ShotCallback : TouchCallback;
}

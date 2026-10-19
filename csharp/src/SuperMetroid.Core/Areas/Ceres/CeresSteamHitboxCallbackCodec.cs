using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Converts the Ceres steam's hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. Every steam rectangle damages Samus on touch
/// and ignores her projectiles; this is the one place those reactions become words, and it goes when that shared contract
/// returns a typed reaction instead of a callbacks.
/// </summary>
internal static class CeresSteamHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns when Samus touches a steam rectangle.</summary>
    internal static EnemyInteractionCallback TouchCallback => EnemyInteractionCallback.CeresSteamTouch;

    /// <summary>The callbacks the shared walker returns when a Samus projectile hits a steam rectangle.</summary>
    internal static EnemyInteractionCallback ShotCallback => EnemyInteractionCallback.NoOp;

    /// <summary>The callbacks for a shot probe when <paramref name="shot"/> is set, otherwise for a touch probe.</summary>
    internal static EnemyInteractionCallback ToCallback(bool shot) => shot ? ShotCallback : TouchCallback;
}

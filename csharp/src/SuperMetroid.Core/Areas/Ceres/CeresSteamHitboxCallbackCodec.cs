using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Converts the Ceres steam's hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. Every steam rectangle damages Samus on touch
/// and ignores her projectiles; this is the one place those reactions become words, and it goes when that shared contract
/// returns a typed reaction instead of a callback word.
/// </summary>
internal static class CeresSteamHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns when Samus touches a steam rectangle.</summary>
    internal static ushort TouchCallbackWord => EnemyAiCodePointers.BankA6.CeresSteamTouch;

    /// <summary>The callback word the shared walker returns when a Samus projectile hits a steam rectangle.</summary>
    internal static ushort ShotCallbackWord => EnemyAiCodePointers.BankA0.NoOp;

    /// <summary>The callback word for a shot probe when <paramref name="shot"/> is set, otherwise for a touch probe.</summary>
    internal static ushort ToCallbackWord(bool shot) => shot ? ShotCallbackWord : TouchCallbackWord;
}

using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Converts Ridley's hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. Every Ridley body rectangle has Ridley's
/// extended-body touch reaction and Ridley's shot reaction; this is the one place they become words, and it goes when
/// that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class RidleyHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns when Samus touches any Ridley body rectangle.</summary>
    internal static ushort TouchCallbackWord => EnemyAiCodePointers.BankA6.RidleyExtendedTouch;

    /// <summary>The callback word the shared walker returns when a Samus projectile hits any Ridley body rectangle.</summary>
    internal static ushort ShotCallbackWord => EnemyAiCodePointers.BankA6.RidleyShot;

    /// <summary>The callback word for a shot probe when <paramref name="shot"/> is set, otherwise for a touch probe.</summary>
    internal static ushort ToCallbackWord(bool shot) => shot ? ShotCallbackWord : TouchCallbackWord;
}

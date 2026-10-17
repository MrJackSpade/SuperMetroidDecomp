using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Converts Crocomire's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="CrocomireHitboxTouch"/> and <see cref="CrocomireHitboxShot"/>; this is the one place they become words,
/// and it goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class CrocomireHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(CrocomireHitboxTouch touch) => touch switch
    {
        CrocomireHitboxTouch.Common => EnemyAiCodePointers.BankA0.NormalEnemyTouch,
        CrocomireHitboxTouch.Claw => EnemyAiCodePointers.BankA4.ClawTouch,
        CrocomireHitboxTouch.Inert => EnemyAiCodePointers.BankA4.HeaderTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(CrocomireHitboxShot shot) => shot switch
    {
        CrocomireHitboxShot.Common => EnemyAiCodePointers.BankA0.NormalEnemyShot,
        CrocomireHitboxShot.CountHit => EnemyAiCodePointers.BankA4.NoOpHitboxShot,
        CrocomireHitboxShot.Dust => EnemyAiCodePointers.BankA4.DustHitboxShot,
        CrocomireHitboxShot.Mouth => EnemyAiCodePointers.BankA4.MouthShot,
        CrocomireHitboxShot.AlternateDust => EnemyAiCodePointers.BankA4.AlternateDustHitboxShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(CrocomireHitboxShot)} {shot}."),
    };
}

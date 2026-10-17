using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Converts Kraid's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="KraidHitboxTouch"/> and <see cref="KraidHitboxShot"/>; this is the one place they become words, and it
/// goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class KraidHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(KraidHitboxTouch touch) => touch switch
    {
        KraidHitboxTouch.Arm => EnemyAiCodePointers.BankA7.KraidArmTouch,
        KraidHitboxTouch.Background => EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(KraidHitboxShot shot) => shot switch
    {
        KraidHitboxShot.Arm => EnemyAiCodePointers.BankA7.KraidArmShot,
        KraidHitboxShot.Inert => EnemyAiCodePointers.BankA7.KraidNoOpShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidHitboxShot)} {shot}."),
    };
}

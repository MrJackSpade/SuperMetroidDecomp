using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Converts Torizo's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="TorizoHitboxTouch"/> and <see cref="TorizoHitboxShot"/>; this is the one place they become words, and it
/// goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class TorizoHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(TorizoHitboxTouch touch) => touch switch
    {
        TorizoHitboxTouch.Body => EnemyAiCodePointers.BankAA.BombTorizoTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(TorizoHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(TorizoHitboxShot shot) => shot switch
    {
        TorizoHitboxShot.Body => EnemyAiCodePointers.BankAA.BombTorizoShot,
        TorizoHitboxShot.StandUpSitDown => EnemyAiCodePointers.BankAA.TorizoStandUpSitDownShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(TorizoHitboxShot)} {shot}."),
    };
}

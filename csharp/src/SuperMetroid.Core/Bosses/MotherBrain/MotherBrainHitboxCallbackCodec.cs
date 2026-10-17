using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Converts Mother Brain's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="MotherBrainHitboxTouch"/> and <see cref="MotherBrainHitboxShot"/>; this is the one place they become
/// words, and it goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class MotherBrainHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(MotherBrainHitboxTouch touch) => touch switch
    {
        MotherBrainHitboxTouch.Head => EnemyAiCodePointers.BankA9.MotherBrainHeadTouch,
        MotherBrainHitboxTouch.Body => EnemyAiCodePointers.BankA9.MotherBrainBodyTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(MotherBrainHitboxShot shot) => shot switch
    {
        MotherBrainHitboxShot.Head => EnemyAiCodePointers.BankA9.MotherBrainHeadShot,
        MotherBrainHitboxShot.Body => EnemyAiCodePointers.BankA9.MotherBrainBodyShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainHitboxShot)} {shot}."),
    };
}

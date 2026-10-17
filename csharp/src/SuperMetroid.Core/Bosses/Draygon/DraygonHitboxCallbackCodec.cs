using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Converts Draygon's typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="DraygonHitboxTouch"/> and <see cref="DraygonHitboxShot"/>; this is the one place they become words, and
/// it goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class DraygonHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(DraygonHitboxTouch touch) => touch switch
    {
        DraygonHitboxTouch.Harmless => EnemyAiCodePointers.BankA0.NoOp,
        DraygonHitboxTouch.Body => EnemyAiCodePointers.BankA5.DraygonTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(DraygonHitboxShot shot) => shot switch
    {
        DraygonHitboxShot.Eye => EnemyAiCodePointers.BankA5.DraygonShot,
        DraygonHitboxShot.Dud => EnemyAiCodePointers.BankA0.DudShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonHitboxShot)} {shot}."),
    };
}

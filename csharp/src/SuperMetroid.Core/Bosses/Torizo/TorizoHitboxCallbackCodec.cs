using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// Converts Torizo's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="TorizoHitboxTouch"/> and <see cref="TorizoHitboxShot"/>; this is the one place they become words, and it
/// goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class TorizoHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(TorizoHitboxTouch touch) => touch switch
    {
        TorizoHitboxTouch.Body => EnemyInteractionCallback.BombTorizoTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(TorizoHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(TorizoHitboxShot shot) => shot switch
    {
        TorizoHitboxShot.Body => EnemyInteractionCallback.BombTorizoShot,
        TorizoHitboxShot.StandUpSitDown => EnemyInteractionCallback.TorizoStandUpSitDownShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(TorizoHitboxShot)} {shot}."),
    };
}

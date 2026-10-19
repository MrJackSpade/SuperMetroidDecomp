using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Converts Draygon's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="DraygonHitboxTouch"/> and <see cref="DraygonHitboxShot"/>; this is the one place they become words, and
/// it goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class DraygonHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(DraygonHitboxTouch touch) => touch switch
    {
        DraygonHitboxTouch.Harmless => EnemyInteractionCallback.NoOp,
        DraygonHitboxTouch.Body => EnemyInteractionCallback.DraygonTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(DraygonHitboxShot shot) => shot switch
    {
        DraygonHitboxShot.Eye => EnemyInteractionCallback.DraygonShot,
        DraygonHitboxShot.Dud => EnemyInteractionCallback.DudShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonHitboxShot)} {shot}."),
    };
}

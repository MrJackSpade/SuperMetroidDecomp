using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Converts Mother Brain's typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="MotherBrainHitboxTouch"/> and <see cref="MotherBrainHitboxShot"/>; this is the one place they become
/// words, and it goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class MotherBrainHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(MotherBrainHitboxTouch touch) => touch switch
    {
        MotherBrainHitboxTouch.Head => EnemyInteractionCallback.MotherBrainHeadTouch,
        MotherBrainHitboxTouch.Body => EnemyInteractionCallback.MotherBrainBodyTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(MotherBrainHitboxShot shot) => shot switch
    {
        MotherBrainHitboxShot.Head => EnemyInteractionCallback.MotherBrainHeadShot,
        MotherBrainHitboxShot.Body => EnemyInteractionCallback.MotherBrainBodyShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(MotherBrainHitboxShot)} {shot}."),
    };
}

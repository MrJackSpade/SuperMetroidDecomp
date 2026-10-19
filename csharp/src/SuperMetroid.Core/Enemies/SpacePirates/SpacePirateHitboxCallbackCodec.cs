namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Converts the ordinary Pirates' typed hitbox reactions into the callbacks the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="SpacePirateHitboxTouch"/> and <see cref="SpacePirateHitboxShot"/>; this is the one place they become
/// words, and it goes when that shared contract returns a typed reaction instead of a callbacks.
/// </summary>
internal static class SpacePirateHitboxCallbackCodec
{
    /// <summary>The callbacks the shared walker returns for a touch reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(SpacePirateHitboxTouch touch) => touch switch
    {
        SpacePirateHitboxTouch.Ordinary => EnemyInteractionCallback.NormalEnemyTouch,
        SpacePirateHitboxTouch.Pirate => EnemyInteractionCallback.SpacePirateTouch,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpacePirateHitboxTouch)} {touch}."),
    };

    /// <summary>The callbacks the shared walker returns for a shot reaction.</summary>
    internal static EnemyInteractionCallback ToCallback(SpacePirateHitboxShot shot) => shot switch
    {
        SpacePirateHitboxShot.Ordinary => EnemyInteractionCallback.NormalEnemyShot,
        SpacePirateHitboxShot.Pirate => EnemyInteractionCallback.SpacePirateShot,
        SpacePirateHitboxShot.GoldNinjaVulnerable => EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot,
        SpacePirateHitboxShot.GoldNinjaInvincible => EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpacePirateHitboxShot)} {shot}."),
    };
}

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Converts the ordinary Pirates' typed hitbox reactions into the callback words the shared extended-hitbox walker
/// (<see cref="IEnemyExtendedHitboxes.FindHitboxCallback"/>) still returns. The rectangles hold
/// <see cref="SpacePirateHitboxTouch"/> and <see cref="SpacePirateHitboxShot"/>; this is the one place they become
/// words, and it goes when that shared contract returns a typed reaction instead of a callback word.
/// </summary>
internal static class SpacePirateHitboxCallbackCodec
{
    /// <summary>The callback word the shared walker returns for a touch reaction.</summary>
    internal static ushort ToCallbackWord(SpacePirateHitboxTouch touch) => touch switch
    {
        SpacePirateHitboxTouch.Ordinary => EnemyAiCodePointers.BankA0.NormalEnemyTouch,
        SpacePirateHitboxTouch.Pirate => EnemyAiCodePointers.BankB2.Touch,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpacePirateHitboxTouch)} {touch}."),
    };

    /// <summary>The callback word the shared walker returns for a shot reaction.</summary>
    internal static ushort ToCallbackWord(SpacePirateHitboxShot shot) => shot switch
    {
        SpacePirateHitboxShot.Ordinary => EnemyAiCodePointers.BankA0.NormalEnemyShot,
        SpacePirateHitboxShot.Pirate => EnemyAiCodePointers.BankB2.Shot,
        SpacePirateHitboxShot.GoldNinjaVulnerable => EnemyAiCodePointers.BankB2.GoldNinjaVulnerableHitboxShot,
        SpacePirateHitboxShot.GoldNinjaInvincible => EnemyAiCodePointers.BankB2.GoldNinjaInvincibleHitboxShot,
        _ => throw new InvalidOperationException($"Undefined {nameof(SpacePirateHitboxShot)} {shot}."),
    };
}

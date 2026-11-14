namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One beam frame-behaviour choice and the projectile frame behaviour it runs; none where the choice runs whatever
/// the game's live state or an unconnected region holds, which is outside the emulation scope limit (#1500).
/// </summary>
/// <param name="Choice">The frame behaviour firing selects.</param>
/// <param name="FrameBehavior">The projectile frame behaviour the choice runs, or none outside the emulation scope.</param>
internal readonly record struct SamusBeamFrameBehaviorDefinition(
    SamusBeamFrameBehaviorChoice Choice,
    SamusProjectileFrameBehavior? FrameBehavior);

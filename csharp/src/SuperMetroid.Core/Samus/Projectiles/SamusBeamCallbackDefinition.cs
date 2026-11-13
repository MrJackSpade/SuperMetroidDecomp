namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One beam callback and its projectile frame behaviour, when supported.</summary>
internal readonly record struct SamusBeamCallbackDefinition(
    SamusBeamCallback Callback,
    SamusProjectileFrameBehavior? Translated);

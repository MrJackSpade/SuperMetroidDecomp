namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One beam frame-behaviour choice and its projectile frame behaviour, when supported.</summary>
internal readonly record struct SamusBeamCallbackDefinition(
    SamusBeamCallback Callback,
    SamusProjectileFrameBehavior? Translated);

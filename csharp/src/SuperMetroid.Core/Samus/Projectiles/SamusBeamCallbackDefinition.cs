namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One beam callback and its translated projectile pre-instruction, when supported.</summary>
internal readonly record struct SamusBeamCallbackDefinition(
    SamusBeamCallback Callback,
    SamusProjectilePreInstruction? Translated);

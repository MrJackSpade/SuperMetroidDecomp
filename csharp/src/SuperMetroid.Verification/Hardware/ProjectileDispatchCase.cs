using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Hardware;

/// <summary>The raw and family of one projectile dispatch case.</summary>
/// <param name="Raw">The raw.</param>
/// <param name="Family">The family.</param>
internal readonly record struct ProjectileDispatchCase(ushort Raw, SamusProjectileFamily Family);

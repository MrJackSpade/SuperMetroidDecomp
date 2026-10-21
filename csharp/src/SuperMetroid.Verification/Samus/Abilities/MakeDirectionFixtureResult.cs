using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The system and samus of one make direction fixture result.</summary>
/// <param name="System">The system.</param>
/// <param name="Samus">The samus.</param>
internal readonly record struct MakeDirectionFixtureResult(SamusBombProjectileSystem System, SamusState Samus);

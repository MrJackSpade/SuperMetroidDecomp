using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Verification.Enemies;

namespace SuperMetroid.Verification.Bosses.Torizo;

/// <summary>The word and cue of one torizo projectile animations word.</summary>
/// <param name="Word">The word.</param>
/// <param name="Cue">The cue.</param>
internal readonly record struct TorizoProjectileAnimationsWord(TorizoProjectileAnimationsVerification.ProjectileWord Word, NativeCueShape<TorizoProjectileCue>? Cue);

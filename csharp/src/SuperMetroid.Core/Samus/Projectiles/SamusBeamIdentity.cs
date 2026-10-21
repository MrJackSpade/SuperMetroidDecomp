using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Whether a beam is charged, and its combination.</summary>
/// <param name="Charged">The charged.</param>
/// <param name="Beam">The beam.</param>
internal readonly record struct SamusBeamIdentity(bool Charged, SamusBeamCombination Beam);

using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Whether a beam is charged, and its combination.</summary>
/// <param name="Charged">Whether the beam is charged.</param>
/// <param name="Beam">The beams combined.</param>
internal readonly record struct SamusBeamIdentity(bool Charged, SamusBeamCombination Beam);

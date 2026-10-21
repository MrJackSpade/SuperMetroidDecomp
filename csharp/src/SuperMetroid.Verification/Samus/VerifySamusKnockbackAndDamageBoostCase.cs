using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Samus;

/// <summary>The pose, hit side and expected direction of one verify samus knockback and damage boost case.</summary>
/// <param name="Pose">The pose.</param>
/// <param name="HitSide">The hit side.</param>
/// <param name="ExpectedDirection">The expected direction.</param>
internal readonly record struct VerifySamusKnockbackAndDamageBoostCase(SamusPoseId Pose, ushort HitSide, ushort ExpectedDirection);

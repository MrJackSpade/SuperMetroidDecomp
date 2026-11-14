using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>What chooses a projectile's trail family: the group its kind picks and its beam combination.</summary>
/// <param name="Group">The group of trail families.</param>
/// <param name="Beams">The beam combination that chooses within the group.</param>
internal readonly record struct ProjectileTrailChoice(ProjectileTrailFamilyGroup Group, SamusBeamCombination Beams);

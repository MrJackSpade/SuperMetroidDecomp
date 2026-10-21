using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>A trail selector row: the table a projectile's kind picks and the row its beam combination picks.</summary>
/// <param name="Group">The selector table.</param>
/// <param name="Beams">The beam combination whose row is read.</param>
internal readonly record struct ProjectileTrailSelectorRow(ProjectileTrailFamilyGroup Group, SamusBeamCombination Beams);

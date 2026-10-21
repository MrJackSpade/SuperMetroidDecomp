using SuperMetroid.Core.Bosses.MotherBrain;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>The direction, turret pose, bullet pose and selector entry of one turret entry.</summary>
/// <param name="Direction">The direction.</param>
/// <param name="TurretPose">The turret pose.</param>
/// <param name="BulletPose">The bullet pose.</param>
/// <param name="SelectorEntry">The selector entry.</param>
internal readonly record struct TurretEntry(MotherBrainTurretDirection Direction, ushort TurretPose, ushort BulletPose, int SelectorEntry);

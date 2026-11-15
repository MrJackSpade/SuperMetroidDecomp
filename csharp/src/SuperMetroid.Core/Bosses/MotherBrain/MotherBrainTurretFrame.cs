namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One turret frame: a turret's or bullet's pose facing one of the eight directions, or one phase of a bullet's impact
/// smoke.
/// </summary>
/// <param name="Animation">The animation the frame belongs to.</param>
/// <param name="Direction">
/// The direction a turret or bullet pose faces; the impact smoke faces none and holds
/// <see cref="MotherBrainTurretDirection.Left"/>, as <see cref="MotherBrainTurretPose.BulletImpact"/> does.
/// </param>
/// <param name="SmokePhase">The phase of the impact smoke; a pose has none and holds zero.</param>
internal readonly record struct MotherBrainTurretFrame(MotherBrainTurretSegment Animation, MotherBrainTurretDirection Direction, int SmokePhase);

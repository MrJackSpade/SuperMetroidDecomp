namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One turret program: a turret's or bullet's still pose facing a direction, or a bullet's impact smoke.</summary>
/// <param name="Program">The turret, bullet or impact program.</param>
/// <param name="Direction">The pose's direction; the impact smoke has none and uses <see cref="MotherBrainTurretDirection.Left"/>.</param>
internal readonly record struct MotherBrainTurretPose(MotherBrainTurretProgram Program, MotherBrainTurretDirection Direction)
{
    /// <summary>The impact smoke's program.</summary>
    internal static MotherBrainTurretPose BulletImpact => new(MotherBrainTurretProgram.BulletImpact, MotherBrainTurretDirection.Left);
}

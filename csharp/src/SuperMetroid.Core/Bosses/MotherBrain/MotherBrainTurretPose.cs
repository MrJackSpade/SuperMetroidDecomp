namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// One turret animation: a turret's or a bullet's still pose facing one of the eight directions, or a bullet's impact smoke,
/// which faces none (#2249).
/// </summary>
internal readonly record struct MotherBrainTurretPose
{
    /// <summary>The pose of <paramref name="animation"/> facing <paramref name="direction"/>; the impact smoke faces none.</summary>
    private MotherBrainTurretPose(MotherBrainTurretSegment animation, MotherBrainTurretDirection? direction)
    {
        Animation = animation;
        Direction = direction;
    }

    /// <summary>The turret, bullet or impact animation.</summary>
    internal MotherBrainTurretSegment Animation { get; }

    /// <summary>The direction a turret's or a bullet's pose faces; null for the impact smoke, which faces none.</summary>
    internal MotherBrainTurretDirection? Direction { get; }

    /// <summary>A turret's pose pointing in <paramref name="direction"/>.</summary>
    internal static MotherBrainTurretPose Turret(MotherBrainTurretDirection direction) => new(MotherBrainTurretSegment.TurretPose, direction);

    /// <summary>A bullet's pose flying in <paramref name="direction"/>.</summary>
    internal static MotherBrainTurretPose Bullet(MotherBrainTurretDirection direction) => new(MotherBrainTurretSegment.BulletPose, direction);

    /// <summary>The impact smoke's animation.</summary>
    internal static MotherBrainTurretPose BulletImpact { get; } = new(MotherBrainTurretSegment.BulletImpact, null);
}

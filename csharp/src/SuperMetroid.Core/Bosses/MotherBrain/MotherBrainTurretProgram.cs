namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The programs of Mother Brain's room turrets and their bullets.</summary>
public enum MotherBrainTurretProgram
{
    /// <summary>A turret's pose for one of the eight directions ($86:C101, six bytes a direction).</summary>
    TurretPose,
    /// <summary>A bullet's pose for one of the eight directions ($86:C143, six bytes a direction).</summary>
    BulletPose,
    /// <summary>The smoke of a bullet that touches Samus or is shot ($86:C19A).</summary>
    BulletImpact,
}

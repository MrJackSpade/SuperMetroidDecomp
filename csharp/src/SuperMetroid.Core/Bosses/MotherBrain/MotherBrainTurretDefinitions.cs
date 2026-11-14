using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled placement, rotation and bullet-physics definitions.</summary>
internal static class MotherBrainTurretDefinitions
{
    /// <summary>
    /// Graphics installed by both projectile initializers: the shared sheet in the Rinkas' colours, the standard colours
    /// of the sheet the import composes under Mother Brain's room's set, which loads them beside her (#1464, #1512). The
    /// turrets and their bullets exist only in the first phase, before the second phase's attack colours load.
    /// </summary>
    internal static SpriteGraphics Graphics { get; } = SpriteGraphics.SharedIn(PictureShading.Standard);

    /// <summary>Minimum random rotation delay the turret's rotation applies.</summary>
    internal const int MinimumRotationDelay = 32;

    /// <summary>Minimum random firing cooldown the turret's firing applies.</summary>
    internal const int MinimumFiringCooldown = 128;

    /// <summary>Turret initialization places four room bays of three turrets.</summary>
    internal const int TurretCount = 12;
    /// <summary>Turret initialization selects eight turret poses in left-to-up-left octant order; it starts the first bay at X=920 and each later bay is 192 pixels left.</summary>
    internal const int FirstBayX = 920;
    /// <summary>The placement rows repeat three mounting positions every twelve blocks.</summary>
    internal const int BaySpacing = 192;
    /// <summary>The bullet uses a 2.75-pixel speed, in 1/256 pixels, rounded in diagonal octants.</summary>
    internal const int BulletSpeed = 704;
    /// <summary>The muzzle samples a seventeen-pixel horizontal radius.</summary>
    internal const int MuzzleRadius = 17;

    /// <summary>
    /// The eight turret directions around the compass, from left through down to up-left; a direction's position on
    /// this ring is the octant the turret's rotation steps through and the launch angle's multiple of 45 degrees.
    /// </summary>
    internal static readonly IdentitySequence<MotherBrainTurretDirection> Ring = new(
        MotherBrainTurretDirection.Left, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.Down,
        MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.Right, MotherBrainTurretDirection.UpRight,
        MotherBrainTurretDirection.Up, MotherBrainTurretDirection.UpLeft);

    /// <summary>The muzzle's vertical offset for each of the eight directions.</summary>
    internal static readonly Dictionary<MotherBrainTurretDirection, int> MuzzleYs = new()
    {
        [MotherBrainTurretDirection.Left] = -9,
        [MotherBrainTurretDirection.Right] = -9,
        [MotherBrainTurretDirection.DownLeft] = 3,
        [MotherBrainTurretDirection.DownRight] = 3,
        [MotherBrainTurretDirection.Down] = 7,
        [MotherBrainTurretDirection.UpLeft] = -19,
        [MotherBrainTurretDirection.UpRight] = -19,
        [MotherBrainTurretDirection.Up] = -21,
    };
}

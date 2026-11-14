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

    /// <summary>
    /// The twelve wall turrets in the order her body's initializer spawns them: four bays of three, from the rightmost bay
    /// leftwards, each bay's ceiling turret first and then its two wall turrets. Each turret has its place, the direction it
    /// starts facing and the arc, in ring order, it may turn through.
    /// </summary>
    internal static IReadOnlyList<MotherBrainTurretDefinition> Turrets { get; } =
    [
        new(new(920, 48), MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.DownRight),
        new(new(840, 64), MotherBrainTurretDirection.Right, MotherBrainTurretDirection.Down, MotherBrainTurretDirection.Right),
        new(new(808, 64), MotherBrainTurretDirection.Down, MotherBrainTurretDirection.Left, MotherBrainTurretDirection.Down),
        new(new(728, 48), MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.DownRight),
        new(new(648, 64), MotherBrainTurretDirection.Right, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.Right),
        new(new(616, 64), MotherBrainTurretDirection.Down, MotherBrainTurretDirection.Left, MotherBrainTurretDirection.DownRight),
        new(new(536, 48), MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.DownRight),
        new(new(456, 64), MotherBrainTurretDirection.Right, MotherBrainTurretDirection.Down, MotherBrainTurretDirection.Right),
        new(new(424, 64), MotherBrainTurretDirection.Down, MotherBrainTurretDirection.Left, MotherBrainTurretDirection.Down),
        new(new(344, 48), MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.DownRight),
        new(new(264, 64), MotherBrainTurretDirection.Right, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.Right),
        new(new(232, 64), MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.Left, MotherBrainTurretDirection.DownRight),
    ];
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

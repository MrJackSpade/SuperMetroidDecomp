using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The turrets' placement, rotation and bullet-physics definitions.</summary>
internal static class MotherBrainTurretDefinitions
{
    /// <summary>
    /// Graphics installed by both projectile setups: the shared sheet in the Rinkas' colours, the standard colours
    /// of the sheet the import composes under Mother Brain's room's set, which loads them beside her (#1464, #1512). The
    /// turrets and their bullets exist only in the first phase, before the second phase's attack colours load.
    /// </summary>
    internal static SpriteGraphics Graphics { get; } = SpriteGraphics.SharedIn(PictureShading.Standard);

    /// <summary>
    /// How far right of and below a turret's position its on-screen test measures: a turret shows while that point lies
    /// within <see cref="ViewWidth"/> of the camera's left edge and within the enemies' view below its top.
    /// </summary>
    internal static readonly Offset16<RoomPixels> ViewOffset = new(4, 16);

    /// <summary>The width, from the camera's left edge, a turret's on-screen test admits.</summary>
    internal const int ViewWidth = 264;

    /// <summary>The fewest updates a turret's random wait before it turns lasts.</summary>
    internal const int MinimumRotationDelay = 32;

    /// <summary>The fewest updates a turret's random wait before it fires lasts.</summary>
    internal const int MinimumFiringCooldown = 128;

    /// <summary>
    /// The twelve wall turrets in the order her body's setup spawns them: four bays of three, from the rightmost bay
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
    /// <summary>A bullet's speed along an axis it is fired straight along: two and three quarter pixels an update.</summary>
    private static readonly SignedEightEight StraightBulletSpeed = SignedEightEight.FromPixelFraction(11, 4);

    /// <summary>
    /// A diagonal bullet's speed along each axis: just under two pixels an update, the straight speed's share of each axis
    /// on a diagonal.
    /// </summary>
    private static readonly SignedEightEight DiagonalBulletSpeed = SignedEightEight.FromPixelFraction(249, 128);

    /// <summary>
    /// The eight turret directions around the compass, counterclockwise from left through down to up-left: the order a
    /// turret turns through and the order each turret's arc is given in.
    /// </summary>
    internal static readonly IdentitySequence<MotherBrainTurretDirection> Ring = new(
        MotherBrainTurretDirection.Left, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.Down,
        MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.Right, MotherBrainTurretDirection.UpRight,
        MotherBrainTurretDirection.Up, MotherBrainTurretDirection.UpLeft);

    /// <summary>
    /// The bullet each of the eight directions fires (#1950): where it leaves the turret's muzzle and its velocity. A
    /// straight shot moves at <see cref="StraightBulletSpeed"/> along its axis and a diagonal one at
    /// <see cref="DiagonalBulletSpeed"/> along each.
    /// </summary>
    internal static readonly Dictionary<MotherBrainTurretDirection, MotherBrainTurretDirectionDefinition> Launches = new()
    {
        [MotherBrainTurretDirection.Left] = new(new(-17, -9), new(-StraightBulletSpeed, SignedEightEight.Zero)),
        [MotherBrainTurretDirection.DownLeft] = new(new(-12, 3), new(-DiagonalBulletSpeed, DiagonalBulletSpeed)),
        [MotherBrainTurretDirection.Down] = new(new(0, 7), new(SignedEightEight.Zero, StraightBulletSpeed)),
        [MotherBrainTurretDirection.DownRight] = new(new(12, 3), new(DiagonalBulletSpeed, DiagonalBulletSpeed)),
        [MotherBrainTurretDirection.Right] = new(new(17, -9), new(StraightBulletSpeed, SignedEightEight.Zero)),
        [MotherBrainTurretDirection.UpRight] = new(new(12, -19), new(DiagonalBulletSpeed, -DiagonalBulletSpeed)),
        [MotherBrainTurretDirection.Up] = new(new(0, -21), new(SignedEightEight.Zero, -StraightBulletSpeed)),
        [MotherBrainTurretDirection.UpLeft] = new(new(-12, -19), new(-DiagonalBulletSpeed, -DiagonalBulletSpeed)),
    };
}

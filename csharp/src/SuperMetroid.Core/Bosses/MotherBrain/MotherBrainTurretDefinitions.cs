using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled placement, rotation and bullet-physics definitions.</summary>
internal static class MotherBrainTurretDefinitions
{
    /// <summary>
    /// Graphics installed by both projectile initializers at $86:BE58/$86:BF5F: character zero in the third sprite
    /// colour set, whose owner in both Mother Brain phases is not yet named (#1464).
    /// </summary>
    internal static SpriteGraphics Graphics { get; } = SpriteGraphics.CharacterZeroIn(new UnnamedSpriteRowColouring(2));

    /// <summary>Minimum random rotation delay applied by $86:BFDF.</summary>
    internal const ushort MinimumRotationDelay = 0x0020;

    /// <summary>Minimum random firing cooldown applied by $86:C00A.</summary>
    internal const ushort MinimumFiringCooldown = 0x0080;

    /// <summary>$86:BE89-$BEF8 places four room bays of three turrets.</summary>
    internal const int TurretCount = 12;
    /// <summary>$86:BEB9/$C040 select eight turret poses in left-to-up-left octant order.</summary>
    /// <summary>$86:BE89 starts the first bay at X=$398; each later bay is 192 pixels left.</summary>
    internal const int FirstBayX = 0x398;
    /// <summary>The native placement rows repeat three mounting positions every twelve tiles.</summary>
    internal const int BaySpacing = 192;
    /// <summary>$86:BFBF/$BFCF use a 2.75-pixel 8.8 bullet speed, rounded in diagonal octants.</summary>
    internal const int BulletSpeed = 0x2c0;
    /// <summary>$86:BF9F samples a seventeen-pixel horizontal muzzle radius.</summary>
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

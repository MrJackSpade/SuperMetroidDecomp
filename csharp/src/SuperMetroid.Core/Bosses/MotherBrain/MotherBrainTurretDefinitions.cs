using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Compiled placement, rotation and bullet-physics definitions.</summary>
internal static class MotherBrainTurretDefinitions
{
    /// <summary>Graphics installed by both projectile initializers at $86:BE58/$86:BF5F: character zero in palette row two.</summary>
    internal static SpriteGraphics Graphics { get; } = SpriteGraphics.InPalette(2);

    /// <summary>Minimum random rotation delay applied by $86:BFDF.</summary>
    internal const ushort MinimumRotationDelay = 0x0020;

    /// <summary>Minimum random firing cooldown applied by $86:C00A.</summary>
    internal const ushort MinimumFiringCooldown = 0x0080;

    /// <summary>$86:BE89-$BEF8 places four room bays of three turrets.</summary>
    internal const int TurretCount = 12;
    /// <summary>$86:BEB9/$C040 select eight turret poses in left-to-up-left octant order.</summary>
    /// <summary>$86:BE89 starts the first bay at X=$398; each later bay is 192 pixels left.</summary>
    private const int FirstBayX = 0x398;
    /// <summary>The native placement rows repeat three mounting positions every twelve tiles.</summary>
    private const int BaySpacing = 192;
    /// <summary>$86:BFBF/$BFCF use a 2.75-pixel 8.8 bullet speed, rounded in diagonal octants.</summary>
    private const int BulletSpeed = 0x2c0;
    /// <summary>$86:BF9F samples a seventeen-pixel horizontal muzzle radius.</summary>
    private const int MuzzleRadius = 17;

    /// <summary>Returns one of the twelve physical turret definitions.</summary>
    internal static MotherBrainTurretDefinition ForTurret(int parameter)
    {
        if (parameter is < 0 or >= TurretCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain turret parameter must be zero through eleven.");
        }

        int bay = parameter / 3;
        int column = parameter % 3;
        bool wideSector = (bay & 1) != 0;
        int xInset = column == 0 ? 0 : 48 + column * 32;
        MotherBrainTurretDirection initial = column switch
        {
            0 => MotherBrainTurretDirection.DownRight,
            1 => MotherBrainTurretDirection.Right,
            _ when bay == 3 => MotherBrainTurretDirection.DownLeft,
            _ => MotherBrainTurretDirection.Down,
        };
        MotherBrainTurretDirection first = column switch
        {
            0 => MotherBrainTurretDirection.DownLeft,
            1 => wideSector ? MotherBrainTurretDirection.DownLeft : MotherBrainTurretDirection.Down,
            _ => MotherBrainTurretDirection.Left,
        };
        MotherBrainTurretDirection last = column switch
        {
            0 => MotherBrainTurretDirection.DownRight,
            1 => MotherBrainTurretDirection.Right,
            _ => wideSector ? MotherBrainTurretDirection.DownRight : MotherBrainTurretDirection.Down,
        };
        return new(new((ushort)(FirstBayX - bay * BaySpacing - xInset), (ushort)(column == 0 ? 48 : 64)), initial, first, last);
    }

    /// <summary>
    /// The eight turret directions around the compass, from left through down to up-left; a direction's position on
    /// this ring is the octant the turret's rotation steps through and the launch angle's multiple of 45 degrees.
    /// </summary>
    internal static readonly IdentitySequence<MotherBrainTurretDirection> Ring = new(
        MotherBrainTurretDirection.Left, MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.Down,
        MotherBrainTurretDirection.DownRight, MotherBrainTurretDirection.Right, MotherBrainTurretDirection.UpRight,
        MotherBrainTurretDirection.Up, MotherBrainTurretDirection.UpLeft);

    /// <summary>Returns the bullet launch for one of the eight directions.</summary>
    internal static MotherBrainTurretDirectionDefinition ForDirection(
        MotherBrainTurretDirection direction)
    {
        int index = Ring.PositionOf(direction);
        double angle = index * Math.PI / 4;
        int muzzleY = direction switch
        {
            MotherBrainTurretDirection.Left or MotherBrainTurretDirection.Right => -9,
            MotherBrainTurretDirection.DownLeft or MotherBrainTurretDirection.DownRight => 3,
            MotherBrainTurretDirection.Down => 7,
            MotherBrainTurretDirection.UpLeft or MotherBrainTurretDirection.UpRight => -19,
            MotherBrainTurretDirection.Up => -21,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
        return new(new((short)Math.Round(-MuzzleRadius * Math.Cos(angle)), (short)muzzleY),
            new((short)Math.Round(-BulletSpeed * Math.Cos(angle)), (short)Math.Round(BulletSpeed * Math.Sin(angle))));
    }

    /// <summary>Whether turret <paramref name="turret"/> may rotate to <paramref name="direction"/>.</summary>
    internal static bool IsRotationAllowed(int turret, MotherBrainTurretDirection direction)
    {
        MotherBrainTurretDefinition definition = ForTurret(turret);
        int position = Ring.PositionOf(direction);
        return position >= Ring.PositionOf(definition.FirstAllowedDirection) && position <= Ring.PositionOf(definition.LastAllowedDirection);
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>Compiled placement, rotation and bullet-physics definitions.</summary>
internal static class MotherBrainTurretDefinitions
{
    /// <summary>Graphics index installed by both projectile initializers at $86:BE58/$86:BF5F.</summary>
    internal const ushort GraphicsIndex = 0x0400;

    /// <summary>Minimum random rotation delay applied by $86:BFDF.</summary>
    internal const ushort MinimumRotationDelay = 0x0020;

    /// <summary>Minimum random firing cooldown applied by $86:C00A.</summary>
    internal const ushort MinimumFiringCooldown = 0x0080;

    /// <summary>$86:BE89-$BEF8 places four room bays of three turrets.</summary>
    internal const int TurretCount = 12;
    /// <summary>$86:BEB9/$C040 select eight turret poses in left-to-up-left octant order.</summary>
    internal const int DirectionCount = 8;
    /// <summary>$86:BEF9 begins twelve eight-byte allowed-direction rows.</summary>
    internal const ushort RotationPolicyStart = 0xbef9;
    /// <summary>$86:BE89 starts the first bay at X=$398; each later bay is 192 pixels left.</summary>
    private const int FirstBayX = 0x398;
    /// <summary>The native placement rows repeat three mounting positions every twelve tiles.</summary>
    private const int BaySpacing = 192;
    /// <summary>$86:BFBF/$BFCF use a 2.75-pixel 8.8 bullet speed, rounded in diagonal octants.</summary>
    private const int BulletSpeed = 0x2c0;
    /// <summary>$86:BF9F samples a seventeen-pixel horizontal muzzle radius.</summary>
    private const int MuzzleRadius = 17;

    /// <summary>Returns one of the twelve physical turret definitions.</summary>
    internal static MotherBrainTurretDefinition ForTurret(ushort parameter)
    {
        if (parameter >= TurretCount)
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
        byte allowed = column switch
        {
            0 => Sector(MotherBrainTurretDirection.DownLeft, MotherBrainTurretDirection.DownRight),
            1 => Sector(wideSector ? MotherBrainTurretDirection.DownLeft : MotherBrainTurretDirection.Down,
                MotherBrainTurretDirection.Right),
            _ => Sector(MotherBrainTurretDirection.Left,
                wideSector ? MotherBrainTurretDirection.DownRight : MotherBrainTurretDirection.Down),
        };
        return new(new((ushort)(FirstBayX - bay * BaySpacing - xInset), (ushort)(column == 0 ? 48 : 64)),
            (ushort)(RotationPolicyStart + parameter * DirectionCount), initial, allowed);
    }

    /// <summary>Returns the bullet launch for one of the eight directions.</summary>
    internal static MotherBrainTurretDirectionDefinition ForDirection(
        MotherBrainTurretDirection direction)
    {
        int index = (byte)direction;
        if ((uint)index >= DirectionCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Mother Brain turret direction must be zero through seven.");
        }

        double angle = index * Math.PI / 4;
        short muzzleY = direction switch
        {
            MotherBrainTurretDirection.Left or MotherBrainTurretDirection.Right => -9,
            MotherBrainTurretDirection.DownLeft or MotherBrainTurretDirection.DownRight => 3,
            MotherBrainTurretDirection.Down => 7,
            MotherBrainTurretDirection.UpLeft or MotherBrainTurretDirection.UpRight => -19,
            MotherBrainTurretDirection.Up => -21,
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };
        return new(new((short)Math.Round(-MuzzleRadius * Math.Cos(angle)), muzzleY),
            new((short)Math.Round(-BulletSpeed * Math.Cos(angle)), (short)Math.Round(BulletSpeed * Math.Sin(angle))));
    }

    /// <summary>
    /// Resolves a native allowed-rotation pointer and tests the requested direction exactly
    /// as the byte table at <c>$86:BEF9-$86:BF58</c> did.
    /// </summary>
    internal static bool IsRotationAllowed(
        ushort allowedRotationPointer,
        MotherBrainTurretDirection direction)
    {
        int pointerOffset = allowedRotationPointer - RotationPolicyStart;
        if (pointerOffset < 0 || pointerOffset >= TurretCount * DirectionCount || (pointerOffset & 7) != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain turret rotation pointer $86:{allowedRotationPointer:X4} is not an authored policy row.");
        }

        int directionIndex = (byte)direction;
        if ((uint)directionIndex >= DirectionCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Mother Brain turret direction must be zero through seven.");
        }

        return (ForTurret((ushort)(pointerOffset / DirectionCount)).AllowedDirectionMask & (1 << directionIndex)) != 0;
    }

    /// <summary>Builds the contiguous allowed-direction bit mask between two octants.</summary>
    private static byte Sector(MotherBrainTurretDirection first, MotherBrainTurretDirection last) =>
        (byte)(((1 << ((int)last - (int)first + 1)) - 1) << (int)first);
}

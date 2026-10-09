namespace SuperMetroid.Core.Game;

/// <summary>The eight mutually exclusive directions used by Mother Brain's room turrets.</summary>
internal enum MotherBrainTurretDirection : byte
{
    /// <summary>Turret pose aimed horizontally toward screen left.</summary>
    Left = 0,
    /// <summary>Turret pose aimed diagonally down and left.</summary>
    DownLeft = 1,
    /// <summary>Turret pose aimed vertically down.</summary>
    Down = 2,
    /// <summary>Turret pose aimed diagonally down and right.</summary>
    DownRight = 3,
    /// <summary>Turret pose aimed horizontally toward screen right.</summary>
    Right = 4,
    /// <summary>Turret pose aimed diagonally up and right.</summary>
    UpRight = 5,
    /// <summary>Turret pose aimed vertically up.</summary>
    Up = 6,
    /// <summary>Turret pose aimed diagonally up and left.</summary>
    UpLeft = 7,
}

/// <summary>Fixed placement and rotation policy for one of the twelve room turrets.</summary>
/// <param name="X">Whole-pixel horizontal placement in the room.</param>
/// <param name="Y">Whole-pixel vertical placement in the room.</param>
/// <param name="AllowedRotationPointer">Bank-$86 pointer to this turret's eight-byte direction-allowance row.</param>
/// <param name="InitialDirection">Direction selected by the room initializer.</param>
/// <param name="AllowedDirectionMask">Bit mask of directions accepted by the native rotation policy.</param>
internal readonly record struct MotherBrainTurretDefinition(
    ushort X,
    ushort Y,
    ushort AllowedRotationPointer,
    MotherBrainTurretDirection InitialDirection,
    byte AllowedDirectionMask);

/// <summary>Animation selector and physical bullet launch for one turret direction.</summary>
/// <param name="InstructionPointer">Bank-$86 address of the timed pose program for this direction.</param>
/// <param name="BulletXOffset">Horizontal muzzle displacement from the turret origin in pixels.</param>
/// <param name="BulletYOffset">Vertical muzzle displacement from the turret origin in pixels.</param>
/// <param name="BulletXVelocity">Signed horizontal projectile velocity in 8.8 fixed-point units.</param>
/// <param name="BulletYVelocity">Signed vertical projectile velocity in 8.8 fixed-point units.</param>
internal readonly record struct MotherBrainTurretDirectionDefinition(
    ushort InstructionPointer,
    short BulletXOffset,
    short BulletYOffset,
    short BulletXVelocity,
    short BulletYVelocity);

/// <summary>Compiled placement, rotation, animation-selector, and bullet-physics definitions.</summary>
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
    /// <summary>$86:C101-$C12B: each direction has one timed pose followed by Sleep, six bytes total.</summary>
    private const int PoseBytes = 6;

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
        return new((ushort)(FirstBayX - bay * BaySpacing - xInset), (ushort)(column == 0 ? 48 : 64),
            (ushort)(RotationPolicyStart + parameter * DirectionCount), initial, allowed);
    }

    /// <summary>Returns the animation and bullet record for one of the eight directions.</summary>
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
        return new((ushort)(MotherBrainTurretInstructionProgramDefinitions.TurretLeft + index * PoseBytes),
            (short)Math.Round(-MuzzleRadius * Math.Cos(angle)), muzzleY,
            (short)Math.Round(-BulletSpeed * Math.Cos(angle)), (short)Math.Round(BulletSpeed * Math.Sin(angle)));
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

    /// <summary>Builds a contiguous inclusive direction mask for one turret's allowed rotation sector.</summary>
    /// <param name="first">First allowed direction in the enum's octant ordering.</param>
    /// <param name="last">Last allowed direction in the inclusive sector.</param>
    /// <returns>One bit for each direction from <paramref name="first"/> through <paramref name="last"/>.</returns>
    private static byte Sector(MotherBrainTurretDirection first, MotherBrainTurretDirection last) =>
        (byte)(((1 << ((int)last - (int)first + 1)) - 1) << (int)first);
}

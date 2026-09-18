namespace SuperMetroid.Core.Game;

/// <summary>The eight mutually exclusive directions used by Mother Brain's room turrets.</summary>
internal enum MotherBrainTurretDirection : byte
{
    Left = 0,
    DownLeft = 1,
    Down = 2,
    DownRight = 3,
    Right = 4,
    UpRight = 5,
    Up = 6,
    UpLeft = 7,
}

/// <summary>Fixed placement and rotation policy for one of the twelve room turrets.</summary>
internal readonly record struct MotherBrainTurretDefinition(
    ushort X,
    ushort Y,
    ushort AllowedRotationPointer,
    MotherBrainTurretDirection InitialDirection,
    byte AllowedDirectionMask);

/// <summary>Animation selector and physical bullet launch for one turret direction.</summary>
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

    /// <summary>
    /// The twelve parallel placement, rotation-pointer, and initial-direction rows at
    /// <c>$86:BE89-$86:BEF8</c>, with each eight-byte permission row at
    /// <c>$86:BEF9-$86:BF58</c> represented as a direction bit mask.
    /// </summary>
    private static readonly MotherBrainTurretDefinition[] Turrets =
    [
        new(0x0398, 0x0030, 0xbef9, MotherBrainTurretDirection.DownRight, 0x0e),
        new(0x0348, 0x0040, 0xbf01, MotherBrainTurretDirection.Right, 0x1c),
        new(0x0328, 0x0040, 0xbf09, MotherBrainTurretDirection.Down, 0x07),
        new(0x02d8, 0x0030, 0xbf11, MotherBrainTurretDirection.DownRight, 0x0e),
        new(0x0288, 0x0040, 0xbf19, MotherBrainTurretDirection.Right, 0x1e),
        new(0x0268, 0x0040, 0xbf21, MotherBrainTurretDirection.Down, 0x0f),
        new(0x0218, 0x0030, 0xbf29, MotherBrainTurretDirection.DownRight, 0x0e),
        new(0x01c8, 0x0040, 0xbf31, MotherBrainTurretDirection.Right, 0x1c),
        new(0x01a8, 0x0040, 0xbf39, MotherBrainTurretDirection.Down, 0x07),
        new(0x0158, 0x0030, 0xbf41, MotherBrainTurretDirection.DownRight, 0x0e),
        new(0x0108, 0x0040, 0xbf49, MotherBrainTurretDirection.Right, 0x1e),
        new(0x00e8, 0x0040, 0xbf51, MotherBrainTurretDirection.DownLeft, 0x0f),
    ];

    /// <summary>
    /// Direction instruction selectors at <c>$86:BEB9-$86:BEC8</c> and
    /// <c>$86:C040-$86:C04F</c>, paired with the signed offset/velocity rows at
    /// <c>$86:BF9F-$86:BFDE</c>. The two selector copies are identical in the cartridge.
    /// </summary>
    private static readonly MotherBrainTurretDirectionDefinition[] Directions =
    [
        new(0xc101, -17, -9, -0x02c0, 0),
        new(0xc107, -12, 3, -0x01f2, 0x01f2),
        new(0xc10d, 0, 7, 0, 0x02c0),
        new(0xc113, 12, 3, 0x01f2, 0x01f2),
        new(0xc119, 17, -9, 0x02c0, 0),
        new(0xc11f, 12, -19, 0x01f2, -0x01f2),
        new(0xc125, 0, -21, 0, -0x02c0),
        new(0xc12b, -12, -19, -0x01f2, -0x01f2),
    ];

    /// <summary>Returns one of the twelve physical turret definitions.</summary>
    internal static MotherBrainTurretDefinition ForTurret(ushort parameter)
    {
        if (parameter >= Turrets.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter), parameter, "Mother Brain turret parameter must be zero through eleven.");
        }

        return Turrets[parameter];
    }

    /// <summary>Returns the animation and bullet record for one of the eight directions.</summary>
    internal static MotherBrainTurretDirectionDefinition ForDirection(
        MotherBrainTurretDirection direction)
    {
        int index = (byte)direction;
        if ((uint)index >= Directions.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Mother Brain turret direction must be zero through seven.");
        }

        return Directions[index];
    }

    /// <summary>
    /// Resolves a native allowed-rotation pointer and tests the requested direction exactly
    /// as the byte table at <c>$86:BEF9-$86:BF58</c> did.
    /// </summary>
    internal static bool IsRotationAllowed(
        ushort allowedRotationPointer,
        MotherBrainTurretDirection direction)
    {
        int pointerOffset = allowedRotationPointer - 0xbef9;
        if (pointerOffset < 0 || pointerOffset >= Turrets.Length * 8 || (pointerOffset & 7) != 0)
        {
            throw new InvalidDataException(
                $"Mother Brain turret rotation pointer $86:{allowedRotationPointer:X4} is not an authored policy row.");
        }

        int directionIndex = (byte)direction;
        if ((uint)directionIndex >= Directions.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction), direction, "Mother Brain turret direction must be zero through seven.");
        }

        return (Turrets[pointerOffset / 8].AllowedDirectionMask & (1 << directionIndex)) != 0;
    }
}

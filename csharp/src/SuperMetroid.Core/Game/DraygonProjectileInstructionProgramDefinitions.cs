namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Draygon goop and wall-turret projectile instruction lists.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class DraygonProjectileInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Touch</c> at $86:8C38.</summary>
    internal const ushort GoopTouch = 0x8c38;

    /// <summary><c>InstList_EnemyProjectile_DraygonGoop</c> at $86:8C3A.</summary>
    internal const ushort Goop = 0x8c3a;

    /// <summary><c>InstList_EnemyProjectile_DraygonGoop_Shot</c> at $86:8C58.</summary>
    internal const ushort GoopShot = 0x8c58;

    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_0</c> at $86:8CA4.</summary>
    internal const ushort WallTurretBloom = 0x8ca4;

    /// <summary><c>InstList_EnemyProjectile_DraygonsWallTurretProjectile_1</c> at $86:8CE6.</summary>
    internal const ushort WallTurretFlight = 0x8ce6;

    public static int MechanicsWordCount => 38;
    public static int PresentationWordCount => 27;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int address = index switch
        {
            0 => GoopTouch,
            < 7 => Goop + (index - 1) * 4,
            < 10 => Goop + 24 + (index - 7) * 2,
            < 12 => GoopShot + (index - 10) * 4,
            < 16 => GoopShot + 8 + (index - 12) * 2,
            < 33 => WallTurretBloom + (index - 16) * 4,
            < 36 => WallTurretFlight + (index - 33) * 4,
            _ => WallTurretFlight + 12 + (index - 36) * 2,
        };
        return new((ushort)address, ReadMechanicsWord((ushort)address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(index switch
        {
            < 6 => Goop + 2 + index * 4,
            < 8 => GoopShot + 2 + (index - 6) * 4,
            < 24 => WallTurretBloom + 2 + (index - 8) * 4,
            _ => WallTurretFlight + 2 + (index - 24) * 4,
        });
    }
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.DraygonGoop or
        RoomEnemyProjectileKind.DraygonWallTurret;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Draygon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    private static bool TryRead(int address, out ushort value)
    {
        value = address switch
        {
            GoopTouch => EnemyProjectileCodePointers.Instruction_DraygonGoop_SamusCollision,
            Goop + 24 or GoopShot + 10 or WallTurretFlight + 12 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY,
            Goop + 26 => Goop,
            Goop + 28 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep,
            GoopShot + 8 => EnemyProjectileCodePointers.Instruction_SpawnEnemyDropsWithDraygonEyeChances,
            GoopShot + 12 => CommonEnemyProjectileInstructionProgramDefinitions.Delete,
            GoopShot + 14 => EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete,
            WallTurretFlight - 2 => EnemyProjectileCodePointers.Instruction_SetPreInst_DraygonsWallTurretProjectile_Fired,
            WallTurretFlight + 14 => WallTurretFlight,
            _ => 0,
        };
        if (value != 0)
            return true;
        if (IsDraw(address, Goop, 6))
            value = 10;
        else if (IsDraw(address, GoopShot, 2) || IsDraw(address, WallTurretFlight, 3))
            value = 8;
        else if (IsDraw(address, WallTurretBloom, 16))
        {
            int frame = (address - WallTurretBloom) / 4;
            // Two six-pose forming ramps; the second is one tick faster.
            // Four ten-tick charging poses precede the fired callback.
            value = (ushort)(frame < 12
                ? Math.Max(3 - frame / 6, 5 - frame / 6 - frame % 6)
                : 10);
        }
        return value != 0;
    }

    private static bool IsDraw(int address, ushort first, int frames)
    {
        int offset = address - first;
        return offset >= 0 && offset < frames * 4 && offset % 4 == 0;
    }

    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        TryRead((ushort)(address & ~1), out _);
}
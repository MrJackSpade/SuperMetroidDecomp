namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Draygon goop and wall-turret projectile instruction lists.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal abstract class DraygonProjectileInstructionProgramDefinitions
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

    /// <summary>Number of spritemap operands interleaved with the compiled Draygon projectile control words.</summary>
    public static int PresentationWordCount => 27;

    /// <summary>Maps an ordinal in the presentation operand set to its native instruction-list address.</summary>
    /// <param name="index">Zero-based index among goop and wall-turret spritemap operands.</param>
    /// <returns>The bank-$86 address of the selected presentation word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation-word range.</exception>
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
    /// <summary>Determines whether this compiled program handles a Draygon goop or wall-turret projectile.</summary>
    /// <param name="kind">Projectile kind to classify.</param>
    /// <returns>True for Draygon goop and wall-turret projectiles.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.DraygonGoop or
        RoomEnemyProjectileKind.DraygonWallTurret;

    /// <summary>Reads a compiled Draygon instruction or timing operand by its native bank-$86 address.</summary>
    /// <param name="address">Address of the requested control word.</param>
    /// <returns>The compiled mechanics value at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not represented by a compiled Draygon projectile program.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Draygon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve a Draygon projectile control or frame-duration word, excluding live spritemap operands.</summary>
    /// <param name="address">Bank-$86 address to look up.</param>
    /// <param name="value">Receives the compiled mechanics value when the address is recognized.</param>
    /// <returns>True when the address belongs to a compiled control or duration word.</returns>
    internal static bool TryRead(int address, out ushort value)
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

    /// <summary>Tests whether an address is one of the frame commands in a contiguous spritemap-bearing instruction list.</summary>
    /// <param name="address">Instruction address being classified.</param>
    /// <param name="first">Address of the list's first frame command.</param>
    /// <param name="frames">Number of four-byte frame commands in the list.</param>
    /// <returns>True for an aligned frame-command address within the list.</returns>
    private static bool IsDraw(int address, ushort first, int frames)
    {
        int offset = address - first;
        return offset >= 0 && offset < frames * 4 && offset % 4 == 0;
    }
}

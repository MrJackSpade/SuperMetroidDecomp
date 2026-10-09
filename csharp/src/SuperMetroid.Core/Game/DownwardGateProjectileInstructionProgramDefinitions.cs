namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for both downward-gate actors. Spritemap operands remain live cartridge
/// presentation data; movement continues to use the shared translated pre-instruction.
/// </summary>
internal abstract class DownwardGateProjectileInstructionProgramDefinitions
{
    /// <summary>Downward-moving gate instruction list at $86:E53C.</summary>
    internal const ushort Moving = 0xe53c;

    /// <summary>Closed gate instruction list at $86:E55E.</summary>
    internal const ushort Closed = 0xe55e;

    /// <summary>Initial closed-gate sleep instruction at $86:E566.</summary>
    internal const ushort ClosedSleep = 0xe566;

    /// <summary>Number of instruction words whose operands control projectile behavior.</summary>
    public static int MechanicsWordCount => 28;

    /// <summary>Number of spritemap pointer words left as live presentation data.</summary>
    public static int PresentationWordCount => 9;

    /// <summary>
    /// Closing and opening each traverse four one-tick pose/sleep stages, advanced by
    /// the movement pre-instruction. The closed hold reverses velocity and waits before
    /// re-enabling movement; completed opening deletes the projectile.
    /// </summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index is >= 4 and < 12)
            return MovementPoseWord((ushort)(Moving + 8), index - 4);
        if (index is >= 19 and < 27)
            return MovementPoseWord((ushort)(Closed + 14), index - 19);
        return index switch
        {
            0 => new(Moving, DownwardGateEnemyProjectileRomData.SetYVelocityInstruction),
            1 => new((ushort)(Moving + 2), 0x0100),
            2 => new((ushort)(Moving + 4), EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
            3 => new((ushort)(Moving + 6), DownwardGateEnemyProjectileRomData.MovementPreInstruction),
            12 => new((ushort)(Closed - 2), EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
            13 => new(Closed, DownwardGateEnemyProjectileRomData.SetYVelocityInstruction),
            14 => new((ushort)(Closed + 2), 0xff00),
            15 => new((ushort)(Closed + 4), 1),
            16 => new(ClosedSleep, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep),
            17 => new((ushort)(Closed + 10), EnemyProjectileCodePointers.Instruction_EnemyProjectile_PreInstructionInY),
            18 => new((ushort)(Closed + 12), DownwardGateEnemyProjectileRomData.MovementPreInstruction),
            _ => new((ushort)(Closed + 38), EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
        };
    }

    /// <summary>Describes one pose or sleep operand in a four-stage gate movement sequence.</summary>
    /// <param name="start">Address of the first pose instruction in the sequence.</param>
    /// <param name="word">Zero-based mechanics-word offset within the sequence.</param>
    /// <returns>The instruction address and the pose pointer or sleep opcode stored there.</returns>
    private static InstructionMechanicsWord MovementPoseWord(ushort start, int word)
    {
        bool sleep = (word & 1) != 0;
        return new((ushort)(start + 6 * (word / 2) + (sleep ? 4 : 0)),
            sleep ? EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep : (ushort)1);
    }

    /// <summary>Maps a presentation-word index to its native spritemap operand address.</summary>
    /// <param name="index">Zero-based index within the live presentation words.</param>
    /// <returns>Address of the corresponding word in the moving or closed instruction list.</returns>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(index < 4 ? Moving + 10 + 6 * index
            : index == 4 ? Closed + 6 : Closed + 16 + 6 * (index - 5));
    }
    /// <summary>Reports whether this compiled definition controls either downward-gate projectile variant.</summary>
    /// <param name="kind">Projectile kind to test.</param>
    /// <returns><see langword="true"/> for the moving or closed downward gate.</returns>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.DownwardGateMoving or
        RoomEnemyProjectileKind.DownwardGateClosed;

    /// <summary>Resolves the compiled behavioral operand stored at a native instruction address.</summary>
    /// <param name="address">Instruction-list address to look up.</param>
    /// <returns>The compiled mechanics word at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not part of the compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Downward-gate projectile mechanics pointer $86:{address:X4} is not compiled.");
    }
}

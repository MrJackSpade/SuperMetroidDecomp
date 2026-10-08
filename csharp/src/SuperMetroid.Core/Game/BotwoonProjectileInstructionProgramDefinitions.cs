namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Botwoon's articulated body, tail, hidden, and spit projectile
/// programs at $86:E80F-$E8F7 and $86:EBAE-$EBC5. Their forty-six spritemap operands
/// select installed presentation artwork.
/// </summary>
internal abstract class BotwoonProjectileInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_UpLeft</c> at $86:E80F.</summary>
    internal const ushort BodyUpLeft = 0xe80f;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_Up_FacingRight</c> at $86:E8C3.</summary>
    internal const ushort TailUpFacingRight = 0xe8c3;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBodyTail_Hidden</c> at $86:E8F3.</summary>
    internal const ushort Hidden = 0xe8f3;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsSpit</c> at $86:EBAE.</summary>
    internal const ushort Spit = 0xebae;
    public static int PresentationWordCount => 46;
    internal const int BodyProgramCount = 17;

    internal static ushort BodyProgram(int index)
    {
        if ((uint)index >= BodyProgramCount) throw new IndexOutOfRangeException();
        // Nine physical body slots include the unused fourth slot.
        return index < 8
            ? (ushort)(BodyUpLeft + 20 * (index < 3 ? index : index + 1))
            : (ushort)(TailUpFacingRight + 6 * (index - 8));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 32) return (ushort)(BodyProgram(index / 4) + 4 * (index % 4) + 2);
        if (index < 41) return (ushort)(BodyProgram(index - 24) + 2);
        return (ushort)(Spit + 4 * (index - 41) + 2);
    }

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BotwoonBody or RoomEnemyProjectileKind.BotwoonSpit;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryBodyOffset(address, out int offset))
        {
            if (offset < 16 && offset % 4 == 0) return 8;
            if (offset == 16) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
            if (offset == 18) return (ushort)(address - offset);
        }
        int sleeping = address - TailUpFacingRight;
        if ((uint)sleeping < 54)
        {
            if (sleeping % 6 == 0) return 1;
            if (sleeping % 6 == 4) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep;
        }
        int spit = address - Spit;
        if ((uint)spit < 20 && spit % 4 == 0) return 3;
        if (spit == 20) return EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY;
        if (spit == 22) return Spit;
        throw new InvalidDataException(
            $"Botwoon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsPresentationWord(ushort address)
    {
        if (TryBodyOffset(address, out int offset)) return offset < 16 && offset % 4 == 2;
        int sleeping = address - TailUpFacingRight;
        if ((uint)sleeping < 54) return sleeping % 6 == 2;
        int spit = address - Spit;
        return (uint)spit < 20 && spit % 4 == 2;
    }

    internal static bool TryBodyOffset(ushort address, out int offset)
    {
        int relative = address - BodyUpLeft;
        offset = relative % 20;
        return (uint)relative < 180 && relative / 20 != 3;
    }
}

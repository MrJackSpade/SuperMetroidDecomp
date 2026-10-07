namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control for Botwoon's articulated body, tail, hidden, and spit projectile
/// programs at $86:E80F-$E8F7 and $86:EBAE-$EBC5. Their forty-six spritemap operands
/// select installed presentation artwork.
/// </summary>
internal abstract class BotwoonProjectileInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_UpLeft</c> at $86:E80F.</summary>
    internal const ushort BodyUpLeft = 0xe80f;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_Left</c> at $86:E823.</summary>
    internal const ushort BodyLeft = 0xe823;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_DownLeft</c> at $86:E837.</summary>
    internal const ushort BodyDownLeft = 0xe837;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_Down_FacingRight</c> at $86:E85F.</summary>
    internal const ushort BodyDownFacingRight = 0xe85f;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_DownRight</c> at $86:E873.</summary>
    internal const ushort BodyDownRight = 0xe873;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_Right</c> at $86:E887.</summary>
    internal const ushort BodyRight = 0xe887;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_UpRight</c> at $86:E89B.</summary>
    internal const ushort BodyUpRight = 0xe89b;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBody_Up_FacingRight</c> at $86:E8AF.</summary>
    internal const ushort BodyUpFacingRight = 0xe8af;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_Up_FacingRight</c> at $86:E8C3.</summary>
    internal const ushort TailUpFacingRight = 0xe8c3;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_UpLeft</c> at $86:E8C9.</summary>
    internal const ushort TailUpLeft = 0xe8c9;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_Left</c> at $86:E8CF.</summary>
    internal const ushort TailLeft = 0xe8cf;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_DownLeft</c> at $86:E8D5.</summary>
    internal const ushort TailDownLeft = 0xe8d5;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_Down</c> at $86:E8DB.</summary>
    internal const ushort TailDown = 0xe8db;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_DownRight</c> at $86:E8E1.</summary>
    internal const ushort TailDownRight = 0xe8e1;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_Right</c> at $86:E8E7.</summary>
    internal const ushort TailRight = 0xe8e7;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsTail_UpRight</c> at $86:E8ED.</summary>
    internal const ushort TailUpRight = 0xe8ed;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsBodyTail_Hidden</c> at $86:E8F3.</summary>
    internal const ushort Hidden = 0xe8f3;
    /// <summary><c>InstList_EnemyProjectile_BotwoonsSpit</c> at $86:EBAE.</summary>
    internal const ushort Spit = 0xebae;

    public static int MechanicsWordCount => 73;
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

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 48)
        {
            int command = index % 6;
            address = BodyProgram(index / 6) + (command < 5 ? 4 * command : 18);
        }
        else if (index < 66)
            address = BodyProgram(8 + (index - 48) / 2) + 4 * ((index - 48) % 2);
        else
        {
            int command = index - 66;
            address = Spit + (command < 6 ? 4 * command : 22);
        }
        return new((ushort)address, ReadMechanicsWord((ushort)address));
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

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & ~1);
        // Body/tail programs start at odd addresses; spit starts even.
        ushort bodyWord = unchecked((ushort)(((ushort)address - BodyUpLeft & ~1) + BodyUpLeft));
        if (TryBodyOffset(bodyWord, out int offset))
            return offset >= 16 || offset % 4 == 0;
        int sleeping = bodyWord - TailUpFacingRight;
        if ((uint)sleeping < 54) return sleeping % 6 != 2;
        int spit = word - Spit;
        return (uint)spit < 24 && (spit >= 20 || spit % 4 == 0);
    }

    private static bool TryBodyOffset(ushort address, out int offset)
    {
        int relative = address - BodyUpLeft;
        offset = relative % 20;
        return (uint)relative < 180 && relative / 20 != 3;
    }
}

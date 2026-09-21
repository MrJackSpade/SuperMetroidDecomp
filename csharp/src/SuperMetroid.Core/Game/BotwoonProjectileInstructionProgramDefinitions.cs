namespace SuperMetroid.Core.Game;

internal readonly record struct BotwoonProjectileInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled control for Botwoon's articulated body, tail, hidden, and spit projectile
/// programs at $86:E80F-$E8F7 and $86:EBAE-$EBC5. Their forty-six spritemap operands
/// remain live presentation data.
/// </summary>
internal static class BotwoonProjectileInstructionProgramDefinitions
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

    private static readonly ushort[] LoopingBodyPrograms =
    [
        BodyUpLeft, BodyLeft, BodyDownLeft, BodyDownFacingRight,
        BodyDownRight, BodyRight, BodyUpRight, BodyUpFacingRight,
    ];

    private static readonly ushort[] SleepingBodyPrograms =
    [
        TailUpFacingRight, TailUpLeft, TailLeft, TailDownLeft,
        TailDown, TailDownRight, TailRight, TailUpRight, Hidden,
    ];

    private static readonly BotwoonProjectileInstructionMechanicsWord[] Words =
        BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static int BodyProgramCount => LoopingBodyPrograms.Length + SleepingBodyPrograms.Length;
    internal static BotwoonProjectileInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];
    internal static ushort BodyProgram(int index) => index < LoopingBodyPrograms.Length
        ? LoopingBodyPrograms[index]
        : SleepingBodyPrograms[index - LoopingBodyPrograms.Length];

    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.BotwoonBody or
        RoomEnemyProjectileKind.BotwoonSpit;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            BotwoonProjectileInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }

        throw new InvalidDataException(
            $"Botwoon projectile mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        foreach (BotwoonProjectileInstructionMechanicsWord word in Words)
        {
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }

    private static BotwoonProjectileInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<BotwoonProjectileInstructionMechanicsWord>(73);
        foreach (ushort program in LoopingBodyPrograms)
        {
            words.Add(new(program, 8));
            words.Add(new(unchecked((ushort)(program + 4)), 8));
            words.Add(new(unchecked((ushort)(program + 8)), 8));
            words.Add(new(unchecked((ushort)(program + 12)), 8));
            words.Add(new(unchecked((ushort)(program + 16)),
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY));
            words.Add(new(unchecked((ushort)(program + 18)), program));
        }
        foreach (ushort program in SleepingBodyPrograms)
        {
            words.Add(new(program, 1));
            words.Add(new(unchecked((ushort)(program + 4)),
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep));
        }
        for (int frame = 0; frame < 5; frame++)
            words.Add(new(unchecked((ushort)(Spit + frame * 4)), 3));
        words.Add(new(0xebc2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_GotoY));
        words.Add(new(0xebc4, Spit));
        return [.. words];
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(46);
        foreach (ushort program in LoopingBodyPrograms)
        {
            for (int frame = 0; frame < 4; frame++)
                words.Add(unchecked((ushort)(program + frame * 4 + 2)));
        }
        foreach (ushort program in SleepingBodyPrograms)
            words.Add(unchecked((ushort)(program + 2)));
        for (int frame = 0; frame < 5; frame++)
            words.Add(unchecked((ushort)(Spit + frame * 4 + 2)));
        return [.. words];
    }
}

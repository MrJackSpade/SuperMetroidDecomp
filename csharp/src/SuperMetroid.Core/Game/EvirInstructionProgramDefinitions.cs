namespace SuperMetroid.Core.Game;

/// <summary>One compiled Evir mechanics word at its native bank-$A8 address.</summary>
internal readonly record struct EvirInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, callback, timer, and loop control for Evir's body, arms, and
/// regenerating projectile. Interleaved spritemap operands remain live presentation data.
/// </summary>
internal static class EvirInstructionProgramDefinitions
{
    /// <summary><c>InstList_Evir_Body_FacingLeft</c> at $A8:86A7.</summary>
    internal const ushort BodyFacingLeft = 0x86a7;
    /// <summary><c>InstList_Evir_Arms_FacingLeft</c> at $A8:86C3.</summary>
    internal const ushort ArmsFacingLeft = 0x86c3;
    /// <summary><c>InstList_Evir_Body_FacingRight</c> at $A8:870B.</summary>
    internal const ushort BodyFacingRight = 0x870b;
    /// <summary><c>InstList_Evir_Arms_FacingRight</c> at $A8:8727.</summary>
    internal const ushort ArmsFacingRight = 0x8727;
    /// <summary><c>InstList_Evir_Projectile_Normal</c> at $A8:876F.</summary>
    internal const ushort ProjectileNormal = 0x876f;
    /// <summary><c>InstList_Evir_Projectile_Regenerating_0</c> at $A8:8775.</summary>
    internal const ushort ProjectileRegenerating = 0x8775;
    /// <summary><c>InstList_Evir_Projectile_Regenerating_1</c> at $A8:877D.</summary>
    internal const ushort ProjectileRegenerationLoop = 0x877d;
    /// <summary>First native Evir instruction callback at $A8:878F.</summary>
    internal const ushort AdjacentCallbackCode = 0x878f;

    internal const int BodyFrameCount = 6;
    internal const int ArmsFrameCount = 17;

    private static readonly EvirInstructionMechanicsWord[] Words = BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static EvirInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Evir control or rejects pointers outside its six programs.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EvirInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Evir instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }

    private static EvirInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<EvirInstructionMechanicsWord>(capacity: 67);
        AddLoop(words, BodyFacingLeft, BodyFrameCount, lastDuration: 10);
        AddLoop(words, ArmsFacingLeft, ArmsFrameCount, lastDuration: 48);
        AddLoop(words, BodyFacingRight, BodyFrameCount, lastDuration: 10);
        AddLoop(words, ArmsFacingRight, ArmsFrameCount, lastDuration: 48);

        words.Add(new(ProjectileNormal, 1));
        words.Add(new(unchecked((ushort)(ProjectileNormal + 4)),
            CommonEnemyInstructionCodes.Sleep));

        words.Add(new(ProjectileRegenerating,
            EnemyInstructionCodePointers.Instruction_Evir_SetInitialRegenerationXOffset));
        words.Add(new(unchecked((ushort)(ProjectileRegenerating + 2)),
            CommonEnemyInstructionCodes.SetTimer));
        words.Add(new(unchecked((ushort)(ProjectileRegenerating + 4)), 8));
        words.Add(new(unchecked((ushort)(ProjectileRegenerating + 6)),
            EnemyInstructionCodePointers.Instruction_Evir_PlaySpitSFX));
        words.Add(new(ProjectileRegenerationLoop, 8));
        words.Add(new(unchecked((ushort)(ProjectileRegenerationLoop + 4)),
            EnemyInstructionCodePointers.Instruction_Evir_AdvanceRegenerationXOffset));
        words.Add(new(unchecked((ushort)(ProjectileRegenerationLoop + 6)),
            CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate));
        words.Add(new(unchecked((ushort)(ProjectileRegenerationLoop + 8)),
            ProjectileRegenerationLoop));
        words.Add(new(unchecked((ushort)(ProjectileRegenerationLoop + 10)), 16));
        words.Add(new(unchecked((ushort)(ProjectileRegenerationLoop + 14)),
            EnemyInstructionCodePointers.Instruction_Evir_FinishRegeneration));
        words.Add(new(unchecked((ushort)(ProjectileRegenerationLoop + 16)),
            CommonEnemyInstructionCodes.Sleep));

        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 49);
        AddLoopPresentation(words, BodyFacingLeft, BodyFrameCount);
        AddLoopPresentation(words, ArmsFacingLeft, ArmsFrameCount);
        AddLoopPresentation(words, BodyFacingRight, BodyFrameCount);
        AddLoopPresentation(words, ArmsFacingRight, ArmsFrameCount);
        words.Add(unchecked((ushort)(ProjectileNormal + 2)));
        words.Add(unchecked((ushort)(ProjectileRegenerationLoop + 2)));
        words.Add(unchecked((ushort)(ProjectileRegenerationLoop + 12)));
        return words.ToArray();
    }

    private static void AddLoop(
        List<EvirInstructionMechanicsWord> words,
        ushort entry,
        int frameCount,
        ushort lastDuration)
    {
        for (int frame = 0; frame < frameCount; frame++)
        {
            words.Add(new(
                unchecked((ushort)(entry + frame * 4)),
                frame == frameCount - 1 ? lastDuration : (ushort)10));
        }
        ushort gotoAddress = unchecked((ushort)(entry + frameCount * 4));
        words.Add(new(gotoAddress, CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(gotoAddress + 2)), entry));
    }

    private static void AddLoopPresentation(
        List<ushort> words,
        ushort entry,
        int frameCount)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(unchecked((ushort)(entry + frame * 4 + 2)));
    }
}

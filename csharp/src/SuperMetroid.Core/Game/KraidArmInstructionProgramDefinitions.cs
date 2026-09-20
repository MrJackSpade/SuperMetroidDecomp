namespace SuperMetroid.Core.Game;

/// <summary>One compiled Kraid-arm mechanics word at its bank-$A7 address.</summary>
internal readonly record struct KraidArmInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and control for Kraid's independently scheduled arm actor. The
/// interleaved extended-spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class KraidArmInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidArm_Normal_0</c> at $A7:89F3.</summary>
    internal const ushort Normal = 0x89f3;
    /// <summary><c>InstList_KraidArm_Normal_1</c> at $A7:8A37.</summary>
    internal const ushort NormalPause = 0x8a37;
    /// <summary><c>InstList_KraidArm_Slow</c> at $A7:8A41.</summary>
    internal const ushort Slow = 0x8a41;
    /// <summary><c>InstList_KraidArm_RisingSinking</c> at $A7:8AA4.</summary>
    internal const ushort RisingOrSinking = 0x8aa4;
    /// <summary><c>InstList_KraidArm_Dying_PreparingToLungeForward</c> at $A7:8AF0.</summary>
    internal const ushort DyingOrPreparingToLunge = 0x8af0;
    /// <summary>First adjacent Kraid-lint instruction program at $A7:8AFE.</summary>
    internal const ushort AdjacentLintProgram = 0x8afe;

    private static readonly KraidArmInstructionMechanicsWord[] Words = BuildWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static KraidArmInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Kraid-arm control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < Words.Length; index++)
        {
            if (Words[index].Address == address)
                return Words[index].Value;
        }

        throw new InvalidDataException(
            $"Kraid arm mechanics pointer $A7:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa70000)
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

    private static KraidArmInstructionMechanicsWord[] BuildWords()
    {
        var words = new List<KraidArmInstructionMechanicsWord>(capacity: 66);
        AddFrames(words, Normal, frameCount: 17, duration: 6);
        words.Add(new(NormalPause, 0x0020));
        words.Add(new(0x8a3b,
            EnemyInstructionCodePointers.Instruction_KraidArm_SlowArmIfLessThanHalfHealth));
        words.Add(new(0x8a3d, CommonEnemyInstructionCodes.Goto));
        words.Add(new(0x8a3f, Normal));

        AddFrames(words, Slow, frameCount: 17, duration: 8);
        words.Add(new(0x8a85, 0x0030));
        words.Add(new(0x8a89,
            EnemyInstructionCodePointers.Instruction_KraidArm_SlowArmIfLessThanHalfHealth));
        words.Add(new(0x8a8b, CommonEnemyInstructionCodes.Goto));
        words.Add(new(0x8a8d, Slow));

        AddFrames(words, RisingOrSinking, frameCount: 17, duration: 6);
        words.Add(new(0x8ae8, 0x0020));
        words.Add(new(0x8aec, CommonEnemyInstructionCodes.Goto));
        words.Add(new(0x8aee, RisingOrSinking));

        words.Add(new(DyingOrPreparingToLunge, 6));
        words.Add(new(0x8af4, 6));
        words.Add(new(0x8af8, 0x7fff));
        words.Add(new(0x8afc, CommonEnemyInstructionCodes.Sleep));
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 57);
        AddFramePresentation(words, Normal, frameCount: 17);
        words.Add(unchecked((ushort)(NormalPause + 2)));
        AddFramePresentation(words, Slow, frameCount: 17);
        words.Add(0x8a87);
        AddFramePresentation(words, RisingOrSinking, frameCount: 17);
        words.Add(0x8aea);
        words.Add(0x8af2);
        words.Add(0x8af6);
        words.Add(0x8afa);
        return words.ToArray();
    }

    private static void AddFrames(
        List<KraidArmInstructionMechanicsWord> words,
        ushort entry,
        int frameCount,
        ushort duration)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(new(unchecked((ushort)(entry + frame * 4)), duration));
    }

    private static void AddFramePresentation(
        List<ushort> words,
        ushort entry,
        int frameCount)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(unchecked((ushort)(entry + frame * 4 + 2)));
    }
}

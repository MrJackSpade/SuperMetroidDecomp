namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from all eight Fune and Namihe instruction programs.</summary>
/// <remarks>
/// Durations, callbacks, common sleep/goto opcodes, and loop targets affect simulation and
/// live here. Each word following a duration selects replaceable spritemap presentation and
/// resolves through separately compiled presentation selectors.
/// </remarks>
internal abstract class FuneNamiheInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>$A8:939F</c>, active left-facing Fune program.</summary>
    internal const ushort FuneActiveLeft = 0x939f;

    /// <summary><c>$A8:93CF</c>, active right-facing Fune program.</summary>
    internal const ushort FuneActiveRight = 0x93cf;

    /// <summary><c>$A8:9399</c>, idle left-facing Fune program.</summary>
    internal const ushort FuneIdleLeft = 0x9399;

    /// <summary><c>$A8:93C9</c>, idle right-facing Fune program.</summary>
    internal const ushort FuneIdleRight = 0x93c9;

    /// <summary><c>$A8:95C3</c>, active left-facing Namihe program.</summary>
    internal const ushort NamiheActiveLeft = 0x95c3;

    /// <summary><c>$A8:95F7</c>, active right-facing Namihe program.</summary>
    internal const ushort NamiheActiveRight = 0x95f7;

    /// <summary><c>$A8:95BD</c>, idle left-facing Namihe program.</summary>
    internal const ushort NamiheIdleLeft = 0x95bd;

    /// <summary><c>$A8:95F1</c>, idle right-facing Namihe program.</summary>
    internal const ushort NamiheIdleRight = 0x95f1;

    public static int MechanicsWordCount => 62;
    public static int PresentationWordCount => 38;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool namihe = index >= 30;
        int wordsPerFacing = namihe ? 16 : 15;
        int speciesIndex = namihe ? index - 30 : index;
        int word = speciesIndex % wordsPerFacing;
        int openingFrames = namihe ? 5 : 4;
        int offset;
        if (word < 2) offset = word * 4; // Idle frame and sleep.
        else if (word < openingFrames + 2) offset = 6 + 4 * (word - 2);
        else if (word < openingFrames + 4) offset = 6 + 4 * openingFrames + 2 * (word - openingFrames - 2);
        else if (word < openingFrames + 8) offset = 10 + 4 * openingFrames + 4 * (word - openingFrames - 4);
        else offset = 26 + 4 * openingFrames + 2 * (word - openingFrames - 8);
        ushort address = (ushort)((namihe ? NamiheIdleLeft : FuneIdleLeft) +
            (namihe ? 52 : 48) * (speciesIndex / wordsPerFacing) + offset);
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        bool namihe = index >= 18;
        int frames = namihe ? 10 : 9;
        int speciesIndex = namihe ? index - 18 : index;
        int frame = speciesIndex % frames;
        int offset = frame == 0 ? 2 : 4 + 4 * frame + (frame > (namihe ? 5 : 4) ? 4 : 0);
        return (ushort)((namihe ? NamiheIdleLeft : FuneIdleLeft) +
            (namihe ? 52 : 48) * (speciesIndex / frames) + offset);
    }

    /// <summary>Normalize species and facing without accepting the intervening artwork.</summary>
    private static bool TryLocate(ushort address, out bool namihe, out bool right, out int local)
    {
        namihe = address >= NamiheIdleLeft;
        int stride = namihe ? 52 : 48;
        int offset = address - (namihe ? NamiheIdleLeft : FuneIdleLeft);
        right = offset >= stride;
        local = offset % stride;
        return (uint)offset < 2 * stride;
    }

    /// <summary>Whether an operand in the eight native lists owns a visual frame pointer.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        if (!TryLocate(address, out bool namihe, out _, out int local)) return false;
        int openingFrames = namihe ? 5 : 4;
        int recovery = 12 + 4 * openingFrames;
        return local == 2 ||
            (local >= 8 && local < 8 + 4 * openingFrames && local % 4 == 0) ||
            (local >= recovery && local < recovery + 16 && local % 4 == 0);
    }

    /// <summary>Idle/sleep, opening frames, fire/sound, recovery frames, finish/goto idle.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryLocate(address, out bool namihe, out bool right, out int local))
        {
            int fire = 6 + 4 * (namihe ? 5 : 4);
            if (local == 0) return 1;
            if (local == 4) return CommonEnemyInstructionCodes.Sleep;
            if ((local >= 6 && local < fire && local % 4 == 2) ||
                (local >= fire + 4 && local < fire + 20 && local % 4 == 2))
                return (ushort)(!namihe && (local == 6 || local == fire + 4) ? 16 : 8);
            if (local == fire)
                return namihe
                    ? right ? EnemyInstructionCodePointers.Instruction_Namihe_SpawnFireball_FacingRight
                        : EnemyInstructionCodePointers.Instruction_Namihe_SpawnFireball_FacingLeft
                    : right ? EnemyInstructionCodePointers.Instruction_Fune_SpawnFireball_FacingRight
                        : EnemyInstructionCodePointers.Instruction_Fune_SpawnFireball_FacingLeft;
            if (local == fire + 2) return EnemyInstructionCodePointers.Instruction_FuneNamihe_QueueSpitSFX;
            if (local == fire + 20) return right
                ? EnemyInstructionCodePointers.Instruction_FuneNamihe_FinishActivity_duplicate
                : EnemyInstructionCodePointers.Instruction_FuneNamihe_FinishActivity;
            if (local == fire + 22) return CommonEnemyInstructionCodes.Goto;
            if (local == fire + 24) return (ushort)(address - local);
        }
        throw new InvalidDataException(
            $"Fune/Namihe instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000) return false;
        ushort bankAddress = unchecked((ushort)address);
        if (!TryLocate(bankAddress, out _, out _, out int local)) return false;
        // Every program begins at an odd bank address; align relative to that start.
        ushort word = (ushort)(bankAddress - (local & 1));
        return !IsPresentationWord(word);
    }
}

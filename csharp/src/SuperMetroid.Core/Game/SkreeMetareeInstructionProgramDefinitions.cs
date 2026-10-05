namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct SkreeMetareeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from the parallel Skree and Metaree programs.</summary>
internal static class SkreeMetareeInstructionProgramDefinitions
{
    /// <summary><c>InstList_Metaree_Idling</c> at $A3:8910.</summary>
    internal const ushort MetareeIdling = 0x8910;
    /// <summary><c>InstList_Metaree_PrepareToLaunchAttack</c> at $A3:8924.</summary>
    internal const ushort MetareePreparingAttack = 0x8924;
    /// <summary><c>InstList_Metaree_LaunchedAttack_0</c> at $A3:8930.</summary>
    internal const ushort MetareeDiving = 0x8930;
    /// <summary><c>UNUSED_InstList_Metaree_StopAnimating_A38946</c> at $A3:8946.</summary>
    internal const ushort MetareeStopAnimating = 0x8946;

    /// <summary><c>InstList_Skree_Idling</c> at $A3:C65E.</summary>
    internal const ushort SkreeIdling = 0xc65e;
    /// <summary><c>InstList_Skree_PrepareToLaunchAttack</c> at $A3:C672.</summary>
    internal const ushort SkreePreparingAttack = 0xc672;
    /// <summary><c>InstList_Skree_LaunchedAttack_0</c> at $A3:C67E.</summary>
    internal const ushort SkreeDiving = 0xc67e;
    /// <summary><c>UNUSED_InstList_Skree_StopAnimating_A3C694</c> at $A3:C694.</summary>
    internal const ushort SkreeStopAnimating = 0xc694;

    // Idle 10, preparation 16/8, dive 2 and stop 1 holds remain required independent timing inputs under #1165.
    internal static int MechanicsWordCount(bool metaree) => 20;
    internal static int PresentationWordCount(bool metaree) => 11;

    internal static SkreeMetareeInstructionMechanicsWord MechanicsWord(bool metaree, int index)
    {
        if ((uint)index >= 20) throw new IndexOutOfRangeException();
        ushort idle = metaree ? MetareeIdling : SkreeIdling;
        ushort preparation = metaree ? MetareePreparingAttack : SkreePreparingAttack;
        ushort dive = metaree ? MetareeDiving : SkreeDiving;
        ushort stop = metaree ? MetareeStopAnimating : SkreeStopAnimating;
        if (index < 6)
            return new((ushort)(idle + (index < 5 ? 4 * index : 18)),
                index < 4 ? (ushort)10 : index == 4 ? CommonEnemyInstructionCodes.Goto : idle);
        if (index < 10)
        {
            int local = index - 6;
            ushort value = local switch
            {
                0 => 16,
                1 => 8,
                2 => metaree ? EnemyInstructionCodePointers.Instruction_Metaree_SetAttackReadyFlag
                    : EnemyInstructionCodePointers.Instruction_Skree_SetAttackReadyFlag,
                _ => CommonEnemyInstructionCodes.Sleep,
            };
            return new((ushort)(preparation + (local < 3 ? 4 * local : 10)), value);
        }
        if (index == 10) return new(dive, CommonEnemyInstructionCodes.EnableOffScreenProcessing);
        if (index < 17)
        {
            int local = index - 11;
            return new((ushort)(dive + 2 + (local < 5 ? 4 * local : 18)),
                local < 4 ? (ushort)2 : local == 4 ? CommonEnemyInstructionCodes.Goto : (ushort)(dive + 2));
        }
        int terminal = index - 17;
        return new((ushort)(stop + (terminal < 2 ? 2 * terminal : 6)), terminal switch
        {
            0 => CommonEnemyInstructionCodes.DisableOffScreenProcessing,
            1 => 1,
            _ => CommonEnemyInstructionCodes.Sleep,
        });
    }

    internal static ushort PresentationWordAddress(bool metaree, int index)
    {
        if ((uint)index >= 11) throw new IndexOutOfRangeException();
        if (index < 4) return (ushort)((metaree ? MetareeIdling : SkreeIdling) + 2 + 4 * index);
        if (index < 6) return (ushort)((metaree ? MetareePreparingAttack : SkreePreparingAttack) + 2 + 4 * (index - 4));
        if (index < 10) return (ushort)((metaree ? MetareeDiving : SkreeDiving) + 4 + 4 * (index - 6));
        return (ushort)((metaree ? MetareeStopAnimating : SkreeStopAnimating) + 4);
    }

    internal static ushort ReadMetareeMechanicsWord(ushort address) => ReadMechanicsWord(true, address);
    internal static ushort ReadSkreeMechanicsWord(ushort address) => ReadMechanicsWord(false, address);

    private static ushort ReadMechanicsWord(bool metaree, ushort address)
    {
        for (int index = 0; index < 20; index++)
        {
            var word = MechanicsWord(metaree, index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException($"{(metaree ? "Metaree" : "Skree")} instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int species = 0; species < 2; species++)
        for (int index = 0; index < 20; index++)
        {
            ushort word = MechanicsWord(species == 0, index).Address;
            if (bankAddress == word || bankAddress == word + 1) return true;
        }
        return false;
    }
}

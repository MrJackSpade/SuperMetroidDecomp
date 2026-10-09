namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from the parallel Skree and Metaree programs.</summary>
internal abstract class SkreeMetareeInstructionProgramDefinitions
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

    /// <summary>Resolves one indexed control or timing word from the selected Skree or Metaree program set.</summary>
    /// <param name="metaree"><see langword="true"/> selects Metaree addresses and attack instructions; otherwise Skree is selected.</param>
    /// <param name="index">Zero-based mechanics-word slot from zero through nineteen.</param>
    /// <returns>The bank-$A3 address and value represented by that program slot.</returns>
    /// <exception cref="IndexOutOfRangeException">The slot is outside the twenty compiled words.</exception>
    internal static InstructionMechanicsWord MechanicsWord(bool metaree, int index)
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

    /// <summary>Reads a mechanics word from a compiled Metaree instruction program.</summary>
    /// <param name="address">Bank-$A3 address of the requested word.</param>
    /// <returns>The control or timing value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled Metaree mechanics word.</exception>
    internal static ushort ReadMetareeMechanicsWord(ushort address) => ReadMechanicsWord(true, address);

    /// <summary>Reads a mechanics word from a compiled Skree instruction program.</summary>
    /// <param name="address">Bank-$A3 address of the requested word.</param>
    /// <returns>The control or timing value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled Skree mechanics word.</exception>
    internal static ushort ReadSkreeMechanicsWord(ushort address) => ReadMechanicsWord(false, address);

    /// <summary>Finds the compiled mechanics value associated with an address in the selected program set.</summary>
    /// <param name="metaree"><see langword="true"/> searches Metaree entries; otherwise Skree entries are searched.</param>
    /// <param name="address">Bank-$A3 address to resolve.</param>
    /// <returns>The mechanics value paired with the matching instruction address.</returns>
    /// <exception cref="InvalidDataException">The address is not represented by the selected program set.</exception>
    private static ushort ReadMechanicsWord(bool metaree, ushort address)
    {
        for (int index = 0; index < 20; index++)
        {
            var word = MechanicsWord(metaree, index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException($"{(metaree ? "Metaree" : "Skree")} instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}

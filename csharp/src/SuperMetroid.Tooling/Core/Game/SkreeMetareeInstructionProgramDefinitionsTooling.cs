using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SkreeMetareeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkreeMetareeInstructionProgramDefinitions))]
internal abstract class SkreeMetareeInstructionProgramDefinitionsTooling : IDeclaredProgramBank
{
    /// <summary>Bank $A3, which holds both species' instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => 0xa3;
    // Idle 10, preparation 16/8, dive 2 and stop 1 holds are authored animation cadence (reviewed under #1165).
    /// <summary>Returns the number of timing and control-flow words represented for either species' instruction programs.</summary>
    /// <param name="metaree"><see langword="true"/> selects Metaree; <see langword="false"/> selects Skree.</param>
    internal static int MechanicsWordCount(bool metaree) => 20;

    /// <summary>Returns the number of editable spritemap operands across the selected species' authored instruction lists.</summary>
    /// <param name="metaree"><see langword="true"/> selects Metaree; <see langword="false"/> selects Skree.</param>
    internal static int PresentationWordCount(bool metaree) => 11;

    /// <summary>Maps an ordered presentation slot to its spritemap operand address in the selected species' idle, attack-preparation, dive, or stop list.</summary>
    /// <param name="metaree"><see langword="true"/> selects Metaree; <see langword="false"/> selects Skree.</param>
    /// <param name="index">Zero-based index among the eleven editable presentation operands.</param>
    /// <returns>The bank-$A3 address of the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation operand range.</exception>
    internal static ushort PresentationWordAddress(bool metaree, int index)
    {
        if ((uint)index >= 11) throw new IndexOutOfRangeException();
        if (index < 4) return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareeIdling : SkreeMetareeInstructionProgramDefinitions.SkreeIdling) + 2 + 4 * index);
        if (index < 6) return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareePreparingAttack : SkreeMetareeInstructionProgramDefinitions.SkreePreparingAttack) + 2 + 4 * (index - 4));
        if (index < 10) return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareeDiving : SkreeMetareeInstructionProgramDefinitions.SkreeDiving) + 4 + 4 * (index - 6));
        return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareeStopAnimating : SkreeMetareeInstructionProgramDefinitions.SkreeStopAnimating) + 4);
    }
}

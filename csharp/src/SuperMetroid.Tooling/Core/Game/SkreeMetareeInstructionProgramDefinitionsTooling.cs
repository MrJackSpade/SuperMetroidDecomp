using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SkreeMetareeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SkreeMetareeInstructionProgramDefinitions))]
internal abstract class SkreeMetareeInstructionProgramDefinitionsTooling : IDeclaredProgramBank
{
    /// <summary>Bank $A3, which holds both species' instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => 0xa3;
    // Idle 10, preparation 16/8, dive 2 and stop 1 holds are authored animation cadence (reviewed under #1165).
    internal static int MechanicsWordCount() => 20;
    internal static int PresentationWordCount() => 11;
    internal static ushort PresentationWordAddress(bool metaree, int index)
    {
        if ((uint)index >= 11) throw new IndexOutOfRangeException();
        if (index < 4) return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareeIdling : SkreeMetareeInstructionProgramDefinitions.SkreeIdling) + 2 + 4 * index);
        if (index < 6) return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareePreparingAttack : SkreeMetareeInstructionProgramDefinitions.SkreePreparingAttack) + 2 + 4 * (index - 4));
        if (index < 10) return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareeDiving : SkreeMetareeInstructionProgramDefinitions.SkreeDiving) + 4 + 4 * (index - 6));
        return (ushort)((metaree ? SkreeMetareeInstructionProgramDefinitions.MetareeStopAnimating : SkreeMetareeInstructionProgramDefinitions.SkreeStopAnimating) + 4);
    }
}

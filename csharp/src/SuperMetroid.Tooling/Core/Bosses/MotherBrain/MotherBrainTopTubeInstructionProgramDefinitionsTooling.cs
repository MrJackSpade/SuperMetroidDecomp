using static SuperMetroid.Core.Programs.InstructionItem;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Development-tool members of <see cref="MotherBrainTopTubeInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTopTubeInstructionProgramDefinitions))]
internal abstract class MotherBrainTopTubeInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, IDeclaredProgramBank
{
    /// <summary>Four one-frame top-tube poses, each followed by a sleep command.</summary>
    internal static readonly InstructionProgramLayout Layout = new(MotherBrainTopTubeInstructionProgramToolingDefinitions.Bank,
        Origin(MotherBrainTopTubeInstructionProgramDefinitions.TopRight),
        Entry(MotherBrainTopTubeInstructionProgramDefinitions.TopRight),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep),
        Entry(MotherBrainTopTubeInstructionProgramDefinitions.TopLeft),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep),
        Entry(MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleLeft),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep),
        Entry(MotherBrainTopTubeInstructionProgramDefinitions.TopMiddleRight),
        Frame(1),
        Op((ushort)EnemyProjectileInstruction.Sleep));

    /// <summary>Counts presentation operand words in Mother Brain's top-tube program.</summary>
    public static int PresentationWordCount => Layout.PresentationSlotCount;
    /// <summary>Returns the cartridge address of an indexed top-tube presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => Layout.PresentationSlotAddress(index);
    /// <summary>Identifies the bank containing the top-tube instruction program.</summary>
    static int IDeclaredProgramBank.Bank => MotherBrainTopTubeInstructionProgramToolingDefinitions.Bank;
    /// <summary>Counts top-tube instruction words compiled as mechanics.</summary>
    public static int MechanicsWordCount => Layout.MechanicsWordCount;
    /// <summary>Pairs a compiled top-tube mechanics word with its original bank address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = Layout.MechanicsWord(index);
        return new(address, value);
    }
}

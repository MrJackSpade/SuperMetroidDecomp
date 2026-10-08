using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BotwoonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BotwoonProjectileInstructionProgramDefinitions))]
internal abstract class BotwoonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => BotwoonProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => BotwoonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 73;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int address;
        if (index < 48)
        {
            int command = index % 6;
            address = BotwoonProjectileInstructionProgramDefinitions.BodyProgram(index / 6) + (command < 5 ? 4 * command : 18);
        }
        else if (index < 66)
            address = BotwoonProjectileInstructionProgramDefinitions.BodyProgram(8 + (index - 48) / 2) + 4 * ((index - 48) % 2);
        else
        {
            int command = index - 66;
            address = BotwoonProjectileInstructionProgramDefinitions.Spit + (command < 6 ? 4 * command : 22);
        }
        return new((ushort)address, BotwoonProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort word = (ushort)(address & ~1);
        // Body/tail programs start at odd addresses; spit starts even.
        ushort bodyWord = unchecked((ushort)(((ushort)address - BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft & ~1) + BotwoonProjectileInstructionProgramDefinitions.BodyUpLeft));
        if (BotwoonProjectileInstructionProgramDefinitions.TryBodyOffset(bodyWord, out int offset))
            return offset >= 16 || offset % 4 == 0;
        int sleeping = bodyWord - BotwoonProjectileInstructionProgramDefinitions.TailUpFacingRight;
        if ((uint)sleeping < 54) return sleeping % 6 != 2;
        int spit = word - BotwoonProjectileInstructionProgramDefinitions.Spit;
        return (uint)spit < 24 && (spit >= 20 || spit % 4 == 0);
    }
}

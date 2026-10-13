using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EnemyProjectileInstructionMechanicsDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EnemyProjectileInstructionMechanicsDefinitions))]
internal abstract class EnemyProjectileInstructionMechanicsDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts the blue-ring words and each timed projectile's prefix, frames, and terminal words.</summary>
    public static int MechanicsWordCount
    {
        get
        {
            int count = 38;
            for (int index = 0; index < EnemyProjectileInstructionMechanicsDefinitions.TimedProgramCount; index++)
            {
                var program = EnemyProjectileInstructionMechanicsDefinitions.TimedProgram(index);
                count += program.FrameCount + 1 + (program.PrefixInstruction.HasValue ? 1 : 0)
                    + (program.TerminalOperand.HasValue ? 1 : 0);
            }
            return count;
        }
    }
    /// <summary>Recognizes either byte of a compiled bank-$86 projectile mechanics word.</summary>
    public static bool IsCompiledMechanicsByte(int address) => (SnesAddressMath.BankBase(address)) == 0x860000 &&
        (EnemyProjectileInstructionMechanicsDefinitions.TryReadMechanicsWord((ushort)address, out _) || EnemyProjectileInstructionMechanicsDefinitions.TryReadMechanicsWord(unchecked((ushort)(address - 1)), out _));
    /// <summary>Reads the indexed word from the blue-ring and timed-projectile address sequence.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        foreach (ushort address in MechanicsAddresses())
            if (index-- == 0) return new(address, EnemyProjectileInstructionMechanicsDefinitions.ReadMechanicsWord(address));
        throw new InvalidOperationException("Projectile mechanics enumeration count disagrees with its programs.");
    }
    /// <summary>
    /// Visits Mother Brain's blue ring and drool special layouts, then each timed program's
    /// optional prefix, frame durations, terminal instruction, and optional operand.
    /// </summary>
    internal static IEnumerable<ushort> MechanicsAddresses()
    {
        for (int frame = 0; frame < EnemyProjectileInstructionMechanicsDefinitions.BlueRingRadiusCount; frame++)
            for (int field = 0; field < 3; field++)
                yield return (ushort)(EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingInitial + frame * 8 + field * 2);
        yield return EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingInitial + 48;
        yield return EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch;
        yield return EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch + 2;
        for (int frame = 0; frame < 6; frame++) yield return (ushort)(EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch + 4 + frame * 4);
        yield return EnemyProjectileInstructionMechanicsDefinitions.MotherBrainBlueRingTouch + 28;
        for (int index = 0; index < EnemyProjectileInstructionMechanicsDefinitions.TimedProgramCount; index++)
        {
            var program = EnemyProjectileInstructionMechanicsDefinitions.TimedProgram(index);
            if (program.InitialPointer == EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolFalling)
            {
                for (int frame = 0; frame < 5; frame++) yield return (ushort)(EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial + frame * 4);
                for (int field = 0; field < 4; field++) yield return (ushort)(EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial + 20 + field * 2);
                yield return EnemyProjectileInstructionMechanicsDefinitions.MotherBrainDroolInitial + 30;
            }
            int first = program.InitialPointer;
            if (program.PrefixInstruction.HasValue) { yield return (ushort)first; first += 2; }
            for (int frame = 0; frame < program.FrameCount; frame++) yield return (ushort)(first + frame * 4);
            yield return (ushort)(first + program.FrameCount * 4);
            if (program.TerminalOperand.HasValue) yield return (ushort)(first + program.FrameCount * 4 + 2);
        }
    }
}

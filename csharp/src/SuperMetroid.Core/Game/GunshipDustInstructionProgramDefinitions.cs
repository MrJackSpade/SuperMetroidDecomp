namespace SuperMetroid.Core.Game;

/// <summary>One of the six authored gunship liftoff-dust animation programs.</summary>
internal readonly record struct GunshipDustInstructionProgramDefinition(
    ushort Initial,
    ushort FirstFrame,
    ushort Terminal,
    GunshipDustDurations Durations);

/// <summary>Each dust pair adds one tick per two poses, reaching its terminal hold after four poses.</summary>
internal readonly record struct GunshipDustDurations(int Length, ushort Initial)
{
    internal ushort this[int frame] => (uint)frame < Length
        ? (ushort)(Initial + Math.Min(frame / 2, 2)) : throw new IndexOutOfRangeException();
}

/// <summary>
/// Compiled control for all six gunship liftoff-dust instruction lists. Interleaved
/// spritemap operands resolve through extracted presentation art.
/// </summary>
internal abstract class GunshipDustInstructionProgramDefinitions
{
    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index0_0</c> at $86:A197.</summary>
    internal const ushort Index0 = 0xa197;

    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index2_0</c> at $86:A1C1.</summary>
    internal const ushort Index2 = 0xa1c1;

    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index4_0</c> at $86:A1EB.</summary>
    internal const ushort Index4 = 0xa1eb;

    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index6_0</c> at $86:A211.</summary>
    internal const ushort Index6 = 0xa211;

    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_Index8_0</c> at $86:A23B.</summary>
    internal const ushort Index8 = 0xa23b;

    /// <summary><c>InstList_EnemyProjectile_GunshipLiftoffDustClouds_IndexA_0</c> at $86:A265.</summary>
    internal const ushort IndexA = 0xa265;

    internal static int ProgramCount => 6;
    public static int MechanicsWordCount => 76;
    public static int PresentationWordCount => 46;

    internal static GunshipDustInstructionProgramDefinition Program(int index)
    {
        if ((uint)index >= ProgramCount) throw new IndexOutOfRangeException();
        int shape = index % 3;
        int count = shape == 2 ? 7 : 8;
        ushort initialDuration = shape switch { 0 => 8, 1 => 6, _ => 11 };
        ushort initial = (ushort)(Index0 + 42 * index - 4 * (index / 3));
        return new(initial, (ushort)(initial + 4), (ushort)(initial + 4 + 4 * count),
            new(count, initialDuration));
    }

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        for (int programIndex = 0; programIndex < ProgramCount; programIndex++)
        {
            GunshipDustInstructionProgramDefinition program = Program(programIndex);
            int count = program.Durations.Length + 5;
            if (index >= count)
            {
                index -= count;
                continue;
            }

            if (index == 0)
            {
                return new(program.Initial,
                    EnemyProjectileCodePointers.Instruction_EnemyProjectile_TimerInY);
            }
            if (index == 1)
                return new(unchecked((ushort)(program.Initial + 2)), 1);
            index -= 2;
            if (index < program.Durations.Length)
            {
                return new(unchecked((ushort)(program.FirstFrame + index * 4)),
                    program.Durations[index]);
            }
            index -= program.Durations.Length;
            return index switch
            {
                0 => new(program.Terminal,
                    EnemyProjectileCodePointers
                        .Instruction_EnemyProjectile_DecrementTimer_GotoYIfNonZero),
                1 => new(unchecked((ushort)(program.Terminal + 2)), program.FirstFrame),
                2 => new(unchecked((ushort)(program.Terminal + 4)),
                    EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
                _ => throw new InvalidOperationException(
                    "Gunship dust mechanics index escaped its program bounds."),
            };
        }

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    public static ushort PresentationWordAddress(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        for (int programIndex = 0; programIndex < ProgramCount; programIndex++)
        {
            GunshipDustInstructionProgramDefinition program = Program(programIndex);
            if (index >= program.Durations.Length)
            {
                index -= program.Durations.Length;
                continue;
            }
            return unchecked((ushort)(program.FirstFrame + index * 4 + 2));
        }
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            InstructionMechanicsWord candidate = MechanicsWord(index);
            if (candidate.Address == address)
                return candidate.Value;
        }
        throw new InvalidDataException(
            $"Gunship dust mechanics pointer $86:{address:X4} is not compiled.");
    }
}

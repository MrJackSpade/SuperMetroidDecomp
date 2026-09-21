namespace SuperMetroid.Core.Game;

/// <summary>One compiled gunship-dust mechanics word at its bank-$86 address.</summary>
internal readonly record struct GunshipDustInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>One of the six authored gunship liftoff-dust animation programs.</summary>
internal readonly record struct GunshipDustInstructionProgramDefinition(
    ushort Initial,
    ushort FirstFrame,
    ushort Terminal,
    ushort[] Durations);

/// <summary>
/// Compiled control for all six gunship liftoff-dust instruction lists. Interleaved
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class GunshipDustInstructionProgramDefinitions
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

    private static readonly GunshipDustInstructionProgramDefinition[] Programs =
    [
        new(Index0, 0xa19b, 0xa1bb, [8, 8, 9, 9, 10, 10, 10, 10]),
        new(Index2, 0xa1c5, 0xa1e5, [6, 6, 7, 7, 8, 8, 8, 8]),
        new(Index4, 0xa1ef, 0xa20b, [11, 11, 12, 12, 13, 13, 13]),
        new(Index6, 0xa215, 0xa235, [8, 8, 9, 9, 10, 10, 10, 10]),
        new(Index8, 0xa23f, 0xa25f, [6, 6, 7, 7, 8, 8, 8, 8]),
        new(IndexA, 0xa269, 0xa285, [11, 11, 12, 12, 13, 13, 13]),
    ];

    internal static int ProgramCount => Programs.Length;
    internal static int MechanicsWordCount => 76;
    internal static int PresentationWordCount => 46;
    internal static GunshipDustInstructionProgramDefinition Program(int index) => Programs[index];

    internal static ushort InitialForParameter(ushort parameter) => parameter switch
    {
        0 => Index0,
        2 => Index2,
        4 => Index4,
        6 => Index6,
        8 => Index8,
        10 => IndexA,
        _ => throw new ArgumentOutOfRangeException(
            nameof(parameter), parameter, "Gunship dust parameter must be 0,2,4,6,8,A."),
    };

    internal static GunshipDustInstructionMechanicsWord MechanicsWord(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        for (int programIndex = 0; programIndex < Programs.Length; programIndex++)
        {
            GunshipDustInstructionProgramDefinition program = Programs[programIndex];
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

    internal static ushort PresentationWordAddress(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        for (int programIndex = 0; programIndex < Programs.Length; programIndex++)
        {
            GunshipDustInstructionProgramDefinition program = Programs[programIndex];
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
            GunshipDustInstructionMechanicsWord candidate = MechanicsWord(index);
            if (candidate.Address == address)
                return candidate.Value;
        }
        throw new InvalidDataException(
            $"Gunship dust mechanics pointer $86:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled mechanics and callback metadata for Mother Brain's recursive red hand-beam
/// projectile program in bank $86. Interleaved spritemap operands remain presentation data.
/// </summary>
internal static class MotherBrainHandBeamInstructionProgramDefinitions
{
    /// <summary><c>$86:C796</c>, shared charging/fired hand-beam program.</summary>
    internal const ushort Initial = 0xc796;

    /// <summary><c>$86:C7B7</c>, second recursive hand-beam emission stage.</summary>
    private const ushort SecondStage = 0xc7b7;

    /// <summary><c>$86:C7D8</c>, third recursive hand-beam emission stage.</summary>
    private const ushort ThirdStage = 0xc7d8;

    /// <summary><c>$86:C7F9</c>, terminal delete after all three emission stages.</summary>
    private const ushort TerminalDelete = 0xc7f9;

    /// <summary><c>$86:C7FB</c>, callback that emits the next fired hand-beam actor.</summary>
    internal const int SpawnNextCallback = 0x86c7fb;

    private static readonly ushort[] StageStarts = [Initial, SecondStage, ThirdStage];
    private static readonly ushort[] Durations = [3, 3, 2, 2, 1, 1, 1];
    private static readonly EnemyProjectileMechanicsWordDefinition[] MechanicsWords =
        BuildMechanicsWords();
    private static readonly ushort[] ExternalCallInstructions = BuildExternalCallInstructions();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    /// <summary>Number of compiled duration, opcode, and terminal words.</summary>
    internal static int NativeWordCount => MechanicsWords.Length;

    /// <summary>Number of three-byte external-callback operands in the program.</summary>
    internal static int ExternalCallCount => ExternalCallInstructions.Length;

    /// <summary>Number of interleaved spritemap words retained as presentation reads.</summary>
    internal static int PresentationWordCount => PresentationWords.Length;

    /// <summary>Returns whether this projectile kind owns the recursive hand-beam program.</summary>
    internal static bool Owns(RoomEnemyProjectileKind kind) => kind is
        RoomEnemyProjectileKind.MotherBrainHandBeamCharging or
        RoomEnemyProjectileKind.MotherBrainHandBeamFired;

    /// <summary>Reads one compiled duration, opcode, or terminal word.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWords.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            EnemyProjectileMechanicsWordDefinition candidate = MechanicsWords[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Mother Brain hand-beam mechanics pointer ${address:X4} is outside its translated program.");
    }

    /// <summary>
    /// Resolves the native 24-bit callback following one of the program's three external-call
    /// opcodes. This metadata is executable identity, not editable projectile presentation.
    /// </summary>
    internal static int ReadExternalFunction(ushort instructionAddress)
    {
        for (int index = 0; index < ExternalCallInstructions.Length; index++)
        {
            if (ExternalCallInstructions[index] == instructionAddress)
                return SpawnNextCallback;
        }

        throw new InvalidDataException(
            $"Mother Brain hand-beam external call ${instructionAddress:X4} is not translated.");
    }

    /// <summary>Returns one compiled address/value pair for cartridge comparison.</summary>
    internal static EnemyProjectileMechanicsWordDefinition NativeWord(int index)
    {
        if ((uint)index >= MechanicsWords.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return MechanicsWords[index];
    }

    /// <summary>Returns one external-call instruction address for cartridge comparison.</summary>
    internal static ushort ExternalCallInstruction(int index)
    {
        if ((uint)index >= ExternalCallInstructions.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return ExternalCallInstructions[index];
    }

    /// <summary>Tests whether one bank-$86 byte is compiled mechanics or callback metadata.</summary>
    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWords.Length; index++)
        {
            ushort start = MechanicsWords[index].Address;
            if (bankAddress == start || bankAddress == unchecked((ushort)(start + 1)))
                return true;
        }

        for (int index = 0; index < ExternalCallInstructions.Length; index++)
        {
            ushort operand = unchecked((ushort)(ExternalCallInstructions[index] + 2));
            if (bankAddress >= operand && bankAddress <= unchecked((ushort)(operand + 2)))
                return true;
        }

        return false;
    }

    /// <summary>Tests whether one bank-$86 byte belongs to an interleaved spritemap word.</summary>
    internal static bool IsPresentationByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PresentationWords.Length; index++)
        {
            ushort start = PresentationWords[index];
            if (bankAddress == start || bankAddress == unchecked((ushort)(start + 1)))
                return true;
        }

        return false;
    }

    private static EnemyProjectileMechanicsWordDefinition[] BuildMechanicsWords()
    {
        var words = new List<EnemyProjectileMechanicsWordDefinition>();
        foreach (ushort stageStart in StageStarts)
        {
            ushort pointer = stageStart;
            words.Add(new(pointer, Durations[0]));
            pointer = unchecked((ushort)(pointer + 4));
            words.Add(new(pointer,
                EnemyProjectileCodePointers.Instruction_EnemyProjectile_CallExternalFunctionInY));
            pointer = unchecked((ushort)(pointer + 5));
            for (int frame = 1; frame < Durations.Length; frame++)
            {
                words.Add(new(pointer, Durations[frame]));
                pointer = unchecked((ushort)(pointer + 4));
            }
        }
        words.Add(new(TerminalDelete,
            EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete));
        words.Sort(static (left, right) => left.Address.CompareTo(right.Address));
        return [.. words];
    }

    private static ushort[] BuildExternalCallInstructions()
    {
        var instructions = new ushort[StageStarts.Length];
        for (int index = 0; index < instructions.Length; index++)
            instructions[index] = unchecked((ushort)(StageStarts[index] + 4));
        return instructions;
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new ushort[StageStarts.Length * Durations.Length];
        int destination = 0;
        foreach (ushort stageStart in StageStarts)
        {
            ushort pointer = stageStart;
            words[destination++] = unchecked((ushort)(pointer + 2));
            pointer = unchecked((ushort)(pointer + 9));
            for (int frame = 1; frame < Durations.Length; frame++)
            {
                words[destination++] = unchecked((ushort)(pointer + 2));
                pointer = unchecked((ushort)(pointer + 4));
            }
        }
        return words;
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control words for the escape-sequence Etecoon's low/high-tide walking,
/// waiting, gratitude, and departure programs. Spritemap selections resolve installed artwork.
/// </summary>
internal abstract class EscapeEtecoonInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_EtecoonEscape_RunningLeft_LowTide_0</c> at $B3:E556.</summary>
    internal const ushort RunningLeftLowTide = 0xe556;
    /// <summary><c>InstList_EtecoonEscape_RunningLeft_HighTide</c> at $B3:E56E.</summary>
    internal const ushort RunningLeftHighTide = 0xe56e;
    /// <summary><c>InstList_EtecoonEscape_RunningRight_LowTide_0</c> at $B3:E582.</summary>
    internal const ushort RunningRightLowTide = 0xe582;
    /// <summary><c>InstList_EtecoonEscape_RunningRight_HighTide</c> at $B3:E59A.</summary>
    internal const ushort RunningRightHighTide = 0xe59a;
    /// <summary><c>InstList_EtecoonEscape_RunningForEscape_0</c> at $B3:E5AE.</summary>
    internal const ushort RunningForEscape = 0xe5ae;
    /// <summary><c>InstList_EtecoonEscape_Stationary</c> at $B3:E5C6.</summary>
    internal const ushort Stationary = 0xe5c6;
    /// <summary><c>InstList_EtecoonEscape_ExpressGratitudeThenEscape_0</c> at $B3:E5DA.</summary>
    internal const ushort ExpressGratitudeThenEscape = 0xe5da;

    public static int MechanicsWordCount => 63;
    public static int PresentationWordCount => 30;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 28)
        {
            bool right = index >= 14;
            int local = index % 14;
            ushort low = right ? RunningRightLowTide : RunningLeftLowTide;
            ushort high = right ? RunningRightHighTide : RunningLeftHighTide;
            if (local == 0)
                return new(low, EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_GotoY_IfAcidPositionLessThanCE);
            if (local == 1)
                return new((ushort)(low + 2), high);
            return local < 8
                ? FourPoseLoop((ushort)(low + 4), local - 2, right ? (ushort)6 : (ushort)5)
                : FourPoseLoop(high, local - 8, 3);
        }
        if (index < 36)
        {
            int local = index - 28;
            if (local == 0)
                return new(RunningForEscape, EscapeAnimalInstructionCodes.Instruction_CommonB3_Enemy0FB2_InY);
            if (local == 1)
                return new((ushort)(RunningForEscape + 2), (ushort)EscapeEtecoonPreInstruction.EscapeRight);
            return FourPoseLoop((ushort)(RunningForEscape + 4), local - 2, 3);
        }
        if (index < 42)
        {
            int local = index - 36;
            return FourPoseLoop(Stationary, local, (ushort)((local & 1) == 0 ? 64 : 8));
        }
        int gratitude = index - 42;
        if (gratitude < 3)
            return new((ushort)(ExpressGratitudeThenEscape + 2 * gratitude), gratitude switch
            {
                0 => EscapeAnimalInstructionCodes.Instruction_CommonB3_SetEnemy0FB2ToRTS,
                1 => CommonEnemyInstructionCodes.SetTimer,
                _ => 8,
            });
        if (gratitude < 15)
        {
            int record = (gratitude - 3) / 3;
            int field = (gratitude - 3) % 3;
            return new((ushort)(ExpressGratitudeThenEscape + 6 + 8 * record + (field == 0 ? 0 : 2 + 2 * field)),
                field switch
                {
                    0 => 8,
                    1 => EscapeAnimalInstructionCodes.Instruction_EtecoonEscape_XPositionPlusY,
                    _ => unchecked((ushort)-3),
                });
        }
        int finish = gratitude - 15;
        int offset = finish < 3 ? 38 + 2 * finish : finish == 3 ? 46 : 42 + 2 * finish;
        ushort value = finish switch
        {
            0 => CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate,
            1 => (ushort)(ExpressGratitudeThenEscape + 6),
            2 => 64,
            3 => 8,
            4 => CommonEnemyInstructionCodes.Goto,
            _ => RunningForEscape,
        };
        return new((ushort)(ExpressGratitudeThenEscape + offset), value);
    }

    private static InstructionMechanicsWord FourPoseLoop(ushort start, int word, ushort duration) =>
        new((ushort)(start + (word < 5 ? 4 * word : 18)),
            word < 4 ? duration : word == 4 ? CommonEnemyInstructionCodes.Goto : start);

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        if (index < 16)
        {
            ushort low = index < 8 ? RunningLeftLowTide : RunningRightLowTide;
            int local = index % 8;
            return (ushort)(low + (local < 4 ? 6 + 4 * local : 26 + 4 * (local - 4)));
        }
        if (index < 20)
            return (ushort)(RunningForEscape + 6 + 4 * (index - 16));
        if (index < 24)
            return (ushort)(Stationary + 2 + 4 * (index - 20));
        return (ushort)(ExpressGratitudeThenEscape + (index < 28 ? 8 + 8 * (index - 24) : 44 + 4 * (index - 28)));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = MechanicsWord(index);
            if (word.Address == address)
                return word.Value;
        }
        throw new InvalidDataException(
            $"Escape Etecoon instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == wordAddress + 1)
                return true;
        }
        return false;
    }
}
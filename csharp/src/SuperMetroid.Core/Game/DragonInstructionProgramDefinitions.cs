namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Dragon's body, wing, and attack programs.</summary>
/// <remarks>
/// Control words derive from the idle, wing and attack layouts; sixteen interleaved
/// spritemap operands belong to the compiled presentation definitions.
/// </remarks>
internal abstract class DragonInstructionProgramDefinitions
{
    /// <summary><c>$A2:E59B</c>, sleeping body facing left.</summary>
    internal const ushort IdleFacingLeft = 0xe59b;

    /// <summary><c>$A2:E5AD</c>, sleeping body facing right.</summary>
    internal const ushort IdleFacingRight = 0xe5ad;

    /// <summary><c>$A2:E5A1</c>, cosmetic wing loop facing left.</summary>
    internal const ushort WingsFacingLeft = 0xe5a1;

    /// <summary><c>$A2:E5B3</c>, cosmetic wing loop facing right.</summary>
    internal const ushort WingsFacingRight = 0xe5b3;

    /// <summary><c>$A2:E5BF</c>, five-frame attack body sequence facing left.</summary>
    internal const ushort AttackingFacingLeft = 0xe5bf;

    /// <summary><c>$A2:E5D7</c>, five-frame attack body sequence facing right.</summary>
    internal const ushort AttackingFacingRight = 0xe5d7;

    public static int MechanicsWordCount => 26;
    public static int PresentationWordCount => 16;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 12)
        {
            int side = index / 6;
            int local = index % 6;
            ushort idle = side == 0 ? IdleFacingLeft : IdleFacingRight;
            ushort wings = side == 0 ? WingsFacingLeft : WingsFacingRight;
            if (local < 2)
                return new((ushort)(idle + 4 * local), local == 0 ? (ushort)1 : (ushort)CommonEnemyInstruction.Sleep);
            int wingWord = local - 2;
            return new((ushort)(wings + (wingWord < 3 ? 4 * wingWord : 10)),
                wingWord < 2 ? (ushort)5 : wingWord == 2 ? (ushort)CommonEnemyInstruction.Goto : wings);
        }
        int attackWord = (index - 12) % 7;
        ushort attack = index < 19 ? AttackingFacingLeft : AttackingFacingRight;
        ushort value = attackWord switch
        {
            0 => 32, // Hold the initial attack pose before extension.
            1 or 3 => 3, // Symmetric extension/retraction poses.
            2 => 7, // Fully extended attack.
            4 => 1, // Return to the initial pose before completion.
            5 => (ushort)DragonInstruction.AttackFinishedCallback,
            _ => (ushort)CommonEnemyInstruction.Sleep,
        };
        return new((ushort)(attack + (attackWord < 6 ? 4 * attackWord : 22)), value);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        if (index < 6)
        {
            ushort idle = index < 3 ? IdleFacingLeft : IdleFacingRight;
            int local = index % 3;
            return (ushort)(idle + (local == 0 ? 2 : 4 + 4 * local));
        }
        ushort attack = index < 11 ? AttackingFacingLeft : AttackingFacingRight;
        return (ushort)(attack + 2 + 4 * ((index - 6) % 5));
    }

    /// <summary>Returns Dragon control derived from the six program layouts.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = MechanicsWord(index);
            if (word.Address == address)
                return word.Value;
        }
        throw new InvalidDataException(
            $"Dragon instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}
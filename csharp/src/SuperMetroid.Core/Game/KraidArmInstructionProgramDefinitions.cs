namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled timing and control for Kraid's independently scheduled arm actor. The
/// interleaved extended-spritemap selectors are compiled presentation data;
/// their selected OAM frames live in the installed enemy-art catalog.
/// </summary>
internal abstract class KraidArmInstructionProgramDefinitions
{
    /// <summary><c>InstList_KraidArm_Normal_0</c> at $A7:89F3.</summary>
    internal const ushort Normal = 0x89f3;
    /// <summary><c>InstList_KraidArm_Normal_1</c> at $A7:8A37.</summary>
    internal const ushort NormalPause = 0x8a37;
    /// <summary><c>InstList_KraidArm_Slow</c> at $A7:8A41.</summary>
    internal const ushort Slow = 0x8a41;
    /// <summary><c>InstList_KraidArm_RisingSinking</c> at $A7:8AA4.</summary>
    internal const ushort RisingOrSinking = 0x8aa4;
    /// <summary><c>InstList_KraidArm_Dying_PreparingToLungeForward</c> at $A7:8AF0.</summary>
    internal const ushort DyingOrPreparingToLunge = 0x8af0;

    /// <summary>Number of indexed mechanics words across the normal, slow, rising/sinking, and dying/lunge lists.</summary>
    public static int MechanicsWordCount => 2 * 21 + 20 + 4;
    /// <summary>Number of spritemap operands selected by the three 18-frame lists and the final 3-frame list.</summary>
    public static int PresentationWordCount => 3 * 18 + 3;

    /// <summary>Normal and slow loops each have eighteen frames, a health
    /// callback and a two-word goto. Rising/sinking omits the callback; the
    /// final program has three frames and Sleep. Frame pairs are four bytes,
    /// while control words are two bytes. The intervening native callback
    /// body is outside the instruction-list domain.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < 42)
        {
            bool slow = index >= 21;
            ushort entry = slow ? Slow : Normal;
            int word = index % 21;
            if (word < 18)
                return new((ushort)(entry + 4 * word), (ushort)(word == 17 ? (slow ? 0x30 : 0x20) : (slow ? 8 : 6)));
            ushort value = word switch
            {
                18 => EnemyInstructionCodePointers.Instruction_KraidArm_SlowArmIfLessThanHalfHealth,
                19 => CommonEnemyInstructionCodes.Goto,
                _ => entry,
            };
            return new((ushort)(entry + 18 * 4 + 2 * (word - 18)), value);
        }
        if (index < 62)
        {
            int word = index - 42;
            return word < 18
                ? new((ushort)(RisingOrSinking + 4 * word), (ushort)(word == 17 ? 0x20 : 6))
                : new((ushort)(RisingOrSinking + 18 * 4 + 2 * (word - 18)),
                    word == 18 ? CommonEnemyInstructionCodes.Goto : RisingOrSinking);
        }
        int frame = index - 62;
        return new((ushort)(DyingOrPreparingToLunge + 4 * frame), frame switch
        {
            < 2 => 6,
            2 => 0x7fff,
            _ => CommonEnemyInstructionCodes.Sleep,
        });
    }

    /// <summary>Three eighteen-frame lists and the final three-frame list;
    /// every presentation operand is two bytes after its duration word.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        ushort entry = index < 18 ? Normal : index < 36 ? Slow : index < 54 ? RisingOrSinking : DyingOrPreparingToLunge;
        int frame = index < 54 ? index % 18 : index - 54;
        return (ushort)(entry + 4 * frame + 2);
    }

    /// <summary>Returns fixed Kraid-arm control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException($"Kraid arm mechanics pointer $A7:{address:X4} is not compiled.");
    }
}

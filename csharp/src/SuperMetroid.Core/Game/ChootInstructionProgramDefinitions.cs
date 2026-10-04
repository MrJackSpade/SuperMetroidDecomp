namespace SuperMetroid.Core.Game;

internal readonly record struct ChootInstructionMechanicsWord(ushort Address, ushort Value);

/// <summary>Control flow for Choot idle, jump and fall animations; visual operands remain independently supplied.</summary>
internal static class ChootInstructionProgramDefinitions
{
    /// <summary>$A2:D82C: disable off-screen processing, display one frame, then sleep.</summary>
    internal const ushort Idle = 0xd82c;
    /// <summary>$A2:D834: enable off-screen processing, display eight ticks then one, then sleep.</summary>
    internal const ushort Jumping = 0xd834;
    /// <summary>$A2:D840: jump control timing with a different final visual operand.</summary>
    internal const ushort Falling = 0xd840;

    internal static int MechanicsWordCount => 11;
    internal static int PresentationWordCount => 5;

    internal static ChootInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort address;
        if (index < 3)
            address = (ushort)(Idle + (index == 2 ? 6 : index * 2));
        else
        {
            int frame = (index - 3) % 4;
            int program = (index - 3) / 4;
            address = (ushort)(Jumping + (Falling - Jumping) * program + (frame == 0 ? 0 : frame * 4 - 2));
        }
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return index == 0 ? (ushort)(Idle + 4) :
            (ushort)(Jumping + (Falling - Jumping) * ((index - 1) / 2) + 4 + 4 * ((index - 1) % 2));
    }

    internal static bool IsPresentationWord(ushort address)
    {
        if (address == Idle + 4)
            return true;
        int offset = address - Jumping - 4;
        return offset >= 0 && offset < Falling - Jumping + 8 &&
            offset % (Falling - Jumping) is 0 or 4;
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Choot instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return TryRead(bankAddress, out _) || TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }

    private static bool TryRead(ushort address, out ushort value)
    {
        if (address < Jumping)
        {
            value = address switch
            {
                Idle => CommonEnemyInstructionCodes.DisableOffScreenProcessing,
                Idle + 2 => 1,
                Idle + 6 => CommonEnemyInstructionCodes.Sleep,
                _ => 0,
            };
            return value != 0;
        }
        int offset = address - Jumping;
        if (offset >= 2 * (Falling - Jumping))
        {
            value = 0;
            return false;
        }
        value = (offset % (Falling - Jumping)) switch
        {
            0 => CommonEnemyInstructionCodes.EnableOffScreenProcessing,
            2 => 8,
            6 => 1,
            10 => CommonEnemyInstructionCodes.Sleep,
            _ => 0,
        };
        return value != 0;
    }
}
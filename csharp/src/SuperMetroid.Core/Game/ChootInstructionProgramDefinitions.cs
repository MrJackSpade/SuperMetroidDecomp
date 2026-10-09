namespace SuperMetroid.Core.Game;

/// <summary>Control flow for Choot idle, jump and fall animations; visual operands remain independently supplied.</summary>
internal abstract class ChootInstructionProgramDefinitions
{
    /// <summary>$A2:D82C: disable off-screen processing, display one frame, then sleep.</summary>
    internal const ushort Idle = 0xd82c;
    /// <summary>$A2:D834: enable off-screen processing, display eight ticks then one, then sleep.</summary>
    internal const ushort Jumping = 0xd834;
    /// <summary>$A2:D840: jump control timing with a different final visual operand.</summary>
    internal const ushort Falling = 0xd840;

    /// <summary>Recognizes the addresses in the idle, jump, and fall programs that carry visual frame operands.</summary>
    /// <param name="address">The bank-$A2 instruction address to classify.</param>
    /// <returns><see langword="true"/> when the address selects a separately supplied presentation frame.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        if (address == Idle + 4)
            return true;
        int offset = address - Jumping - 4;
        return offset >= 0 && offset < Falling - Jumping + 8 &&
            offset % (Falling - Jumping) is 0 or 4;
    }

    /// <summary>Reads one compiled control or timing word from Choot's instruction programs.</summary>
    /// <param name="address">The bank-$A2 address of the mechanics word.</param>
    /// <returns>The engine-control code or timing value stored at that address.</returns>
    /// <exception cref="InvalidDataException">The address is not a compiled mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (TryRead(address, out ushort value))
            return value;
        throw new InvalidDataException(
            $"Choot instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    /// <summary>Attempts to resolve a Choot mechanics word, leaving visual operands unclaimed.</summary>
    /// <param name="address">The bank-$A2 address to inspect.</param>
    /// <param name="value">Receives the compiled control or timing word, or zero when the address is unrecognized.</param>
    /// <returns><see langword="true"/> when the address contains a compiled mechanics word.</returns>
    internal static bool TryRead(ushort address, out ushort value)
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

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for plain vertical shutters and Kamer platforms.
/// Their interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class VerticalShutterInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    internal const ushort Plain = 0xe9aa;

    /// <summary><c>InstructionList_KamerPlatform</c> at $A2:EDE7.</summary>
    internal const ushort KamerPlatform = 0xede7;

    /// <summary>True only for the plain vertical shutter, not the Kamer platform loop.</summary>
    internal static bool IsPlainShutterPresentationWord(ushort address) => address == Plain + 2;

    /// <summary>Tests whether a bank-local address is one of the four spritemap operands in the Kamer platform loop.</summary>
    /// <param name="address">Candidate presentation-word address in the compiled bank-$A2 program layout.</param>
    /// <returns><see langword="true"/> for a Kamer frame selector, excluding its mechanics words.</returns>
    internal static bool IsKamerPresentationWord(ushort address)
    {
        int offset = address - (KamerPlatform + 2);
        return (uint)offset < 16 && offset % 4 == 0;
    }

    /// <summary>Reads a shutter or Kamer loop mechanics word while rejecting presentation slots and uncompiled addresses.</summary>
    /// <param name="address">Bank-local address of the candidate instruction word.</param>
    /// <returns>The native frame duration, sleep opcode, loop opcode, or loop target compiled at that address.</returns>
    /// <exception cref="InvalidDataException">The address is outside the compiled mechanics words.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Plain) return 1;
        if (address == Plain + 4) return CommonEnemyInstructionCodes.Sleep;
        int offset = address - KamerPlatform;
        if ((uint)offset < 16 && offset % 4 == 0) return 10;
        if (offset == 16) return CommonEnemyInstructionCodes.Goto;
        if (offset == 18) return KamerPlatform;
        throw new InvalidDataException(
            $"Vertical-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

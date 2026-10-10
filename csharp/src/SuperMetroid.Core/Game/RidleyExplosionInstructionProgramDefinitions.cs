namespace SuperMetroid.Core.Game;

/// <summary>Native $A6:CA47..CAEF breakup programs: one frame followed by sleep.</summary>
internal abstract class RidleyExplosionInstructionProgramDefinitions
{
    /// <summary>$A6:CA47, the first Ridley explosion instruction list (small tail segment).</summary>
    internal const ushort First = 0xca47;
    /// <summary>The 29 native six-byte lists ending at $A6:CAEF.</summary>
    internal const int ProgramCount = 29;

    /// <summary>Returns the compiled duration or sleep mechanics word within a Ridley breakup instruction list.</summary>
    /// <param name="address">Bank-local instruction address in one of the 29 six-byte lists.</param>
    /// <returns>One at each list's duration word or the shared sleep opcode at its sleep word.</returns>
    /// <exception cref="InvalidDataException">The address is outside the compiled lists or does not identify a mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - First;
        if (offset >= 0 && offset < ProgramCount * 6)
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return CommonEnemyInstructionCodes.Sleep;
        }
        throw new InvalidDataException($"Ridley breakup mechanics pointer $A6:{address:X4} is not compiled.");
    }
}

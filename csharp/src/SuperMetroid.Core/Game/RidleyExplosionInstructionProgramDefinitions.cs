namespace SuperMetroid.Core.Game;

/// <summary>Native $A6:CA47..CAEF breakup programs: one frame followed by sleep.</summary>
internal abstract class RidleyExplosionInstructionProgramDefinitions
{
    /// <summary>$A6:CA47, the first Ridley explosion instruction list (small tail segment).</summary>
    internal const ushort First = 0xca47;
    /// <summary>The 29 native six-byte lists ending at $A6:CAEF.</summary>
    internal const int ProgramCount = 29;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - First;
        if (offset is >= 0 and < (ProgramCount * 6))
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return (ushort)CommonEnemyInstruction.Sleep;
        }
        throw new InvalidDataException($"Ridley breakup mechanics pointer $A6:{address:X4} is not compiled.");
    }
}

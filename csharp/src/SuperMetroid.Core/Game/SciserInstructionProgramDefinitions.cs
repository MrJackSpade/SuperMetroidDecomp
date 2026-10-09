namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Sciser's four surface loops. The sixteen interleaved
/// spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class SciserInstructionProgramDefinitions
{
    /// <summary><c>InstList_Sciser_UpsideRight_0</c> at $A3:967B.</summary>
    internal const ushort UpsideRight = 0x967b;
    /// <summary><c>InstList_Sciser_UpsideLeft_0</c> at $A3:9693.</summary>
    internal const ushort UpsideLeft = 0x9693;
    /// <summary><c>InstList_Sciser_UpsideDown_0</c> at $A3:96AB.</summary>
    internal const ushort UpsideDown = 0x96ab;
    /// <summary><c>InstList_Sciser_UpsideUp_0</c> at $A3:96C3.</summary>
    internal const ushort UpsideUp = 0x96c3;

    /// <summary>Number of independently addressed Sciser surface animation loops in this program family.</summary>
    internal const int SurfaceCount = 4;

    /// <summary>Byte span occupied by one surface loop, including its control words and spritemap operands.</summary>
    internal const int ProgramBytes = 24;

    /// <summary>True only for a spritemap operand in one of the four native loops.</summary>
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - UpsideRight;
        int within = offset % ProgramBytes;
        return (uint)offset < SurfaceCount * ProgramBytes && within is >= 6 and <= 18 && within % 4 == 2;
    }
    /// <summary>Resolves a control or timing word from one of Sciser's four compiled surface loops.</summary>
    /// <param name="address">The bank-$A3 address of the candidate instruction word.</param>
    /// <returns>The mechanics value encoded at that address.</returns>
    /// <exception cref="InvalidDataException">The address is outside the compiled Sciser programs or selects no mechanics word.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - UpsideRight;
        if ((uint)offset < SurfaceCount * ProgramBytes)
        {
            int surface = offset / ProgramBytes;
            switch (offset % ProgramBytes)
            {
                case 0: return EnemyInstructionCodePointers.Instruction_Crawlers_FunctionInY;
                case 2: return (ushort)(surface < 2 ? CrawlerEnemyFunction.CrawlingVertically : CrawlerEnemyFunction.CrawlingHorizontally);
                case 4: case 8: case 12: case 16: return 8;
                case 20: return CommonEnemyInstructionCodes.Goto;
                case 22: return (ushort)(UpsideRight + surface * ProgramBytes + 4);
            }
        }
        throw new InvalidDataException($"Sciser instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}

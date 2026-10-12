namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the growing shutter's four height programs.
/// Their interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class GrowingShutterInstructionProgramDefinitions
{
    /// <summary><c>InstructionList_ShutterGrowing_10px</c> at $A2:E998.</summary>
    internal const ushort TenPixels = 0xe998;

    /// <summary><c>InstructionList_ShutterGrowing_20px</c> at $A2:E99E.</summary>
    internal const ushort TwentyPixels = 0xe99e;

    /// <summary><c>InstructionList_ShutterGrowing_30px</c> at $A2:E9A4.</summary>
    internal const ushort ThirtyPixels = 0xe9a4;

    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    internal const ushort FortyPixels = 0xe9aa;

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (TenPixels + 2);
        return (uint)offset < 24 && offset % 6 == 0;
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - TenPixels;
        if ((uint)offset < 24)
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return (ushort)CommonEnemyInstruction.Sleep;
        }
        throw new InvalidDataException(
            $"Growing-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

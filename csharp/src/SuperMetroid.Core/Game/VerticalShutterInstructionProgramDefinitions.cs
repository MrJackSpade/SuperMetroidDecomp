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

    internal static bool IsKamerPresentationWord(ushort address)
    {
        int offset = address - (KamerPlatform + 2);
        return (uint)offset < 16 && offset % 4 == 0;
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Plain) return 1;
        if (address == Plain + 4) return (ushort)CommonEnemyInstruction.Sleep;
        int offset = address - KamerPlatform;
        if ((uint)offset < 16 && offset % 4 == 0) return 10;
        if (offset == 16) return (ushort)CommonEnemyInstruction.Goto;
        if (offset == 18) return KamerPlatform;
        throw new InvalidDataException(
            $"Vertical-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}

namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for plain vertical shutters and Kamer platforms.
/// Their interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class VerticalShutterInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    internal const ushort Plain = 0xe9aa;

    /// <summary><c>InstructionList_KamerPlatform</c> at $A2:EDE7.</summary>
    internal const ushort KamerPlatform = 0xede7;

    public static int MechanicsWordCount => 8;
    public static int PresentationWordCount => 5;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int frame = index - 2;
        ushort address = (ushort)(index < 2 ? Plain + 4 * index
            : KamerPlatform + (frame < 4 ? 4 * frame : 16 + 2 * (frame - 4)));
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index == 0 ? Plain + 2 : KamerPlatform + 2 + 4 * (index - 1));
    }

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
        if (address == Plain + 4) return CommonEnemyInstructionCodes.Sleep;
        int offset = address - KamerPlatform;
        if ((uint)offset < 16 && offset % 4 == 0) return 10;
        if (offset == 16) return CommonEnemyInstructionCodes.Goto;
        if (offset == 18) return KamerPlatform;
        throw new InvalidDataException(
            $"Vertical-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int bankAddress = unchecked((ushort)address);
        int plainOffset = bankAddress - Plain;
        if ((uint)plainOffset < 6) return plainOffset < 2 || plainOffset >= 4;
        int kamerOffset = bankAddress - KamerPlatform;
        return (uint)kamerOffset < 20 && (kamerOffset >= 16 || kamerOffset % 4 < 2);
    }
}

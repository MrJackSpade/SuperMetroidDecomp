namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the growing shutter's four height programs.
/// Their interleaved spritemap operands select separately installed presentation data.
/// </summary>
internal abstract class GrowingShutterInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstructionList_ShutterGrowing_10px</c> at $A2:E998.</summary>
    internal const ushort TenPixels = 0xe998;

    /// <summary><c>InstructionList_ShutterGrowing_20px</c> at $A2:E99E.</summary>
    internal const ushort TwentyPixels = 0xe99e;

    /// <summary><c>InstructionList_ShutterGrowing_30px</c> at $A2:E9A4.</summary>
    internal const ushort ThirtyPixels = 0xe9a4;

    /// <summary><c>InstructionList_ShutterGrowing_40px</c> at $A2:E9AA.</summary>
    internal const ushort FortyPixels = 0xe9aa;

    public static int MechanicsWordCount => 8;
    public static int PresentationWordCount => 4;
    internal static int ProgramCount => 4;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(TenPixels + 6 * (index / 2) + 4 * (index % 2));
        return new(address, ReadMechanicsWord(address));
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(TenPixels + 6 * index + 2);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (TenPixels + 2);
        return (uint)offset < 24 && offset % 6 == 0;
    }

    /// <summary>Each growth stage owns a six-byte duration/visual/sleep program.</summary>
    internal static ushort ProgramEntryPoint(int index)
    {
        if ((uint)index >= ProgramCount) throw new ArgumentOutOfRangeException(nameof(index));
        return (ushort)(TenPixels + 6 * index);
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - TenPixels;
        if ((uint)offset < 24)
        {
            if (offset % 6 == 0) return 1;
            if (offset % 6 == 4) return CommonEnemyInstructionCodes.Sleep;
        }
        throw new InvalidDataException(
            $"Growing-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - TenPixels;
        return (uint)offset < 24 && (offset % 6 < 2 || offset % 6 >= 4);
    }
}
